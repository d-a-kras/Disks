using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using WebApplication2.App_Start;

namespace WebApplication2.Models
{
    public class MainModel
    {
        public string Title { get; set; }
        public List<Disks> Items { get; set; }

        public MainModel(){
            Title = "Модель";
            Items = new List<Disks>();
        }
    }

    public class PDF
    {

        public int Id { get; set; }
        public string Name { get; set; }
       

        public PDF() {
            this.Name = "Таблица кодов и пин-кодов";
           
        }
        

    }

    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class PDFContext : DbContext
    {
        public PDFContext() : base("conn")
        { }

        public DbSet<PDF> ListPDF { get; set; }
    }
}