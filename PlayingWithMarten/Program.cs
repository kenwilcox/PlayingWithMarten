using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marten;

namespace PlayingWithMarten
{
    class Program
    {
        static void Main(string[] args)
        {
            var store = DocumentStore.For("host=localhost;database=mydb;password=dummy;username=dummy");
            //using (var session = store.LightweightSession())
            //{
            //    var user = new User {FirstName = "Ken", LastName = "Wilcox", UserName = "wilcoxk", Internal = true};
            //    session.Store(user);

            //    user = new User {FirstName = "Stefan", LastName = "Nuxoll", UserName = "nuxolls", Internal = true};
            //    session.Store(user);
            //    session.SaveChanges();
            //}

            using (var session = store.OpenSession())
            {
                //var existing = session.Query<User>().Where(x => x.FirstName == "Han" && x.LastName == "Solo").Single();
                //Console.WriteLine("UserName: " + existing.UserName);
                var internalUsers = session.Query<User>().Where(x => x.Internal);
                foreach (var user in internalUsers)
                {
                    Console.WriteLine(user.UserName);
                }
            }
        }
    }

    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Internal { get; set; }
        public string UserName { get; set; }
    }
}
