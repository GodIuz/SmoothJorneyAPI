using SmoothJorneyAPI.Interfaces;
using System.Net;
using System.Net.Mail;

namespace SmoothJorneyAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string messageBody)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");

                var senderEmail = emailSettings["SenderEmail"];
                var password = emailSettings["Password"];
                var host = emailSettings["Host"];
                var port = int.Parse(emailSettings["Port"]);

                Console.WriteLine($"Attempting to send email via {host}:{port} from {senderEmail}...");

                var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage(senderEmail, toEmail, subject, messageBody)
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(mailMessage);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine("SMTP ERROR: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("INNER ERROR: " + ex.InnerException.Message);
                Console.WriteLine("--------------------------------------------------");
                throw;
            }
        }
    }
}
