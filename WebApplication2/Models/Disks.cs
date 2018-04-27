using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Microsoft.AspNet.Identity;

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
        public bool? Vip { get; set; }
        public string Folder { get; set; }
        public string Marka { get; set; }
        public DateTime Data { get; set; }
        public bool? Optom { get; set; }
        public String OldOwner { get; set; }
        public bool? EditNow { get; set; }


        public Disks(string iu) {
            this.IdUser = iu;
            Code = "";
            LOT = false;
            SendMVD = false;
            Paid = false;
            Folder = "temp";

        }
        public Disks()
        {
            this.IdUser = "";
            Code = "";
            LOT = false;
            SendMVD = false;
            Paid = false;
            Folder = "temp";

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