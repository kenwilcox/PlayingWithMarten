using System;
using System.Collections.Generic;
using System.Linq;
using Marten;
using Npgsql;

namespace PlayingWithMarten
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //export_ddl();

            var store = DocumentStore.For(_ =>
            {
                _.AutoCreateSchemaObjects = AutoCreate.CreateOnly; // set to none to not change your schema
                _.Schema.Include<MyMartenRegistry>();
                _.Connection("host=localhost;database=mydb;password=dummy;username=dummy");
                _.UpsertType = Marten.Schema.PostgresUpsertType.Standard;
                _.Logger(new ConsoleMartenLogger());
            });

            using (var session = store.LightweightSession())
            {
                var users = new List<User>();
                for (var i = 0; i < 100000; i++)
                {
                    var user = new User { FirstName = "Bulk", LastName = "User", UserName = "userbk", Internal = false };
                    users.Add(user);
                }
                var data = users.ToArray();
                store.BulkInsert(data);

                var count = session.Query<User>().Count(x => x.UserName.StartsWith("user"));
                Console.WriteLine("Yes! " + count);
            }

            using (var session = store.DirtyTrackedSession())
            {
                var matt = session.Query<User>().Single(x => x.UserName == "overallm");
                if (matt != null)
                {
                    matt.OtherField = "Development Lead";
                    session.SaveChanges();
                }
            }

            using (var session = store.OpenSession())
            {
                var internalUsers = session.Query<User>().Where(x => x.Internal).OrderBy(x => x.UserName);
                foreach (var user in internalUsers)
                {
                    Console.Write(user.UserName);
                    if (!string.IsNullOrEmpty(user.OtherField)) Console.Write(": " + user.OtherField);
                    Console.WriteLine("");
                }
            }
        }

        private static void export_ddl()
        {
            var store = DocumentStore.For(_ =>
            {
                _.Connection("host=localhost;database=mydb;password=dummy;username=dummy");
                _.Schema.For<User>();
            });

            //store.Schema.WriteDDL("mydb.sql");
            //store.Schema.WriteDDL("sql");
            var sql = store.Schema.ToDDL();
            Console.WriteLine(sql);
        }
    }

    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Internal { get; set; }
        public string UserName { get; set; }
        public string OtherField { get; set; }
    }

    public class MyMartenRegistry : MartenRegistry
    {
        public MyMartenRegistry()
        {
            For<User>().Searchable(x => x.UserName);
        }
    }

    public class ConsoleMartenLogger : IMartenLogger, IMartenSessionLogger
    {
        public IMartenSessionLogger StartSession(IQuerySession session)
        {
            return this;
        }

        public void SchemaChange(string sql)
        {
            Console.WriteLine("SchemaChange: Executing DDL change:");
            Console.WriteLine("SchemaChange: " + sql);
            Console.WriteLine();
        }

        public void LogSuccess(NpgsqlCommand command)
        {
            Console.WriteLine("LogSuccess: " + command.CommandText);
            Console.WriteLine();
        }

        public void LogFailure(NpgsqlCommand command, Exception ex)
        {
            Console.WriteLine("LogFailure: Postgresql command failed!");
            Console.WriteLine("LogFailure: " + command.CommandText);
            Console.WriteLine("LogFailure: " + ex);
            Console.WriteLine();
        }

        public void RecordSavedChanges(IDocumentSession session)
        {
            var lastCommit = session.LastCommit;
            Console.WriteLine($"RecordSavedChanges: Persisted {lastCommit.Updated.Count()} updates, {lastCommit.Inserted.Count()} inserts, and {lastCommit.Deleted.Count()} deletions");
        }
    }
}
