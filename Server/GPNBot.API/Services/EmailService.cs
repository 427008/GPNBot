using System;
using System.Threading.Tasks;

using Serilog;

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

using GPNBot.API.Models;


namespace GPNBot.API.Services
{
    public class EmailService 
    {
        private readonly SmtpParams _smtpSettings;
        private readonly MimeMessage _emailMessage;

        public EmailService(SmtpParams smtpSettings)
        {
            try
            {
                _smtpSettings = smtpSettings ?? throw new ArgumentNullException(nameof(smtpSettings));

                _emailMessage = new MimeMessage
                {
                    From = { new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.From) }
                };
            }
            catch (ArgumentNullException ex)
            {
                Log.Error(ex, $"Error {DateTime.Now}");
            }
        }

        public async Task<bool> SendAsync(string nameRecipient, string email, string subject, string body)
        {
            try
            {
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                _emailMessage.Body = bodyBuilder.ToMessageBody();
                _emailMessage.To.Add(new MailboxAddress(nameRecipient, email));
                //_emailMessage.Bcc.Add(new MailboxAddress("копия", _smtpSettings.From));
                _emailMessage.Subject = subject;

                using var smtpClient = new SmtpClient();
                smtpClient.LocalDomain = _smtpSettings.LocalDomain;
                await smtpClient.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.Auto).ConfigureAwait(false);
                await smtpClient.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password).ConfigureAwait(false);
                await smtpClient.SendAsync(_emailMessage).ConfigureAwait(false);
                await smtpClient.DisconnectAsync(true).ConfigureAwait(false);

                return true;
            }
            catch (Exception e)
            {
                Log.Error(e, "Ошибка при отправке почты SendAsync");
                return false;
            }
        }
    }
}
