using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;
using System.Data.Entity;
using WebApplication2.App_Start;
using ImageResizer;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Json;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private DisksContext db = new DisksContext();

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateDisks()
        {
            @ViewBag.CP = 0;
            
            var path = "~/Picture/"+User.Identity.Name;
            if (!Directory.Exists(Request.MapPath(path)))
            {
                DirectoryInfo Dir = new DirectoryInfo(Request.MapPath(path));
                Dir.CreateSubdirectory(@User.Identity.Name);
                path = "~/Picture/"+User.Identity.Name;
                Dir = new DirectoryInfo(Request.MapPath(path));
                Dir.CreateSubdirectory("temp");

            }
            else
            {
                path = "~/Picture/"+User.Identity.Name+"/temp";
                if (Directory.Exists(Request.MapPath(path)))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(Request.MapPath(path));

                    foreach (FileInfo file in dirInfo.GetFiles())
                    {
                        file.Delete();
                    }
                }
                else
                {
                    path = "~/Picture/" + User.Identity.Name;
                    DirectoryInfo Dir = new DirectoryInfo(Request.MapPath(path));
                    Dir.CreateSubdirectory("temp");
                    
                }
            }

            
            return View();
        }

        [HttpPost]
        public ActionResult CreateDisks(Disks disk)
        {
            db.ListDisks.Add(disk);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult CheckDisk()
        {
            return PartialView();
        }

        public ActionResult ListDisks()
        {
            var mylistDisks = db.ListDisks.Where(a => a.IdUser == User.Identity.Name).ToList();
            if (mylistDisks.Count <= 0)
            {
                
            }
            return PartialView(mylistDisks);
            
        }

        

        

        [HttpPost]
        public async Task<ActionResult> CheckDisk(string code, string number)
        {
            if ((code == "")&&(code.Length!=10))
            {
                return HttpNotFound();
            }
            // str = "QWERTYUIOP";
            int check = 0;
            //int co = db.ListDisks.Count();
            Disks disks;
            try
            {
                 disks = db.ListDisks.Where(m => m.Code == code).Single();
                if (disks.LOT)
                {
                    check = 1;
                    await EmailService2.SendEmailAsync(disks.IdUser,"","");
                }
                else
                {
                    check = 2;
                }
            }
            catch {
                check = 3;
            }
                

            ViewBag.Check = check;
            return PartialView();

        }

        [HttpPost]
        public ActionResult EditDisks(Disks disk)
        {
            db.Entry(disk).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult AutoServices()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        StoreContext dbo = new StoreContext();
        [HttpPost]
        public ActionResult Pay2()
        {
            ViewBag.Message = "Your contact page.";
            Disks disks = new Disks();
            Order order = new Order(disks.Id, 1, disks.IdUser);

            OrderModel orderModel = new OrderModel { OrderId = order.Id, Sum = order.Sum };
           
            return View(orderModel);
        }


        //...........................
        /* [HttpGet]
         public ActionResult UploadPicture( )
         {

             return PartialView();
         }*/

        [HttpPost]
        public JsonResult UploadPicture()
        {
            string htmltext = "";
            ForJson[] answer;
            int countpicture = 0;
            foreach (string file in Request.Files)
            {
                var upload = Request.Files[file];
                if (upload != null)
                {
                    // получаем имя файла
                    var path = Server.MapPath("~/Picture/" + User.Identity.Name + "/temp/");
                    countpicture = System.IO.Directory.GetFiles(path).Length + 1;
                    if (countpicture>4) {
                        if (Directory.Exists(Request.MapPath(path)))
                        {
                            DirectoryInfo dirInfo = new DirectoryInfo(Request.MapPath(path));

                            foreach (FileInfo fil in dirInfo.GetFiles())
                            {
                                fil.Delete();
                            }
                        }
                    }
                    // string fileName = System.IO.Path.GetFileName(upload.FileName);
                    // сохраняем файл в папку Files в проекте
                    //upload.SaveAs(Server.MapPath("~/Files/" + fileName));
                    upload.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                    try
                    {
                        ImageBuilder.Current.Build(
                            new ImageJob(
                                upload.InputStream,
                                path + countpicture + ".png",
                                new Instructions("maxwidth=600&maxheight=600"),
                                false,
                                false));
                    }
                    catch {  }
                    @ViewBag.CP = countpicture;

                    for (int i = 1; i <= countpicture; i++)
                    {
                        htmltext += "<img  src='../../Picture/" + @User.Identity.Name + "/temp/" + i + ".png'  width='50' height='50' >";
                    }
                    if (countpicture == 4)
                    {
                        htmltext += "<form method='post' action='Pay2'> <input type='submit' value='Перейти к оплате'></form>";
                    }
                    
                    
                  
                }
            }
            try
            {
                ForJson answer1 = new ForJson(htmltext, countpicture);
                answer = new ForJson[] { answer1 };
                return Json(answer);
            }
            catch {
                ForJson answer1 = new ForJson("", 0);
                answer = new ForJson[] { answer1 };
                return Json(answer);
            }
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase upload, string folder)
        {
            if (upload != null)
            {
                var path = Server.MapPath("~/Picture/"+User.Identity.Name+"/"+folder+"/");
                
                int countpicture=System.IO.Directory.GetFiles(path).Length+1;
                upload.InputStream.Seek(0, System.IO.SeekOrigin.Begin);

                ImageBuilder.Current.Build(
                    new ImageJob(
                        upload.InputStream,
                        path + countpicture+"png",
                        new Instructions("maxwidth=600&maxheight=600"),
                        false,
                        false));
                @ViewBag.CP = countpicture;
            }
            @ViewBag.folder = folder;
            
           
            return PartialView();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public async Task<ActionResult> SendMessage()
        {
            EmailService emailService = new EmailService();
            await App_Start.EmailService2.SendEmailAsync("somemail@mail.ru", "Тема письма", "Тест письма: тест!");
            return RedirectToAction("Index");
        }

        
        
        public ActionResult Pay(Disks disks)
        {
            db.ListDisks.Add(disks);
            db.SaveChanges();

            Order order = new Order(disks.Id,1,disks.IdUser); 
            
                OrderModel orderModel = new OrderModel { OrderId = order.Id, Sum = order.Sum };
                return PartialView(orderModel);
            
            
        }

        [HttpGet]
        public ActionResult Paid()
        {
            return View();
        }

        [HttpPost]
        public void Paid(string notification_type, string operation_id, int label, string datetime,
        decimal amount, decimal withdraw_amount, string sender, string sha1_hash, string currency, bool codepro)
        {
            string key = "xxxxxxxxxxxxxxxx"; // секретный код
                                             // проверяем хэш
            string paramString = String.Format("{0}&{1}&{2}&{3}&{4}&{5}&{6}&{7}&{8}",
                notification_type, operation_id, amount, currency, datetime, sender,
                codepro.ToString().ToLower(), key, label);
            string paramStringHash1 = GetHash(paramString);
            // создаем класс для сравнения строк
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            // если хэши идентичны, добавляем данные о заказе в бд
            if (0 == comparer.Compare(paramStringHash1, sha1_hash))
            {
                Order order = dbo.Orders.FirstOrDefault(o => o.Id == label);
                order.Operation_Id = operation_id;
                order.Date = DateTime.Now;
                order.Amount = amount;
                order.WithdrawAmount = withdraw_amount;
                order.Sender = sender;
                dbo.Entry(order).State = EntityState.Modified;
                dbo.SaveChanges();

                Disks disks = db.ListDisks.FirstOrDefault(o => o.IdOrder == order.Id);
                disks.Code = Kod.GetKode();
                disks.Paid = true;

            }
        }
        public string GetHash(string val)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] data = sha.ComputeHash(Encoding.Default.GetBytes(val));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

    }
}