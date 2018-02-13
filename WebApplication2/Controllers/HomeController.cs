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



        [HttpPost]
        public ActionResult CheckCode(string str)
        {
            if ((str == "")&&(str.Length!=10))
            {
                return HttpNotFound();
            }
            str = "QWERTYUIOP";
            int co = db.ListDisks.Count();
            Disks disks = db.ListDisks.First(m => m.Code == str);
            if (disks!=null) { }
            else { disks = new Disks(); }


            return View("~/Views/Home/Index", disks);

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
        public ActionResult Upload(HttpPostedFileBase upload)
        {
            if (upload != null)
            {
                var path = Server.MapPath("~/Files/");
                upload.InputStream.Seek(0, System.IO.SeekOrigin.Begin);

                ImageBuilder.Current.Build(
                    new ImageJob(
                        upload.InputStream,
                        path + upload.FileName,
                        new Instructions("maxwidth=600&maxheight=600"),
                        false,
                        false));
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public async Task<ActionResult> SendMessage()
        {
            EmailService emailService = new EmailService();
            await App_Start.EmailService.SendEmailAsync("somemail@mail.ru", "Тема письма", "Тест письма: тест!");
            return RedirectToAction("Index");
        }


    }
}