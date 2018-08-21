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
using NLog;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity.Owin;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private DisksContext db = new DisksContext();
        private PDFContext dbpdf = new PDFContext();
        private StoreContext dbo = new StoreContext();
        private AutoServiceContext dbas = new AutoServiceContext();
        private VIPContext dbvip = new VIPContext();
        private ApplicationUserManager _userManager;


        public ActionResult Index()
        {
            return View();
        }

        [auth]
        public ActionResult SuccessSellDisksPin()
        {
            return View();
        }
       

        [auth]
        public ActionResult SellDisks()
        {
            return View();
        }

        [auth]
        [HttpPost]
        public async Task<ActionResult> SellKod(string code, string email)
        {
            
            if ((code == "") && (code.Length != 10))
            {
                return HttpNotFound();
            }
           
            int check = 0;
         
            try
            {
                Disks disks;
           
                disks = db.ListDisks.Where(m => m.Code == code).Single();
                if (disks.LOT)
                {
                    check = 1;
                    if (Const.SendMVD)
                    {
                        try
                        {
                            await EmailService2.SendEmailAsync(Const.PostMvd, Const.Tema, Const.MessageMVDAС + " " + disks.Code.ToString());
                        }
                        catch { }
                    }
                }
                else if (disks.IdUser ==  User.Identity.Name )
                {
                    bool b = await CheckLogin(email);
                    if (b) {
                        check = 5;
                        disks.IdUser = email;
                        db.Entry(disks).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Home/SuccessSellDisksPin");
                    }
                    else {
                        check = 4;
                    }
                    

                }
                else
                {
                    check = 2;
                    
                }
               
            }
            catch
            {
                check = 3;
            }
            logger.Trace("check=" + check + " " + code + " " + User.Identity.Name);

            ViewBag.Check = check;
            return PartialView();

        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public async Task<bool> CheckLogin(String email)
        {
            
                var user = await UserManager.FindByNameAsync(email);
                if (user != null)
                {
                    return true;
                }
            
               
                    return false;
               
        }


        [auth]
        public ActionResult CheckCreateDisksPin()
        {

            return View();
        }

        [auth]
        public ActionResult SuccessCreateDisksPin()
        {

            return View();
        }

        [auth]
        [HttpPost]
        public ActionResult UpdateDisksWithPinSave(string id)
        {
            // logger.Trace("Save disks"+disks.Id);
            
           
                bool vkod = true;
                try
                {
                Disks disks;

                    disks = db.ListDisks.Where(m => m.Id.ToString() == id).Single();
                    disks.Paid = true;
                    disks.IdUser = User.Identity.Name;
                disks.checkpin = -1;
                     disks.Folder = disks.Id.ToString();
                    string currentFolderName = Server.MapPath("~/Picture/" + disks.Folder);
                    DirectoryInfo nf = new DirectoryInfo(currentFolderName);
                    
                    if (!nf.Exists)
                    {
                        var path = "~/Picture/" ;
                        var Dir = new DirectoryInfo(Request.MapPath(path));
                        nf = Dir.CreateSubdirectory(disks.Id.ToString());
                    }
                   

                    try
                    {
                        string[] files = Directory.GetFiles(Server.MapPath("~/Picture/" + disks.IdUser + "/temp/"), "d*");
                        int i = 1;
                        foreach (string srcFilePath in files)
                        {
                            System.IO.File.Move(srcFilePath, nf.ToString() + "/d" + i + ".png");
                            i++;
                        }
                        files = Directory.GetFiles(Server.MapPath("~/Picture/" + disks.IdUser + "/temp/"), "auto*");

                        foreach (string srcFilePath in files)
                        {
                            System.IO.File.Move(srcFilePath, nf.ToString() + "/auto.png");

                        }
                    }
                    catch (Exception e)
                    {
                        logger.Trace("Ошибка перемещения файла(ов): " + e.Message);
                    }

                    

                    db.Entry(disks).State = EntityState.Modified;
                    db.SaveChanges();


                    try
                    {
                       EmailService2.SendEmailAsync(Const.PostAdmin, "Новый код", Const.MessageAdminACD + " " + disks.Code);
                    }
                    catch
                    {
                        logger.Trace("error send mail admin");
                    }
                    try
                    {
                       EmailService2.SendEmailAsync(disks.IdUser, "Новый код", Const.MessageUser + " " + disks.Code);
                    }
                    catch
                    {
                        logger.Trace("error send mail admin");
                    }
                }
                catch (Exception ex)
                {
                    logger.Trace(ex.StackTrace);
                    
                }
            
            return RedirectToAction("SuccessCreateDisksPin");
        }

        public ActionResult CreateDisksPinRoute(int id, int pin)
        {
            Disks disks = db.ListDisks.Where(m => m.Id == id).Single();

            if ((disks == null) || (disks.pincode != pin))
            {
                return HttpNotFound();
            }
            return RedirectToAction("CreateDisksPin", "Home", disks);
        }

        [auth]
        public ActionResult CreateDisksPin(Disks disks)
        {
            

            if ((disks==null))
            {
                return HttpNotFound();
            }
            int price = 0;
            bool vip = false;
          

            @ViewBag.CP = 0;
            logger.Debug("CreateDisksPin");
            var path = "~/Picture/" + User.Identity.Name;
            if (!Directory.Exists(Request.MapPath(path)))
            {
                path = "~/Picture/";
                DirectoryInfo Dir = new DirectoryInfo(Request.MapPath(path));
                Dir.CreateSubdirectory(@User.Identity.Name);
                path = "~/Picture/" + User.Identity.Name;
                Dir = new DirectoryInfo(Request.MapPath(path));
                Dir.CreateSubdirectory("temp");

            }
            else
            {
                path = "~/Picture/" + User.Identity.Name + "/temp";
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

            
            try {
               
                disks.Data = DateTime.Now;
                disks.Vip = vip;
                disks.IdOrder = 0;
                db.Entry(disks).State = EntityState.Modified;
                db.SaveChanges();


            }
            catch(Exception ex)
            {

               
                
                    logger.Trace(ex.Message);
                    return RedirectToAction("ErrorDisks");
                    
                
            }

            
            
            return View(disks);
        }

        [HttpPost]
        public async Task<ActionResult> CheckPinAndCode(string code, string pin)
        {

            try
            {
                await EmailService2.SendEmailAsync(Const.PostAdmin, Const.Tema, Const.MessageAdmin + code);
            }
            catch { }
            if ((code == "") && (code.Length != 10))
            {
                return HttpNotFound();
            }
            // str = "QWERTYUIOP";
            int check = 0;
            //int co = db.ListDisks.Count();
            try
            {
                Disks disks;
           
                disks = db.ListDisks.Where(m => m.Code == code).Single();
                if (disks.checkpin > 0)
                {
                    disks.checkpin++;
                }
                db.Entry(disks).State = EntityState.Modified;
                db.SaveChanges();

                if (disks.LOT)
                {
                    check = 1;
                    if (Const.SendMVD)
                    {
                        try
                        {
                            await EmailService2.SendEmailAsync(Const.PostMvd, Const.Tema, Const.MessageMVDAС + " " + disks.Code.ToString());
                        }
                        catch { }
                    }
                }
                else if (disks.pincode.ToString() == pin)
                {
                    if (disks.checkpin == -1)
                    {  
                        check = 4;

                    }
                    else
                    {

                        check = 5;
                        ViewBag.IdDisks = disks.Id;
                        ViewBag.pin = disks.pincode;
                        //  return RedirectToAction("CreateDisksPin","Home", disks);
                    }

                }
                else
                {
                    check = 2;
                    
                }
               
            }
            catch
            {
                check = 3;
            }
            logger.Trace("check=" + check + " " + code + " " + User.Identity.Name);

            ViewBag.Check = check;
           
            return PartialView();

        }

        public ActionResult AdminPanelServices()
        {
            if (User.Identity.Name == Const.AdminLogin)
            {
                List<AutoService> las = new List<AutoService>();
                las = dbas.ListAutoServices.Where(x => x.Id >= 0).ToList();
                return View(las);
            }
            else
            {
                return HttpNotFound();
            }

        }

        public ActionResult AdminPanelCreatePinCode()
        {
            if (User.Identity.Name == Const.AdminLogin)
            {

                var model = new MainModel();
               
                model.Items = db.ListDisks.Where(m => m.pincode != null).ToList();
                return View(model);
                
            }
            else
            {
                return HttpNotFound();
            }

        }


        public ActionResult EditAutoService(int? id)
        {
            if ((id != null) && (User.Identity.Name == Const.AdminLogin))
            {
                AutoService autoService = dbas.ListAutoServices.Find(id);
                if (autoService != null)
                {
                    return View(autoService);
                }
            }
            else {
                return HttpNotFound();
            }

            return HttpNotFound();
        }

       

        public ActionResult CreateAutoService()
        {
            
            return View();


        }

        public ActionResult PublishServices()
        {

            return View();


        }

        [HttpPost]
        public ActionResult EditAutoService(AutoService autoS, HttpPostedFileBase Image)
        {
            if (Image != null)
            {
                byte[] imageData = null;
                // считываем переданный файл в массив байтов
                using (var binaryReader = new BinaryReader(Image.InputStream))
                {
                    imageData = binaryReader.ReadBytes(Image.ContentLength);
                }
                // установка массива байтов
                autoS.Picture = imageData;

            }
            else  {

            }
            dbas.Entry(autoS).State = EntityState.Modified;
            dbas.SaveChanges();
            return RedirectToAction("AdminPanelServices");
        }

       

        [HttpPost]
        public ActionResult CreateAutoService(AutoService autoS, HttpPostedFileBase uploadImage)
        {
            if (uploadImage != null)
            {
                byte[] imageData = null;
                // считываем переданный файл в массив байтов
                using (var binaryReader = new BinaryReader(uploadImage.InputStream))
                {
                    imageData = binaryReader.ReadBytes(uploadImage.ContentLength);
                }
                // установка массива байтов
                autoS.Picture = imageData;
                dbas.ListAutoServices.Add(autoS);
                dbas.SaveChanges();

            }
            else {
                dbas.ListAutoServices.Add(autoS);
                dbas.SaveChanges();

            }

            return RedirectToAction("PublishServices");
        }

      

        [HttpPost]
        public FileResult CreatePDF(MainModel mm)
        {
            PDF pdf = new PDF();
            //Объект документа пдф
           
               
                dbpdf.ListPDF.Add(pdf);
                dbpdf.SaveChanges();
                var ld = mm.Items;
               ld = ld.FindAll(t => t.printPDF == true);
            
                iTextSharp.text.Document doc = new iTextSharp.text.Document();

                //Создаем объект записи пдф-документа в файл
                PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~/PDF/"+pdf.Id + ".pdf"), FileMode.Create));

                //Открываем документ
                doc.Open();

                //Определение шрифта необходимо для сохранения кириллического текста
                //Иначе мы не увидим кириллический текст
                //Если мы работаем только с англоязычными текстами, то шрифт можно не указывать
                BaseFont baseFont = BaseFont.CreateFont(Server.MapPath("~/PDF/arial.ttf"), BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                iTextSharp.text.Font font = new iTextSharp.text.Font(baseFont, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL);

                //Обход по всем таблицам датасета (хотя в данном случае мы можем опустить
                //Так как в нашей бд только одна таблица)
                

                
                foreach (Disks d in ld)
                {
                    //Создаем объект таблицы и передаем в нее число столбцов таблицы из нашего датасета
                    PdfPTable table = new PdfPTable(2);

                    //Добавим в таблицу общий заголовок
                    PdfPCell cell = new PdfPCell(new Phrase("Уникальный код Skrindisk'a", font));

                    cell.Colspan = 2;
                    cell.HorizontalAlignment = 1;
                    //Убираем границу первой ячейки, чтобы были как заголовок
                    cell.Border = 0;
                    table.AddCell(cell);

                    //Сначала добавляем заголовки таблицы

                    cell = new PdfPCell(new Phrase(new Phrase("Уникальный код", font)));
                    cell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(new Phrase("Пин-код", font)));
                    cell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
                    table.AddCell(cell);

                cell = new PdfPCell(new Phrase(d.Code, font));
                cell.BackgroundColor = iTextSharp.text.BaseColor.WHITE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(d.pincode.ToString(), font));
                cell.BackgroundColor = iTextSharp.text.BaseColor.WHITE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Перед активацией кода зарегистрируйтесь в системе или войдите под существующей учетной записью.", font));

                cell.Colspan = 2;
                cell.HorizontalAlignment = 1;
                //Убираем границу первой ячейки, чтобы были как заголовок
                cell.Border = 0;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("              ", font));

                cell.Colspan = 2;
                cell.HorizontalAlignment = 1;
                //Убираем границу первой ячейки, чтобы были как заголовок
                cell.Border = 0;
                table.AddCell(cell);


                doc.Add(table);
                }
            if (ld.Count < 1)
            {
                PdfPTable table = new PdfPTable(2);

                //Добавим в таблицу общий заголовок
                PdfPCell cell = new PdfPCell(new Phrase("Уникальный код Skrindisk'a", font));

                cell.Colspan = 2;
                cell.HorizontalAlignment = 1;
                //Убираем границу первой ячейки, чтобы были как заголовок
                cell.Border = 0;
                table.AddCell(cell);

                //Сначала добавляем заголовки таблицы

                cell = new PdfPCell(new Phrase(new Phrase("Уникальный код", font)));
                cell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(new Phrase("Пин-код", font)));
                cell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                doc.Add(table);
            }
            //Закрываем документ
            doc.Close();
           
            string file_path = Server.MapPath("~/PDF/"+pdf.Id+".pdf");
            // Тип файла - content-type
            string file_type = "application/pdf";
            // Имя файла - необязательно
            string file_name = pdf.Name + ".pdf";
            return File(file_path, file_type, file_name);
            

        }

        [HttpGet]
        public ActionResult PublishAutoService(int? id) {
            if ((id != null) && (User.Identity.Name == Const.AdminLogin))
            {
                AutoService autoService = dbas.ListAutoServices.Find(id);
                if (autoService != null)
                {
                    autoService.Publish=true;
                    dbas.Entry(autoService).State = EntityState.Modified;
                    dbas.SaveChanges();
                }
            }
            else
            {
                return HttpNotFound();
            }

            return RedirectToAction("AdminPanelServices");
        }

       [HttpGet]
        public ActionResult DelAutoService(int? id)
        {
            if ((id != null) && (User.Identity.Name == Const.AdminLogin))
            {
                AutoService autoService = dbas.ListAutoServices.Find(id);
                if (autoService != null)
                {
                    dbas.ListAutoServices.Remove(autoService);
                    dbas.SaveChanges();
                }
            }
            else {
                return HttpNotFound();
            }

            return RedirectToAction("AdminPanelServices");
        }

        [auth]
        public ActionResult CreateListDisks(int? Id)
        {
            Order order = new Order();
            try
            {

                switch (Id) {
                    case 10: order.Sum = 3; break;
                    case 50: order.Sum = 4; break;
                    default: return HttpNotFound();
                }

                order.UserId = User.Identity.Name;
                order.CountDisks = (int)Id;
                dbo.Orders.Add(order);
                dbo.SaveChanges();
                if (Id!=null) {
                    ViewBag.count = Id;
                }

            }
            catch {
            }

            return View(order);
        }

        
        [auth]
        public ActionResult UpdateDisksWithPin(int idd)
        {
            int price = 0;
            bool vip = false;
            
             var path = "~/Picture/" + User.Identity.Name;
            if (!Directory.Exists(Request.MapPath(path)))
            {
                path = "~/Picture/";
                DirectoryInfo Dir = new DirectoryInfo(Request.MapPath(path));
                Dir.CreateSubdirectory(@User.Identity.Name);
                path = "~/Picture/" + User.Identity.Name;
                Dir = new DirectoryInfo(Request.MapPath(path));
                Dir.CreateSubdirectory("temp");

            }
            else
            {
                path = "~/Picture/" + User.Identity.Name + "/temp";
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

            @ViewBag.CP = 0;
            logger.Debug("UpdateDisksPin");
            Disks disks;
             try {
             disks= db.ListDisks.Single(t => t.Id==idd);
            path = "~/Picture/" + disks.Id;
                DirectoryInfo dirInfo=null;
            if (!Directory.Exists(Request.MapPath(path)))
            {
                path = "~/Picture/";
                DirectoryInfo Dir = new DirectoryInfo(Request.MapPath(path));
                    dirInfo = Dir.CreateSubdirectory(disks.Id.ToString());
                

            }else{
                  dirInfo = new DirectoryInfo(Request.MapPath(path));

                    foreach (FileInfo file in dirInfo.GetFiles())
                    {
                        file.Delete();
                    }
                
            }
                disks.Data = DateTime.Now;
                disks.Vip = vip;
                disks.Folder=dirInfo.Name.ToString();
                disks.IdOrder = 0;
                db.Entry(disks).State = EntityState.Modified;
                db.SaveChanges();


            }
            catch(Exception ex)
            {

              
                    logger.Trace(ex.Message);
                    return RedirectToAction("ErrorDisks");
                    
                
            }

            return View(disks);
        }

        [auth]
        public ActionResult CreateDisks()
        {
            int price = Const.Price1;
            bool vip = false;
          

            @ViewBag.CP = 0;
            logger.Debug("CreateDisks");
            var path = "~/Picture/" + User.Identity.Name;
            if (!Directory.Exists(Request.MapPath(path)))
            {
                path = "~/Picture/";
                DirectoryInfo Dir = new DirectoryInfo(Request.MapPath(path));
                Dir.CreateSubdirectory(@User.Identity.Name);
                path = "~/Picture/" + User.Identity.Name;
                Dir = new DirectoryInfo(Request.MapPath(path));
                Dir.CreateSubdirectory("temp");

            }
            else
            {
                path = "~/Picture/" + User.Identity.Name + "/temp";
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

            var b = db.ListDisks.Where(t => (t.Folder == "temp") && (t.IdUser == User.Identity.Name)).ToList();
            if (b.Count > 1)
            {
                foreach (Disks d in b)
                {
                    db.ListDisks.Remove(d);
                }
                db.SaveChanges();
            }
            Order order = new Order();
            try {
                Disks disks = db.ListDisks.Single(t => (t.Folder == "temp") && (t.IdUser == User.Identity.Name) && (t.Paid == false));
                disks.Data = DateTime.Now;
                disks.Vip = vip;
                order = dbo.Orders.First(o => o.Id == disks.IdOrder);
                order.Sum = price;
                order.CountDisks = 1;
                dbo.Orders.Add(order);
                dbo.SaveChanges();
                disks.IdOrder = order.Id;
                db.Entry(disks).State = EntityState.Modified;
                db.SaveChanges();


            }
            catch
            {

                Disks disks = new Disks(User.Identity.Name);
                disks.Data = DateTime.Now;
                disks.Vip = vip;
                db.ListDisks.Add(disks);
                db.SaveChanges();

                try
                {
                    disks = db.ListDisks.Single(t => (t.Folder == "temp") && (t.IdUser == User.Identity.Name));
                    order = new Order(disks.Id, 2, disks.IdUser, 1);
                    dbo.Orders.Add(order);
                    dbo.SaveChanges();
                    disks.IdOrder = order.Id;
                    db.Entry(disks).State = EntityState.Modified;
                    db.SaveChanges();
                    logger.Trace("create order");
                }
                catch (Exception ex)
                {
                    logger.Trace(ex.Message);
                    return RedirectToAction("ErrorDisks");
                    
                }
            }

            return View(order);
        }

        [auth]
        public ActionResult CreateDisksV()
        {
            int price = Const.PriceVip;
            bool vip = true;


            @ViewBag.CP = 0;
            logger.Debug("CreateDisks");
            var path = "~/Picture/" + User.Identity.Name;
            if (!Directory.Exists(Request.MapPath(path)))
            {
                path = "~/Picture/";
                DirectoryInfo Dir = new DirectoryInfo(Request.MapPath(path));
                Dir.CreateSubdirectory(@User.Identity.Name);
                path = "~/Picture/" + User.Identity.Name;
                Dir = new DirectoryInfo(Request.MapPath(path));
                Dir.CreateSubdirectory("temp");

            }
            else
            {
                path = "~/Picture/" + User.Identity.Name + "/temp";
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

            var b = db.ListDisks.Where(t => (t.Folder == "temp") && (t.IdUser == User.Identity.Name)).ToList();
            if (b.Count > 1)
            {
                foreach (Disks d in b)
                {
                    db.ListDisks.Remove(d);
                }
                db.SaveChanges();
            }
            Order order = new Order();
            try
            {
                Disks disks = db.ListDisks.Single(t => (t.Folder == "temp") && (t.IdUser == User.Identity.Name) && (t.Paid == false));
                disks.Data = DateTime.Now;
                disks.Vip = vip;
                order = dbo.Orders.First(o => o.Id == disks.IdOrder);
                order.Sum = price;
                order.CountDisks = 1;
                dbo.Orders.Add(order);
                dbo.SaveChanges();
                disks.IdOrder = order.Id;
                db.Entry(disks).State = EntityState.Modified;
                db.SaveChanges();


            }
            catch
            {

                Disks disks = new Disks(User.Identity.Name);
                disks.Data = DateTime.Now;
                disks.Vip = vip;
                db.ListDisks.Add(disks);
                db.SaveChanges();

                try
                {
                    disks = db.ListDisks.Single(t => (t.Folder == "temp") && (t.IdUser == User.Identity.Name));
                    order = new Order(disks.Id, 2, disks.IdUser, 1);
                    dbo.Orders.Add(order);
                    dbo.SaveChanges();
                    disks.IdOrder = order.Id;
                    db.Entry(disks).State = EntityState.Modified;
                    db.SaveChanges();
                    logger.Trace("create order");
                }
                catch (Exception ex)
                {
                    logger.Trace(ex.Message);
                    return RedirectToAction("ErrorDisks");

                }
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

        public ActionResult CreateCodeDisksPin()
        {

            return View();
        }
        
        public ActionResult CreateDisks2()
        {

            return View();
        }

        public ActionResult ErrorEditDisks()
        {

            return View();
        }

        public ActionResult CheckDisk()
        {
            return PartialView();
        }

        [auth]
        public ActionResult ListDisks()
        {
            
                var mylistDisks = db.ListDisks.Where(a => (a.IdUser == User.Identity.Name) && (a.Paid == true)).ToList();
                if (mylistDisks.Count < 0)
                {
                    return HttpNotFound();
                }
                return View(mylistDisks);
            
            
        }

        [auth]
        public ActionResult EditDisks(int? Id)
        {
            if(db.ListDisks.Count(o => (o.OldOwner == User.Identity.Name) && (o.EditNow == true))==0) { 
                Disks disks = new Disks();
                if (Id != null) {
                    disks = db.ListDisks.First(t => t.Id == Id);
                    disks.EditNow = true;
                    db.Entry(disks).State = EntityState.Modified;
                    db.SaveChanges();


                    var path = "~/Picture/" + User.Identity.Name;
                    if (!Directory.Exists(Request.MapPath(path)))
                    {
                        path = "~/Picture/";
                        DirectoryInfo Dir = new DirectoryInfo(Request.MapPath(path));
                        Dir.CreateSubdirectory(@User.Identity.Name);
                        path = "~/Picture/" + User.Identity.Name;
                        Dir = new DirectoryInfo(Request.MapPath(path));
                        Dir.CreateSubdirectory("o"+Id);

                    }
                    else
                    {
                        path = "~/Picture/" + User.Identity.Name + "/o"+Id;
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
                            Dir.CreateSubdirectory("o"+Id);

                        }
                    }
                }
                else {

                    return HttpNotFound();
                }
                return View(disks);
            }
            
        else{
                return RedirectToAction("ErrorEditDisks");
            }
        }

        [auth]
        public ActionResult CloseEditDisks()
        {
            if (db.ListDisks.Count(o => (o.OldOwner == User.Identity.Name) && (o.EditNow == true)) == 0)
            {
                return RedirectToAction("ListDisks");
            }

            else
            {
                List<Disks> d= db.ListDisks.Where(t=>(t.EditNow==true)&&(t.OldOwner==User.Identity.Name)).ToList();
                foreach (var di in d) {
                    di.EditNow = false;
                    db.Entry(di).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("ListDisks");
            }
        }

        [auth]
        public ActionResult CloseEditDisks2(int? Id)
        {
            if (Id != null)
            {

                Disks d = db.ListDisks.First(t => t.Id == Id);
                
                    d.EditNow = false;
                    db.Entry(d).State = EntityState.Modified;
                    db.SaveChanges();
                
                return RedirectToAction("ListDisks");
            }
            else {
                return RedirectToAction("ListDisks");
            }
            
        }


        public ActionResult PanelDoc()
        {
            var mylistDisks = db.ListDisks.Where(a => (a.Code.ToString().Length != 10) && (a.Paid == true)).ToList();
            if (User.Identity.Name != Const.AdminLogin)
            {
                return HttpNotFound();
            }
            return View(mylistDisks);

        }

        public async Task<ActionResult> Doc(int id)
        {
            //var mylistDisks = db.ListDisks.Where(a => (a.Code.ToString().Length != 10) && (a.Paid == true)).ToList();
            if (User.Identity.Name != Const.AdminLogin)
            {
                return HttpNotFound();
            }

            Disks disks = db.ListDisks.FirstOrDefault(o => o.Id == id);

            if (disks.Folder == "temp")
            {
                string currentFolderName = Server.MapPath("~/Picture/" + disks.IdUser);
                DirectoryInfo drInfo = new DirectoryInfo(currentFolderName);
                DirectoryInfo nf;
                if (drInfo.Exists)
                {
                    List<DirectoryInfo> di = drInfo.GetDirectories().ToList();
                    nf = drInfo.CreateSubdirectory((di.Count + 1).ToString());

                }
                else
                {
                    var path = "~/Picture/";
                    DirectoryInfo Dir = new DirectoryInfo(Request.MapPath(path));
                    Dir.CreateSubdirectory(disks.IdUser);
                    path = "~/Picture/" + disks.IdUser;
                    Dir = new DirectoryInfo(Request.MapPath(path));
                    nf = Dir.CreateSubdirectory("1");
                    //log
                }

                try
                {
                    string[] files = Directory.GetFiles(Server.MapPath("~/Picture/" + disks.IdUser + "/temp/"),"d*");
                    int i = 1;
                    foreach (string srcFilePath in files)
                    {
                        System.IO.File.Move(srcFilePath, nf.ToString() + "/d" +i+".png");
                        i++;
                    }
                     files = Directory.GetFiles(Server.MapPath("~/Picture/" + disks.IdUser + "/temp/"), "auto*");
                    
                    foreach (string srcFilePath in files)
                    {
                        System.IO.File.Move(srcFilePath, nf.ToString() + "/auto.png" );
                        
                    }
                }
                catch (Exception e)
                {
                    logger.Trace("Ошибка перемещения файла(ов): " + e.Message);
                }

                disks.Folder = nf.Name.ToString();

                db.Entry(disks).State = EntityState.Modified;
                db.SaveChanges();
            }

            bool vkod = false;

            for (int i = 0; i < 60; i++)
            {
                if (!Kod.close)
                {
                    Kod.close = true;
                    try
                    {
                        disks.Code = Kod.GetKode();
                        db.Entry(disks).State = EntityState.Modified;
                        db.SaveChanges();
                        logger.Trace(disks.Code);
                        Kod.close = false;
                        vkod = true;
                        break;
                    }
                    catch
                    {
                        logger.Error("Doc599");
                        Kod.close = false;
                    }

                }
                Thread.Sleep(1000);
            }


            if (!vkod)
            {
                disks.Code = Kod.GetKode();
                Kod.close = false;
                vkod = true;
            }

            try
            {
                await EmailService2.SendEmailAsync(Const.PostAdmin, "Новый код", Const.MessageAdminACD + " " + disks.Code);
            }
            catch
            {
                logger.Trace("error send mail admin");
            }
            try
            {
                await EmailService2.SendEmailAsync(disks.IdUser, "Новый код", Const.MessageUser + " " + disks.Code);
            }
            catch
            {
                logger.Trace("error send mail admin");
            }

            return RedirectToAction("PanelDoc");
        }

        [auth]
        public async Task<ActionResult> TheftDisks(int id)
        {
            var disks = db.ListDisks.Where(m => m.Id == id).Single();
            if (disks != null)
            {
                disks.LOT = true;
                try
                {
                    await EmailService2.SendEmailAsync(disks.IdUser, Const.TemaTheft, "Вы сообщили о краже дисков с уникальным кодом: " + disks.Code.ToString());
                }
                catch {

                }
                try
                {
                    await EmailService2.SendEmailAsync(Const.PostAdmin, Const.TemaTheft, Const.MessageMVDAT +" "+ disks.Code.ToString());
                }
                catch { }
                if (Const.SendMVD)
                {
                    try
                    {
                        await EmailService2.SendEmailAsync(Const.PostMvd, Const.TemaTheft, Const.MessageMVDAT +" "+ disks.Code.ToString());
                    }
                    catch { }
                }
                disks.SendMVD = true;
                db.Entry(disks).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("ListDisks");
        }

        [HttpPost]
        public ActionResult NewCodeAndPin()
        {
            
                Disks disks = new Disks(true);
                db.Entry(disks).State = EntityState.Added;
                db.SaveChanges();
                
            List<Disks> ldisks;

            ldisks = db.ListDisks.Where(m => m.pincode != null).ToList();
            return RedirectToAction("AdminPanelCreatePinCode",ldisks);
        }

        [HttpPost]
        public ActionResult PartialListCodeAndPin()
        {
            Disks disks = new Disks(true);
            db.Entry(disks).State = EntityState.Added;
            db.SaveChanges();
            List<Disks> ldisks;
          
              
           ldisks = db.ListDisks.Where(m => m.pincode != null).ToList();

            return PartialView(ldisks);


        }

       

        [HttpPost]
        public async Task<ActionResult> CheckDisk(string code, string number)
        {
            
            try
            {
                await EmailService2.SendEmailAsync(Const.PostAdmin, Const.Tema, Const.MessageAdmin + code);
            }
            catch { }
            if ((code == "") && (code.Length != 10))
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
                    if (Const.SendMVD)
                    {
                        try
                        {
                            await EmailService2.SendEmailAsync(Const.PostMvd, Const.Tema, Const.MessageMVDAС +" "+ disks.Code.ToString());
                        }
                        catch { }
                    }
                }
                else
                {
                    check = 2;
                }
            }
            catch
            {
                check = 3;
            }
            logger.Trace("check="+check+" "+code+" "+number+" "+User.Identity.Name);

            ViewBag.Check = check;
            return PartialView();

        }

       

        [HttpPost]
        public async Task<ActionResult> EditkDisk1(string code, string number)
        {
            logger.Trace("check");
            try
            {
                await EmailService2.SendEmailAsync(Const.PostAdmin, Const.Tema, Const.MessageAdmin + code);
            }
            catch { }
            if ((code == "") && (code.Length != 10))
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
                    if (Const.SendMVD)
                    {
                        try
                        {
                            await EmailService2.SendEmailAsync(Const.PostMvd, Const.Tema, Const.MessageMVDAС + " " + disks.Code.ToString());
                        }
                        catch { }
                    }
                }
                else
                {
                    check = 2;
                }
            }
            catch
            {
                check = 3;
            }


            ViewBag.Check = check;
            return PartialView();

        }

     

        [HttpPost]
        public ActionResult SaveEditDisks(Disks disk)
        {
           
          
            disk.Optom = false;
            disk.EditNow = false;
            db.Entry(disk).State = EntityState.Modified;
            db.SaveChanges();

            string currentFolderName = Server.MapPath("~/Picture/" + disk.IdUser);
            DirectoryInfo drInfo = new DirectoryInfo(currentFolderName);
            
            if (!drInfo.Exists)
            {
                string Folder = Server.MapPath("~/Picture/");
                DirectoryInfo Dir = new DirectoryInfo(Request.MapPath(Folder));
                Dir.CreateSubdirectory(disk.IdUser);
               

            }

            currentFolderName = Server.MapPath("~/Picture/" + disk.IdUser +"/"+ disk.Folder);
             drInfo = new DirectoryInfo(currentFolderName);

            if (!drInfo.Exists)
            {
                string Folder = Server.MapPath("~/Picture/" + disk.IdUser);
                DirectoryInfo Dir = new DirectoryInfo(Request.MapPath(Folder));
                Dir.CreateSubdirectory(disk.Folder);


            }



            try
            {
                string[] files = Directory.GetFiles(Server.MapPath("~/Picture/" + disk.OldOwner + "/" + disk.Folder), "d*");
                int i = 1;
                foreach (string srcFilePath in files)
                {
                    System.IO.File.Move(srcFilePath, currentFolderName + "/d" + i + ".png");
                    i++;
                }
                files = Directory.GetFiles(Server.MapPath("~/Picture/" + disk.OldOwner + "/" + disk.Folder), "auto*");

                foreach (string srcFilePath in files)
                {
                    System.IO.File.Move(srcFilePath, currentFolderName + "/auto.png");

                }
            }
            catch (Exception e)
            {
                logger.Trace("Ошибка перемещения файла(ов): " + e.Message);
            }

            return RedirectToAction("Index");
        }

        public ActionResult About()
        {
            logger.Trace("о");
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
            
            var listAutoServices = dbas.ListAutoServices.Where(a => a.Name!="").ToList();
            if (listAutoServices.Count < 0)
            {
                return HttpNotFound();
            }
            List<string> cites = new List<string>();

            int x = 0;

            foreach (AutoService autos in listAutoServices) {
                cites.Add(autos.City);
            }
            cites = cites.Distinct().ToList();
                foreach(var A in cites) {
                    ViewData[x.ToString()] = A;
                    x++;
                }
            
            ViewBag.CC = cites.Count;
            return View(listAutoServices);
            
        }

        public ActionResult AutoServices2()
        {

            
            return View();

        }

        public ActionResult ListServicesForCity(String city)
        {
           
            try
            {
                List<AutoService> listAutoServices = dbas.ListAutoServices.Where(a => (a.City == city) && (a.Publish == true)).ToList();
                if (listAutoServices.Count < 0)
                {
                    return HttpNotFound();
                }
                return PartialView(listAutoServices);
            }
            catch {
               
            }
            return HttpNotFound();
        }


        [HttpPost]
        public ActionResult Pay2()
        {
            ViewBag.Message = "Your contact page.";
            Disks disks = new Disks(User.Identity.Name);
            Order order = new Order(disks.Id, 1, disks.IdUser,1);



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
                    countpicture = System.IO.Directory.GetFiles(path,"d*").Length + 1;
                    if (countpicture > 4)
                    {
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
                                path +"d"+countpicture + ".png",
                                new Instructions("maxwidth=600&maxheight=600"),
                                false,
                                false));
                    }
                    catch { }
                    @ViewBag.CP = countpicture;

                    for (int i = 1; i <= countpicture; i++)
                    {
                        htmltext += "<img  src='../../Picture/" + @User.Identity.Name + "/temp/d" + i + ".png'  style='margin: 10px' width='100' height='100' >";
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
            catch
            {
                ForJson answer1 = new ForJson("", 0);
                answer = new ForJson[] { answer1 };
                return Json(answer);
            }
        }

        [HttpPost]
        public JsonResult UploadPictureCar()
        {

            string htmltext = "";
            ForJson[] answer;
            foreach (string file in Request.Files)
            {
                var upload = Request.Files[file];
                if (upload != null)
                {
                    // получаем имя файла
                    var path = Server.MapPath("~/Picture/" + User.Identity.Name + "/temp/");
                    
                 
                    // string fileName = System.IO.Path.GetFileName(upload.FileName);
                    // сохраняем файл в папку Files в проекте
                    //upload.SaveAs(Server.MapPath("~/Files/" + fileName));
                    upload.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                    try
                    {
                        ImageBuilder.Current.Build(
                            new ImageJob(
                                upload.InputStream,
                                path +  "auto.png",
                                new Instructions("maxwidth=600&maxheight=600"),
                                false,
                                false));
                    }
                    catch { }


                    htmltext = "<img  src='../../Picture/" + @User.Identity.Name + "/temp/auto.png'  style='margin: 10px' width='100' height='100' >";
                

            }
            }
            try
            {
                ForJson answer1 = new ForJson(htmltext, 1);
                answer = new ForJson[] { answer1 };
                return Json(answer);
            }
            catch
            {
                ForJson answer1 = new ForJson("", 0);
                answer = new ForJson[] { answer1 };
                return Json(answer);
            }

        }

        [HttpPost]
        public JsonResult UploadPictureCar2()
        {

            string htmltext = "";
            ForJson[] answer;
            foreach (string file in Request.Files)
            {
                var upload = Request.Files[file];
                if (upload != null)
                {
                    // получаем имя файла
                    Disks disks = db.ListDisks.SingleOrDefault(o => (o.OldOwner == User.Identity.Name) && (o.Optom == true) && (o.EditNow == true));
                    var path = Server.MapPath("~/Picture/" + disks.IdUser + "/" + disks.Folder + "/");


                    // string fileName = System.IO.Path.GetFileName(upload.FileName);
                    // сохраняем файл в папку Files в проекте
                    //upload.SaveAs(Server.MapPath("~/Files/" + fileName));
                    upload.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                    try
                    {
                        ImageBuilder.Current.Build(
                            new ImageJob(
                                upload.InputStream,
                                path + "auto.png",
                                new Instructions("maxwidth=600&maxheight=600"),
                                false,
                                false));
                    }
                    catch { }


                    htmltext = "<img  src='../../Picture/" + @User.Identity.Name +"/"+disks.Folder+ "/auto.png'  style='margin: 10px' width='100' height='100' >";


                }
            }
            try
            {
                ForJson answer1 = new ForJson(htmltext, 1);
                answer = new ForJson[] { answer1 };
                return Json(answer);
            }
            catch
            {
                ForJson answer1 = new ForJson("", 0);
                answer = new ForJson[] { answer1 };
                return Json(answer);
            }

        }

        [HttpPost]
        public JsonResult UploadPic()
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
                    Disks disks = db.ListDisks.SingleOrDefault(o => (o.OldOwner == User.Identity.Name)&&(o.Optom==true)&&(o.EditNow==true));
                    var path = Server.MapPath("~/Picture/" + disks.IdUser + "/"+disks.Folder+"/");
                    countpicture = System.IO.Directory.GetFiles(path, "d*").Length + 1;
                    if (countpicture > 4)
                    {
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo dirInfo = new DirectoryInfo(path);

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
                                path + "d"+countpicture + ".png",
                                new Instructions("maxwidth=600&maxheight=600"),
                                false,
                                false));
                    }
                    catch { }
                    @ViewBag.CP = countpicture;

                    for (int i = 1; i <= countpicture; i++)
                    {
                        htmltext += "<img  src='../../Picture/" + @disks.IdUser + "/"+@disks.Folder+ "/d" + i + ".png'  style='margin: 10px' width='100' height='100' >";
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
            catch
            {
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
                logger.Debug("Upload");
                var path = Server.MapPath("~/Picture/" + User.Identity.Name + "/" + folder + "/");

                int countpicture = System.IO.Directory.GetFiles(path, "d*").Length + 1;
                upload.InputStream.Seek(0, System.IO.SeekOrigin.Begin);

                ImageBuilder.Current.Build(
                    new ImageJob(
                        upload.InputStream,
                        path + countpicture + "png",
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





        public ActionResult Pay()
        {
            Disks disks = new Disks(User.Identity.Name);

            Order order = new Order(disks.Id, 1, disks.IdUser,1);




            return PartialView(order);


        }

        [HttpGet]
        public ActionResult Paid()
        {
            return View();
        }

        [HttpPost]
        public void SendMarka(string marka, string iddisks)
        {
            try {
                int id = int.Parse(iddisks);
                Disks disks = db.ListDisks.FirstOrDefault(o => o.Id == id);
                disks.Marka = marka;
                db.Entry(disks).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch(Exception ex){
                logger.Trace("Marka"+ex.Message);
            }
           
            

        }



        [HttpPost]
        public async Task Paid2(String label, String amount)
        {
            //   logger.Trace("label1=" + label + " amount=" + amount);
            //    if (label != null) { } else { label="1";}
            logger.Trace("label=" + label + " amount=" + amount);
            logger.Trace(int.Parse(label));
            int lo = int.Parse(label);
            Order order = dbo.Orders.SingleOrDefault(o => o.Id == lo);

            if (order.CountDisks == 1)
            {
                bool vkod = false;
                try
                {


                    // order.Operation_Id = label;
                    order.Date = DateTime.Now;
                    string result = "";
                    for (int i = 0; i < amount.Length; i++)// Проход по каждом символе в строке Sell
                    {
                        if (char.IsDigit(amount[i]) || (amount[i] == '.'))// Проверяем символ. Если символ - цифра 
                        {
                            if (amount[i] == '.') { result += ','; }
                            else
                            {
                                result += amount[i]; // то пишем её в переменную result. 
                            }
                        }
                    }
                    double am = double.Parse(result);

                    order.Amount = am;

                    //  order.WithdrawAmount = withdraw_amount;
                    // order.Sender = sender;
                    dbo.Entry(order).State = EntityState.Modified;
                    dbo.SaveChanges();

                    if (Math.Abs((double)order.Sum - (double)order.Amount) > 100)
                    {
                        logger.Trace("Ошибка в стоимости: " + order.Id );
                        return;
                    }
                    Disks disks = db.ListDisks.FirstOrDefault(o => o.IdOrder == order.Id);
                    disks.Paid = true;
                    db.Entry(disks).State = EntityState.Modified;
                    db.SaveChanges();
                    string currentFolderName = Server.MapPath("~/Picture/" + disks.Id);
                    DirectoryInfo drInfo = new DirectoryInfo(currentFolderName);
                    DirectoryInfo nf;
                    if (drInfo.Exists)
                    {


                        foreach (FileInfo file in drInfo.GetFiles())
                        {
                            file.Delete();
                        }

                    }
                    else
                    {
                        var path = "~/Picture/";
                        DirectoryInfo Dir = new DirectoryInfo(Request.MapPath(path));
                        Dir.CreateSubdirectory(disks.Id.ToString());
                    }

                    try
                    {

                        string[] files = Directory.GetFiles(Server.MapPath("~/Picture/" + disks.IdUser + "/temp/"), "d*");
                        int i = 1;
                        foreach (string srcFilePath in files)
                        {
                            System.IO.File.Move(srcFilePath, drInfo.ToString() + "/d" + i + ".png");
                            i++;
                        }
                        files = Directory.GetFiles(Server.MapPath("~/Picture/" + disks.IdUser + "/temp/"), "auto*");

                        foreach (string srcFilePath in files)
                        {
                            System.IO.File.Move(srcFilePath, drInfo.ToString() + "/auto.png");

                        }
                    }
                    catch (Exception e)
                    {
                        logger.Trace("Ошибка перемещения файла(ов): " + e.Message);
                    }

                    disks.Folder = disks.Id.ToString();

                    db.Entry(disks).State = EntityState.Modified;
                    db.SaveChanges();

                    if ((disks.Vip!=null)&&((bool)disks.Vip)) {
                       VipCode vc= dbvip.VIPCodes.First(t => t.Busy == false);
                        if (vc != null)
                        {
                            disks.Code = vc.Code;
                            db.Entry(disks).State = EntityState.Modified;
                            db.SaveChanges();
                            vc.Busy = true;
                            dbvip.Entry(vc).State = EntityState.Modified;
                            dbvip.SaveChanges();
                        }
                        else {
                            try
                            {
                                await EmailService2.SendEmailAsync(disks.IdUser, "Ошибка получения VIP кода", "Добрый день. У нас временно закончились vip коды. Обратитесь в службу поддержки сайта для получения своего кода.");
                                await EmailService2.SendEmailAsync(Const.PostAdmin, "ERROR VIP", "Вип коды закончились!");
                            }
                            catch
                            {
                                logger.Trace("error send mail admin");
                            }
                            
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 60; i++)
                        {
                            if (!Kod.close)
                            {

                                Kod.close = true;
                                try
                                {
                                    disks.Code = Kod.GetKode();
                                    db.Entry(disks).State = EntityState.Modified;
                                    db.SaveChanges();

                                    logger.Trace(disks.Code);
                                    Kod.close = false;
                                    vkod = true;
                                    break;
                                }
                                catch
                                {
                                    logger.Error("599");
                                    Kod.close = false;
                                }

                            }
                            Thread.Sleep(1000);
                        }


                        if (!vkod)
                        {
                            disks.Code = Kod.GetKode();
                            Kod.close = false;
                            vkod = true;
                        }
                    }



                    try
                    {
                        await EmailService2.SendEmailAsync(Const.PostAdmin, "Новый код", Const.MessageAdminACD + " " + disks.Code);
                    }
                    catch
                    {
                        logger.Trace("error send mail admin");
                    }
                    try
                    {
                        await EmailService2.SendEmailAsync(disks.IdUser, "Новый код", Const.MessageUser + " " + disks.Code);
                    }
                    catch
                    {
                        logger.Trace("error send mail admin");
                    }
                }
                catch (Exception ex)
                {
                    logger.Trace(ex.StackTrace);
                    Kod.close = false;
                }
            }
            else {
                string ld = "Список уникальных кодов:\n";
                for (int i=0;i< order.CountDisks;i++) {
                    Disks disks = new Disks(order.UserId);
                    disks.Optom = true;

                    for (int j = 0; j < 1000; j++)
                    {
                        if (!Kod.close)
                        {
                            Kod.close = true;
                            try
                            {
                                disks.Code = Kod.GetKode();
                                disks.IdOrder = order.Id;
                                disks.OldOwner = disks.IdUser;
                                disks.Paid = true;
                                db.ListDisks.Add(disks);
                                db.SaveChanges();
                                disks.Folder = "o" + disks.Id;
                                db.Entry(disks).State = EntityState.Modified;
                                db.SaveChanges();
                                logger.Trace(disks.Code);
                                Kod.close = false;
                                
                                break;
                            }
                            catch
                            {
                                logger.Error(order.UserId+" 895");
                                Kod.close = false;
                                ld = "Произошла ошибка 895. Платежный документ № "+order.Id ;
                            }

                        }
                        Thread.Sleep(100);
                    }
                    ld += disks.Code + " \n";
                }
                try
                {
                    await EmailService2.SendEmailAsync(Const.PostAdmin, "Новый код", Const.MessageAdminACD + "\n " + ld);
                }
                catch
                {
                    logger.Trace("error send mail admin");
                }
                try
                {
                    await EmailService2.SendEmailAsync(order.UserId, "Новый код", Const.MessageUser + " " + ld);
                }
                catch
                {
                    logger.Trace("error send mail admin");
                }
            }
        }
    }


}