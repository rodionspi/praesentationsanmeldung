using System.Net;
using System.Net.Mail;

namespace praesentationsanmeldung.Services;

public class MailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<MailService> _logger;

    public MailService(IConfiguration config, ILogger<MailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<bool> SendenAsync(string an, string betreff, string text)
    {
        try
        {
            using var client = new SmtpClient(_config["Smtp:Host"], _config.GetValue<int>("Smtp:Port"));
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(_config["Smtp:Benutzer"], _config["Smtp:Passwort"]);

            using var mail = new MailMessage(_config["Smtp:Absender"]!, an, betreff, text);
            await client.SendMailAsync(mail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mail an {An} konnte nicht gesendet werden.", an);
            return false;
        }
    }
}
