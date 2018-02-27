using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;
using WebApplication2.Filters;
using System.Data.Entity;
using WebApplication2.App_Start;
using ImageResizer;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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

        [auth]
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

            var b = db.ListDisks.Where(t=>(t.Folder=="temp")&&(t.IdUser==User.Identity.Name)).ToList();
            if (b.Count > 0)
            {
                foreach (Disks d in b) {
                    db.ListDisks.Remove(d);
                }
                db.SaveChanges();
            }
            Disks disks = new Disks(User.Identity.Name);
            db.ListDisks.Add(disks);
            db.SaveChanges();
            Order order=new Order();
            try
            {
                disks = db.ListDisks.Single(t => (t.Folder == "temp")&&(t.IdUser==User.Identity.Name));
                order = new Order(disks.Id, 1, disks.IdUser);
                
            }
            catch{

            }
            

            return View(order);
        }

        [Authorize]
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

        [auth]
        public ActionResult ListDisks()
        {
            var mylistDisks = db.ListDisks.Where(a => a.IdUser == User.Identity.Name).ToList();
            if (mylistDisks.Count <= 0)
            {
                return HttpNotFound();
            }
            return View(mylistDisks);
            
        }

        [auth]
        public async Task<ActionResult> TheftDisks( int id )
        {
            var disks = db.ListDisks.Where(m => m.Id == id).Single();
            if (disks!=null) {
                disks.LOT = true;
                await EmailService2.SendEmailAsync(disks.IdUser, "Кража дисков", "Вы сообщили о краже дисков с уникальным кодом:" + disks.Code.ToString());
                disks.SendMVD = true;
                db.Entry(disks).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("ListDisks");
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
            Disks disks = new Disks(User.Identity.Name);
            Order order = new Order(disks.Id, 1, disks.IdUser);

           
           
            return View(order);
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
                        htmltext += "<img  src='../../Picture/" + @User.Identity.Name + "/temp/" + i + ".png'  style='margin: 10px' width='100' height='100' >";
                    }
                   /* if (countpicture == 4)
                    {
                        htmltext += "<form method='post' action='Pay2'> <input type='submit' value='Перейти к оплате'></form>";
                    }*/
                    
                    
                  
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


       
        public ActionResult Pay()
        {
            Disks disks = new Disks(User.Identity.Name);

            Order order = new Order(disks.Id, 1, disks.IdUser);

          

           
            return PartialView(order);
            
            
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
            
            bool vkod = false;
            string key = "Ij4ELfvLuhA32IhYXPORmxiD"; // секретный код
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
                for(int i=0;i<60;i++){
                    if (!Kod.close) {
                        Kod.close = true;
                        disks.Code = Kod.GetKode();
                        Kod.close = false;
                        vkod = true;
                        break;
                    }
                    Thread.Sleep(1000);
                }

                if (!vkod) {
                    disks.Code = Kod.GetKode();
                    Kod.close = false;
                    vkod = true;
                }

               

                string currentFolderName = Server.MapPath("~/Picture/" + User.Identity.Name);
                DirectoryInfo drInfo = new DirectoryInfo(currentFolderName);
                DirectoryInfo nf;
                if (drInfo.Exists)
                {
                    List<DirectoryInfo> di = drInfo.GetDirectories().ToList();
                    nf=drInfo.CreateSubdirectory(di.Count.ToString());

                }
                else {
                   var path = "~/Picture/" ;
                    DirectoryInfo Dir = new DirectoryInfo(Request.MapPath(path));
                    Dir.CreateSubdirectory(User.Identity.Name);
                    path= "~/Picture/" + User.Identity.Name;
                    Dir = new DirectoryInfo(Request.MapPath(path));
                    nf=Dir.CreateSubdirectory("1");
                    //log
                }

                try
                {
                    string[] files = Directory.GetFiles(Server.MapPath("~/Picture/" + User.Identity.Name+"/temp/"));
                    foreach (string srcFilePath in files)
                    {
                        System.IO.File.Move(srcFilePath, nf.ToString());
                    }
                }
                catch (Exception e)
                {
                    //log
                    Console.WriteLine("Ошибка перемещения файла(ов): " + e.Message);
                }

                disks.Folder = nf.ToString();
                disks.Paid = true;
                db.Entry(disks).State = EntityState.Modified;
                db.SaveChanges();
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