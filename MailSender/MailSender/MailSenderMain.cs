using System;
using System.Linq;
using Typesafe.Mailgun;

namespace MailSender
{
    public class MailSenderMain
    {
        static void Main(string[] args)
        {
            MailModel mail = new MailModel();

            Console.Write("From: ");
            mail.From = Console.ReadLine();

            Console.Write("To: ");
            mail.To = Console.ReadLine();

            Console.Write("Subject: ");
            mail.Subject = Console.ReadLine();

            Console.Write("Body: ");
            mail.Body = Console.ReadLine();

            var mailClient = new MailgunClient(MailSenderSettings.Default.Domain, MailSenderSettings.Default.ApiKey);
            mailClient.SendMail(new System.Net.Mail.MailMessage(mail.From, mail.To)
            {
                Subject = mail.Subject,
                Body = mail.Body
            });
        }
    }
}
