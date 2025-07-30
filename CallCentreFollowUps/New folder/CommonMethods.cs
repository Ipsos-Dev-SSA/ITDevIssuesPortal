using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Web;

namespace CallCentreFollowUps
{
    public class CommonMethods
    {
        public static string CurrentUserName
        {
            get
            {
                return GetUserPart(0);
            }
        }

        public static string CurrentUserRole
        {
            get
            {
                return GetUserPart(1);
            }
        }

        public static string CurrentUserFullName
        {
            get
            {
                return GetUserPart(2);
            }
        }

        public static string CurrentUserEmail
        {
            get
            {
                return GetUserPart(3);
            }
        }

        private static string GetUserPart(int index)
        {
            try
            {
                if (HttpContext.Current?.Request?.IsAuthenticated == true)
                {
                    var parts = HttpContext.Current.User.Identity.Name.Split('|');
                    if (parts.Length > index)
                    {
                        return parts[index];
                    }
                }
            }
            catch
            {
                // You may log this if needed
            }

            return string.Empty;
        }

        public static void SendMail(string toEmailAddress, string fromEmailAddress, string subject, string body, bool isBodyHtml, List<string> attachments = null)
        {
            try
            {
                var fromEmail = "mtn.portal@ipsos.com";
                fromEmailAddress = fromEmail;

                if (string.IsNullOrEmpty(fromEmailAddress))
                {
                    fromEmailAddress = ConfigurationManager.AppSettings["outgoingsmtpmailusername"];
                }

                if (string.IsNullOrEmpty(toEmailAddress))
                {
                    toEmailAddress = ConfigurationManager.AppSettings["outgoingsmtpmailusername"];
                }

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(fromEmailAddress),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isBodyHtml
                };

                mail.To.Add(toEmailAddress);

                // ✅ Add attachments if available
                if (attachments != null && attachments.Any())
                {
                    foreach (var filePath in attachments)
                    {
                        if (File.Exists(filePath))
                        {
                            Attachment attachment = new Attachment(filePath);
                            mail.Attachments.Add(attachment);
                        }
                    }
                }

                //// Optional: add BCC
                //mail.Bcc.Add("sphesihle.ndlovu@ipsos.com");

                SmtpClient client = new SmtpClient
                {
                    Host = ConfigurationManager.AppSettings["outgoingsmtpmailserver"],
                    Port = 25,
                    Credentials = new NetworkCredential(
                        ConfigurationManager.AppSettings["outgoingsmtpmailusername"],
                        ConfigurationManager.AppSettings["outgoingsmtpmailpassword"]
                    ),
                    EnableSsl = false // Change to true if your SMTP server requires SSL
                };

                client.Send(mail);
                mail.Dispose();
            }
            catch (Exception ex)
            {
                // Consider logging this exception
                throw new Exception("Failed to send email: " + ex.Message, ex);
            }
        }
    }
}
