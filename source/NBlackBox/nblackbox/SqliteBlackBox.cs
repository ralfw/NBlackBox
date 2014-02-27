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

        public void Record(IEvent @event) { Record(@event.Name, @event.Context, @event.Data); }

        public void Record(string name, string context, string data)
        {
            using (var connection = new SQLiteConnection(this.connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        var timestamp = DateTime.Now;
                        command.CommandText = @"INSERT INTO events (timestamp, name, context, data) VALUES(@timestamp,@name,@context,@data)";
                        command.Parameters.AddWithValue("@timestamp", timestamp);
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@context", context);
                        command.Parameters.AddWithValue("@data", data);

                        command.ExecuteNonQuery();
                        var index = connection.LastInsertRowId - 1; // NOTE: SQLite's autoincrement is one-based!

                        OnRecorded(new RecordedEvent(timestamp, index, name, context, data));
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

        private class SqlitePlayer : IBlackBoxPlayer
        {
            private string connectionString;

            public SqlitePlayer(string connectionString)
            {
                this.connectionString = connectionString;
            }

            public IBlackBoxPlayer WithContext(params string[] contexts)
            {
                throw new NotImplementedException();
            }

            public IBlackBoxPlayer ForEvent(params string[] eventnames)
            {
                throw new NotImplementedException();
            }

            public IBlackBoxPlayer FromIndex(long index)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IRecordedEvent> Play()
            {
                using (var connection = new SQLiteConnection(this.connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"SELECT eventIndex, timestamp, name, context, data FROM events";

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
        }
    }
}
