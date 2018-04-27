using System.Data.Entity;

namespace WebApplication2.Models
{
    public class AutoService
    {


        public int Id { get; set; }
        public string City { get; set; }
        public byte[] Picture { get; set; }
        public string Phone { get; set; }
        public string Grapf { get; set; }
        public string Site { get; set; }
        public string VK { get; set; }
        public string FaceBook { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Publish { get; set; }

        public AutoService()
        {


        }

    }


    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class AutoServiceContext : DbContext
    {
        public AutoServiceContext() : base("conn")
        { }

        public DbSet<AutoService> ListAutoServices { get; set; }
    }
}