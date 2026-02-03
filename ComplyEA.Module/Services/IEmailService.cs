using System.Threading.Tasks;

namespace ComplyEA.Module.Services
{
    /// <summary>
    /// Result of an email send operation.
    /// </summary>
    public class EmailResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public static EmailResult Succeeded() => new EmailResult { Success = true };
        public static EmailResult Failed(string error) => new EmailResult { Success = false, ErrorMessage = error };
    }

    /// <summary>
    /// Service for sending emails via SMTP.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="to">Recipient email address.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="bodyHtml">HTML body content.</param>
        /// <param name="bodyText">Plain text body content (optional fallback).</param>
        Task<EmailResult> SendEmailAsync(string to, string subject, string bodyHtml, string bodyText = null);

        /// <summary>
        /// Tests the email configuration by sending a test message.
        /// </summary>
        Task<EmailResult> SendTestEmailAsync(string to);

        /// <summary>
        /// Checks if email service is configured and ready to send.
        /// </summary>
        bool IsConfigured { get; }
    }
}
