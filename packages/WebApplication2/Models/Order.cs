using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace WebApplication2.Models
{
    public class Order
    {
        public int Id { get; set; } // id заказа
        public int IdDisks { get; set; } // id заказа
        public DateTime? Date { get; set; } // дата
        public double Sum { get; set; } // сумма заказа
        public String Sender { get; set; } // отправитель - кошелек в ЯД
        public String Operation_Id { get; set; } // id операции в ЯД
        public double? Amount { get; set; } // сумма, которую заплатали с учетом комиссии
        public double? WithdrawAmount { get; set; } // сумма, которую заплатали без учета комиссии
        public string UserId { get; set; } // id пользователя в системе, который сделал заказ
        public int CountDisks { get; set; } // id пользователя в системе, который сделал заказ

        public Order(int idd, float s,string ui, int CD) {
            this.IdDisks = idd;
           // this.Date = DateTime.Now;
            this.Sum = s;
            this.UserId = ui;
            this.CountDisks = CD;
        }
        public Order() {

        }
    }

    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class StoreContext : DbContext
    {
        public StoreContext() : base("conn")
        {
        }
        public DbSet<Order> Orders { get; set; }
    }

    
}