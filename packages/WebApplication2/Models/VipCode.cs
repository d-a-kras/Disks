using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace WebApplication2.Models
{
    public class VipCode
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public bool Busy { get; set; }
       
    }

    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class VIPContext : DbContext
    {
        public VIPContext() : base("conn")
        {
        }
        public DbSet<VipCode> VIPCodes { get; set; }
    }

}