using MimeKit;
using MailKit.Net.Smtp;
using MailKit;

namespace PachecoClinic.Helpers
{
    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _configuration;

        public MailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Response SendEmail(string to, string subject, string body)
        {
            string? nameFrom = _configuration["Mail:NameFrom"];
            string? from = _configuration["Mail:From"];
            string? smtp = _configuration["Mail:Smtp"];
            string? port = _configuration["Mail:Port"];
            string? password = _configuration["Mail:Password"];

            MimeMessage message = new MimeMessage();

            message.From.Add(new MailboxAddress(nameFrom, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;

            BodyBuilder bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };

            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using(var client = new SmtpClient())
                {
                    client.Connect(smtp, int.Parse(port!), false);
                    client.Authenticate(from, password);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.ToString(),
                };
            }

            return new Response
            {
                IsSuccess = true,
            };

        }
    }
}
