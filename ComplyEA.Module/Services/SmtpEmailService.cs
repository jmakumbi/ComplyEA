using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using ComplyEA.Module.BusinessObjects.Configuration;

namespace ComplyEA.Module.Services
{
    public class SmtpEmailService : IEmailService
    {
        // Configuration keys
        private const string KEY_SMTP_HOST = "Email.Smtp.Host";
        private const string KEY_SMTP_PORT = "Email.Smtp.Port";
        private const string KEY_SMTP_USE_SSL = "Email.Smtp.UseSsl";
        private const string KEY_SMTP_USERNAME = "Email.Smtp.Username";
        private const string KEY_SMTP_PASSWORD = "Email.Smtp.Password";
        private const string KEY_FROM_ADDRESS = "Email.From.Address";
        private const string KEY_FROM_NAME = "Email.From.Name";

        private readonly IObjectSpace _objectSpace;
        private readonly SmtpConfiguration _config;

        public SmtpEmailService()
        {
            // Default constructor for DI - configuration loaded lazily
            _config = null;
        }

        public SmtpEmailService(IObjectSpace objectSpace)
        {
            _objectSpace = objectSpace;
            _config = LoadConfiguration(objectSpace);
        }

        public bool IsConfigured => _config != null && !string.IsNullOrEmpty(_config.Host) && !string.IsNullOrEmpty(_config.FromAddress);

        public async Task<EmailResult> SendEmailAsync(string to, string subject, string bodyHtml, string bodyText = null)
        {
            if (!IsConfigured)
            {
                return EmailResult.Failed("Email service is not configured. Please configure SMTP settings.");
            }

            if (string.IsNullOrEmpty(to))
            {
                return EmailResult.Failed("Recipient email address is required.");
            }

            try
            {
                using (var client = CreateSmtpClient())
                using (var message = CreateMessage(to, subject, bodyHtml, bodyText))
                {
                    await client.SendMailAsync(message);
                    return EmailResult.Succeeded();
                }
            }
            catch (SmtpException ex)
            {
                return EmailResult.Failed($"SMTP error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return EmailResult.Failed($"Failed to send email: {ex.Message}");
            }
        }

        public async Task<EmailResult> SendTestEmailAsync(string to)
        {
            var subject = "ComplyEA - Test Email";
            var bodyHtml = @"
<html>
<body>
<h2>Test Email from ComplyEA</h2>
<p>This is a test email to verify your SMTP configuration is working correctly.</p>
<p>If you received this email, your email settings are configured properly.</p>
<p><strong>Sent at:</strong> " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"</p>
</body>
</html>";
            var bodyText = "Test Email from ComplyEA\n\nThis is a test email to verify your SMTP configuration is working correctly.\n\nSent at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return await SendEmailAsync(to, subject, bodyHtml, bodyText);
        }

        private SmtpClient CreateSmtpClient()
        {
            var client = new SmtpClient(_config.Host, _config.Port)
            {
                EnableSsl = _config.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrEmpty(_config.Username))
            {
                client.Credentials = new NetworkCredential(_config.Username, _config.Password);
            }

            return client;
        }

        private MailMessage CreateMessage(string to, string subject, string bodyHtml, string bodyText)
        {
            var from = new MailAddress(_config.FromAddress, _config.FromName ?? "ComplyEA");
            var message = new MailMessage(from, new MailAddress(to))
            {
                Subject = subject,
                IsBodyHtml = true,
                Body = bodyHtml
            };

            // Add plain text alternative if provided
            if (!string.IsNullOrEmpty(bodyText))
            {
                var plainTextView = AlternateView.CreateAlternateViewFromString(bodyText, null, "text/plain");
                var htmlView = AlternateView.CreateAlternateViewFromString(bodyHtml, null, "text/html");
                message.AlternateViews.Add(plainTextView);
                message.AlternateViews.Add(htmlView);
            }

            return message;
        }

        private SmtpConfiguration LoadConfiguration(IObjectSpace objectSpace)
        {
            if (objectSpace == null)
                return null;

            var config = new SmtpConfiguration();

            config.Host = GetConfigValue(objectSpace, KEY_SMTP_HOST);
            config.Port = GetConfigIntValue(objectSpace, KEY_SMTP_PORT, 587);
            config.UseSsl = GetConfigBoolValue(objectSpace, KEY_SMTP_USE_SSL, true);
            config.Username = GetConfigValue(objectSpace, KEY_SMTP_USERNAME);
            config.Password = GetConfigValue(objectSpace, KEY_SMTP_PASSWORD);
            config.FromAddress = GetConfigValue(objectSpace, KEY_FROM_ADDRESS);
            config.FromName = GetConfigValue(objectSpace, KEY_FROM_NAME);

            return config;
        }

        private string GetConfigValue(IObjectSpace objectSpace, string key)
        {
            var config = objectSpace.FindObject<SystemConfiguration>(
                new BinaryOperator("Key", key));
            return config?.Value;
        }

        private int GetConfigIntValue(IObjectSpace objectSpace, string key, int defaultValue)
        {
            var config = objectSpace.FindObject<SystemConfiguration>(
                new BinaryOperator("Key", key));
            return config?.GetIntValue(defaultValue) ?? defaultValue;
        }

        private bool GetConfigBoolValue(IObjectSpace objectSpace, string key, bool defaultValue)
        {
            var config = objectSpace.FindObject<SystemConfiguration>(
                new BinaryOperator("Key", key));
            return config?.GetBoolValue(defaultValue) ?? defaultValue;
        }

        private class SmtpConfiguration
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public bool UseSsl { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string FromAddress { get; set; }
            public string FromName { get; set; }
        }
    }
}
