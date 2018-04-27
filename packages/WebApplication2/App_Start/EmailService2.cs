using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MimeKit;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace WebApplication2.App_Start

{
    public class EmailService2
    {

        public static async Task SendEmailAsync(string toMail, string subject, string body)
        {
            MailAddress from = new MailAddress(Const.from);
            MailAddress to = new MailAddress(toMail);
            MailMessage m = new MailMessage(from, to);
            m.Subject = subject;
            m.Body = body;
            SmtpClient smtp = new SmtpClient(Const.server, 25);
           // smtp.Credentials = new NetworkCredential("somemail@gmail.com", "mypassword");
            smtp.EnableSsl = false;
            await smtp.SendMailAsync(m);
            Console.WriteLine("Письмо отправлено");
        }

        public static void SendEmail(string toMail, string subject, string body)
        {
            MailAddress from = new MailAddress(Const.from);
            MailAddress to = new MailAddress(toMail);
            MailMessage m = new MailMessage(from, to);
            m.Subject = subject;
            m.Body = body;
            SmtpClient smtp = new SmtpClient(Const.server, 25);
            // smtp.Credentials = new NetworkCredential("somemail@gmail.com", "mypassword");
            smtp.EnableSsl = false;
            smtp.SendMailAsync(m);
            Console.WriteLine("Письмо отправлено");
        }



    }
}