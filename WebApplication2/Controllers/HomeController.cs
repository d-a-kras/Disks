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
        public ActionResult CheckDisk(string code)
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


        //...........................

        [HttpPost]
        public ActionResult UploadPicture(HttpPostedFileBase upload, int Id)
        {
            if (upload != null)
            {
                var path = Server.MapPath("~/Picture/{User.Identity.Name}");
                if (!Directory.Exists(path))
                {
                    DirectoryInfo Dir = new DirectoryInfo(Request.MapPath(path));
                    Dir.CreateSubdirectory(@User.Identity.Name);
                }
                path = Server.MapPath("~/Picture/{User.Identity.Name}/{Id}");
                if (!Directory.Exists(path))
                {
                    DirectoryInfo Dir = new DirectoryInfo(Request.MapPath(path));
                    Dir.CreateSubdirectory(Id.ToString());
                }
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
            @ViewBag.Id = Id;
           
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


    }
}