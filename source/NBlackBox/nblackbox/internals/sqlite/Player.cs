using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using nblackbox.contract;

namespace nblackbox.internals.sqlite
{
    class Player : IBlackBoxPlayer
    {
        private readonly string connectionString;

        private readonly List<IEnumerable<String>> contextConstraints = new List<IEnumerable<String>>();
        private readonly List<IEnumerable<String>> nameConstraints = new List<IEnumerable<String>>();

        private long fromSequenceNumber = 0;


        public Player(string connectionString)
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

        public IBlackBoxPlayer FromSequenceNumber(string sequencenumber)
        {
            fromSequenceNumber = long.Parse(sequencenumber);
            return this;
        }

        public IEnumerable<IRecordedEvent> Play()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    var sb = new StringBuilder("SELECT sequencenumber, timestamp, name, context, data FROM events")
                                .AppendFormat(" WHERE sequencenumber >= {0}", fromSequenceNumber);

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
                                reader.GetInt64(0).ToSequenceNumber(),
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
