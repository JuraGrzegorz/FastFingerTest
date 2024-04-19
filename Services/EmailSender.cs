using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace FastFingerTest.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_config["EmailSender:EmailAddress"], _config["EmailSender:password"])
            };
            MailMessage message = new MailMessage(_config["EmailSender:EmailAddress"], email, subject, htmlMessage);
            message.IsBodyHtml = true;

            await client.SendMailAsync(message);
        }
    }
}
