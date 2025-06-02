using MailKit.Net.Smtp;
using MimeKit;

namespace cce106_palit.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("PALIT", _configuration["SmtpSettings:Username"]));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            email.Body = new TextPart("html") { Text = message };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_configuration["SmtpSettings:Host"],
                                    int.Parse(_configuration["SmtpSettings:Port"]),
                                    MailKit.Security.SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(_configuration["SmtpSettings:Username"],
                                         _configuration["SmtpSettings:Password"]);

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);


        }
    }
}
