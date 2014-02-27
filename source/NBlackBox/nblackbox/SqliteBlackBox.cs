using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nblackbox.contract;

namespace nblackbox
{
    using System.Data.SQLite;
    using System.IO;

    using nblackbox.internals;

    public class SqliteBlackBox : IBlackBox
    {
        private readonly string connectionString;

        public SqliteBlackBox(string filename)
        {
            connectionString = String.Format("Data Source={0}", filename);

            if (!File.Exists(filename))
            {
                this.InitializeDatabase();
            }
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(this.connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS events (
                        eventIndex INTEGER PRIMARY KEY AUTOINCREMENT,
                        timestamp DATETIME,
                        name VARCHAR,
                        context VARCHAR,
                        data VARCHAR
                        )";
                    command.ExecuteNonQuery();
                }
            }
        }


        public void Record(IEvent @event) { Record(new[] {@event}); }

        public void Record(string name, string context, string data)
        {
            Record(new BareEvent() { Name = name, Context = context, Data = data});
        }

        /// <summary>
        /// Inserts the given events in a single transaction; use this method in favour of looping in the application for preformance reasons.
        /// </summary>
        /// <param name="events"></param>
        public void Record(IEnumerable<IEvent> events)
        {
            using (var connection = new SQLiteConnection(this.connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"INSERT INTO events (timestamp, name, context, data) VALUES(@timestamp,@name,@context,@data)";
                        command.Prepare();

                        foreach (var @event in events)
                        {
                            var timestamp = DateTime.Now;
                        command.Parameters.AddWithValue("@timestamp", timestamp);
                        command.Parameters.AddWithValue("@name", @event.Name);
                        command.Parameters.AddWithValue("@context", @event.Context);
                        command.Parameters.AddWithValue("@data", @event.Data);

                        command.ExecuteNonQuery();
                        var index = connection.LastInsertRowId - 1; // NOTE: SQLite's autoincrement is one-based!

                        OnRecorded(new RecordedEvent(timestamp, index, @event.Name, @event.Context, @event.Data));

                        }
                    }

                    transaction.Commit();
                }
            }
        }

        public IBlackBoxPlayer Player
        {
            get { return new SqlitePlayer(this.connectionString); }
        }

        public event Action<IRecordedEvent> OnRecorded = _ => { };

        public void Dispose()
        {
            // nothing to do
        }

        private class BareEvent : IEvent
        {
            public string Name { get; set; }
            public string Context { get; set; }
            public string Data { get; set; }
        }

        private class SqlitePlayer : IBlackBoxPlayer
        {
            private string connectionString;

            private List<IEnumerable<String>> contextConstraints = new List<IEnumerable<String>>();
            private List<IEnumerable<String>> nameConstraints = new List<IEnumerable<String>>();

            private long startIndex = 0;

            public SqlitePlayer(string connectionString)
            {
                this.connectionString = connectionString;
            }

            public IBlackBoxPlayer WithContext(params string[] contexts)
            {
                contextConstraints.Add(contexts);
                return this;
            }

            public IBlackBoxPlayer ForEvent(params string[] eventnames)
            {
                nameConstraints.Add(eventnames);
                return this;
            }

            public IBlackBoxPlayer FromIndex(long index)
            {
                startIndex = Math.Max(startIndex, index);
                return this;
            }

            public IEnumerable<IRecordedEvent> Play()
            {
                using (var connection = new SQLiteConnection(this.connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        var sb = new StringBuilder("SELECT eventIndex, timestamp, name, context, data FROM events")
                           .AppendFormat(" WHERE eventIndex >= {0}", startIndex + 1); // NOTE: SQLite's autoincrement is one-based!

                        foreach (var nameConstraint in nameConstraints)
                        {
                            var options = String.Join(", ", nameConstraint.Select(SqliteStringEscape));
                            sb.AppendFormat(" AND name in ({0})", options);
                        }

                        foreach (var contextConstraint in contextConstraints)
                        {
                            var options = String.Join(", ", contextConstraint.Select(SqliteStringEscape));
                            sb.AppendFormat(" AND context in ({0})", options);
                        }

                        command.CommandText = sb.ToString();

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var recordedEvent = new RecordedEvent(
                                    reader.GetDateTime(1),
                                    reader.GetInt64(0) - 1, // NOTE: SQLite's autoincrement is one-based!
                                    reader.GetString(2),
                                    reader.GetString(3),
                                    reader.GetString(4));
                                yield return recordedEvent;
                            }
                        }
                    }
                }
            }

            private String SqliteStringEscape(String s)
            {
                return String.Concat("'", s, "'");
            }
        }
    }
}
