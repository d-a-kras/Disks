using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace WebApplication2.Models
{
    public class Disks
    {

        public int Id { get; set; }
        public string IdUser { get; set; }
        public string Code { get; set; }
        public bool LOT { get; set; }
        public bool SendMVD { get; set; }
        public int IdOrder { get; set; }
        public bool Paid { get; set; }
        public string Folder { get; set; }

        public Disks() {
           

        }



    }

    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class DisksContext : DbContext
    {
        public DisksContext() : base("conn")
        { }

        public DbSet<Disks> ListDisks { get; set; }
    }
}