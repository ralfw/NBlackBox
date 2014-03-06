using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using nblackbox.contract;
using nblackbox.internals;
using nblackbox.internals.sqlite;

namespace nblackbox
{
    public class SQliteBlackBox : IBlackBox
    {
        private readonly string connectionString;

        public SQliteBlackBox(string filename)
        {
            connectionString = String.Format("Data Source={0}", filename);
            if (!File.Exists(filename)) InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS events (
                                                sequencenumber INTEGER PRIMARY KEY AUTOINCREMENT,
                                                id VARCHAR,
                                                timestamp DATETIME,
                                                name VARCHAR,
                                                context VARCHAR,
                                                data VARCHAR
                                                );
                                            CREATE INDEX IF NOT EXISTS idx_name on events (name ASC);
                                            CREATE INDEX IF NOT EXISTS idx_context on events (context ASC);
                                            CREATE UNIQUE INDEX IF NOT EXISTS idx_id on events (id ASC)";
                    command.ExecuteNonQuery();
                }
            }
        }


        public void Record(string name, string context, string data) { Record(new Event(name, context, data)); }
        public void Record(IEvent @event) { Record(new[] {@event}); }
        public void Record(IEnumerable<IEvent> events)
        {
            var recordedEvents = new List<RecordedEvent>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"INSERT INTO events (id, timestamp, name, context, data) VALUES(@id, @timestamp,@name,@context,@data)";
                        command.Prepare();

                        foreach (var @event in events)
                        {
                            var id = Guid.NewGuid();
                            var timestamp = DateTime.Now.ToUniversalTime();
                            command.Parameters.AddWithValue("@id", id.ToString());
                            command.Parameters.AddWithValue("@timestamp", timestamp);
                            command.Parameters.AddWithValue("@name", @event.Name);
                            command.Parameters.AddWithValue("@context", @event.Context);
                            command.Parameters.AddWithValue("@data", @event.Data);

                            command.ExecuteNonQuery();
                            var sequencenumber = connection.LastInsertRowId.ToSequenceNumber();

                            recordedEvents.Add(new RecordedEvent(id, timestamp, sequencenumber, @event.Name, @event.Context, @event.Data));
                        }
                    }
                    transaction.Commit();
                }
            }

            recordedEvents.ForEach(OnRecorded);
        }


        public IBlackBoxPlayer Player { get { return new Player(connectionString); } }

        public event Action<IRecordedEvent> OnRecorded = _ => { };

        public void Dispose() {}
    }
}
