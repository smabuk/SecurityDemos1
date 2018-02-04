using Microsoft.Extensions.Options;
using SecurityDemo1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SecurityDemo1.Services
{
    public class EmailConfiguration
    {
        public string Company { get; set; } = "";
        public string FromAddress { get; set; } = "";
        public string FromName { get; set; } = "";
        public string Password { get; set; } = "";
        public string Server { get; set; } = "localhost";
    }

    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _email;

        public EmailSender(IOptions<EmailConfiguration> EmailConfig)
        {
            _email = EmailConfig.Value;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            string MyEmailAddress = _email.FromAddress;
            string MyEmailPassword = _email.Password;

            MailAddress fromAddress = new MailAddress(MyEmailAddress, _email.FromName);
            MailAddress toAddress = new MailAddress(email);
            MailMessage mail = new MailMessage(fromAddress, toAddress)
            {
                SubjectEncoding = System.Text.Encoding.Unicode,
                BodyEncoding = System.Text.Encoding.Unicode,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };

            mail.Headers.Add("X-Company", _email.Company);
            mail.Headers.Add("X-Location", _email.FromName);

            // Request a Delivery Receipt
            mail.Headers.Add("Return-Receipt-To", $"<{_email.FromAddress}>");

            // Request a Read receipt
            mail.Headers.Add("Disposition-Notification-To", $"<{_email.FromAddress}>");

            mail.Subject = subject;
            mail.Body = message;

            SmtpClient client = new SmtpClient
            {
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(userName: MyEmailAddress, password: MyEmailPassword),
                Port = 587,
                Host = _email.Server,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = false
            };

            client.Send(mail);

            mail.Dispose();

            return Task.CompletedTask;
        }
    }
}

