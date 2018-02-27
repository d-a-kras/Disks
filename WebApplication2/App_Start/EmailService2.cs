using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
namespace WebApplication2.App_Start
{
    public class EmailService2
    {
        

        public static async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Администрация сайта", Const.login));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (MailKit.Net.Smtp.SmtpClient client = new SmtpClient())
            {
                await client.ConnectAsync(Const.server, 25, false);
               // await client.AuthenticateAsync(Const.login, Const.password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }

        public static async Task SendEmail(string email, string number)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Администрация сайта", Const.login));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = "Кража дисков";
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = "Была зарегистрирована проверка украденных дисков, с указанием номера владельца "+ number+ ". Необходимо провести проверку."
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(Const.server, 25, false);
                await client.AuthenticateAsync(Const.login, Const.password);
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }
    }
}