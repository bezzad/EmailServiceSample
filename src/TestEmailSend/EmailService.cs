using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestEmailSend
{
    public class EmailService
    {
        public string Sender { get; set; }
        public string Username { get; set; }
        public string SenderName { get; set; }
        public string SenderPassword { get; set; }
        public string SmtpUrl { get; set; }
        public int SmtpPort { get; set; }
        public byte[] Logo { get; set; }


        public EmailService(string username, string senderName, string sender, string pass, string url, int port, byte[] logo)
        {
            Username = username;
            SenderName = senderName;
            Sender = sender;
            SenderPassword = pass;
            SmtpUrl = url;
            SmtpPort = port;
            Logo = logo;
        }

        public async Task<bool> SendAsync(string email, string text, string subject)
        {
            var completed = true;

            try
            {
                if (string.IsNullOrEmpty(email) || !ValidateEmailAddressSyntax(email))
                    return false;

                var mailMessage = GetMailMessage(email, text, subject);
                var smtpClient = GetSmtpClient();

                smtpClient.SendCompleted += (s, e) =>
                {
                    Console.WriteLine("Email SENT DELIVERY!!!");
                    smtpClient.Dispose();
                    mailMessage.Dispose();
                };
                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"Email Send successfully to: {email}");

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                completed = false;
            }

            return completed;
        }

        public SmtpClient GetSmtpClient()
        {
            var smtpClient = new SmtpClient
            {
                Host = SmtpUrl,
                Port = SmtpPort,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };
            if (Sender != null)
                smtpClient.Credentials = new NetworkCredential(Username, SenderPassword);

            return smtpClient;
        }

        private MailMessage GetMailMessage(string receiver, string text, string subject)
        {
            // Construct the alternate body as HTML.
            var mail = new MailMessage(Sender, receiver)
            {
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8,
                From = new MailAddress($"{SenderName}<{Sender}>"),
                To = { receiver },
                Subject = $"{subject}  -  {DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("en"))}",
                Body = text.Replace(@"\n", "<br/>"),
                IsBodyHtml = true,
                Priority = MailPriority.High
            };

            var imgStream = new MemoryStream(Logo);

            var attachment = new Attachment(imgStream, "logo.png")
            {
                ContentId = "logo.png",
                TransferEncoding = TransferEncoding.Base64
            };

            mail.Attachments.Add(attachment);

            return mail;
        }

        /// <summary>
        ///     Verifies if the specified string is a correctly formatted e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address string.</param>
        /// <returns><b>true</b> if the syntax of the specified string is correct; otherwise, <b>false</b>.</returns>
        /// <remarks>
        ///     This method only checks the syntax of the e-mail address. It does NOT make any network connections and thus
        ///     does NOT actually check if this address exists and you can send e-mails there.
        ///     <note>
        ///         As this method is static, you do not need to create an instance of
        ///         <see cref="T:NotificationServices.EmailService.EmailService" /> object in order to use it.
        ///     </note>
        /// </remarks>
        /// <example>
        ///     This example returns <i>Correct syntax</i> as <i>_mike.o'neil_loves_underscores@sub-domain.travel</i> address is
        ///     complex but still has valid syntax.
        ///     <code lang="C#">
        /// using NotificationServices.EmailService;
        /// 
        /// if (EmailService.ValidateEmailAddressSyntax("_mike.o'neil_loves_underscores@sub-domain.travel"))
        /// {
        /// 	Console.WriteLine("Correct syntax");
        /// }
        /// else
        /// {
        /// 	Console.WriteLine("Wrong syntax");
        /// }
        /// </code>
        /// </example>
        public static bool ValidateEmailAddressSyntax(string email)
        {
            if (email == null)
                throw new ArgumentNullException(nameof(email));

            return Regex.IsMatch(email,
                "^(([\\w]+['\\.\\-+])+[\\w]+|([\\w]+))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z0-9]+[\\w-]*\\.)+[a-zA-Z]{2,9})$");
        }
    }
}
