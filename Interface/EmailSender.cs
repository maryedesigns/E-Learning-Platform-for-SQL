
using System.Net.Mail;
using System.Net;

namespace E_LearningProject.Interface
{
    public class EmailSender : IEmailSender
    {
        private readonly string _smtpServer = "smtp.example.com"; // SMTP server
        private readonly int _smtpPort = 587; // SMTP port
        private readonly string _smtpUsername = "mary.erhieyovwe4christ@gmail.com"; // Your email username
        private readonly string _smtpPassword = "Pa$$w0rd"; // Your email password

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUsername),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
