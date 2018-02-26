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
        public decimal Sum { get; set; } // сумма заказа
        public string Sender { get; set; } // отправитель - кошелек в ЯД
        public string Operation_Id { get; set; } // id операции в ЯД
        public decimal? Amount { get; set; } // сумма, которую заплатали с учетом комиссии
        public decimal? WithdrawAmount { get; set; } // сумма, которую заплатали без учета комиссии
        public string UserId { get; set; } // id пользователя в системе, который сделал заказ

        public Order(int idd, decimal s,string ui) {
            this.IdDisks = idd;
            this.Date = DateTime.Now;
            this.Sum = s;
            this.UserId = ui;
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