using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Web;
using Benefit.Web.Models.Admin;

namespace Benefit.Services
{
    public class EmailService
    {
        private const string DisplayName = "Benefit Company";

        private void SendTemplatedEmail(string templateName, string toad, string subjectcontent, params string[] parameters)
        {
            string body;
            using (var sr = new StreamReader(Path.Combine(HttpContext.Current.Server.MapPath("\\EmailTemplates") + templateName + ".html")))
            {
                body = sr.ReadToEnd();
            }

            string messageBody = string.Format(body, parameters);
            var usermail = Mailbodplain(new List<string> { toad }, messageBody, DisplayName, subjectcontent);
            var client = new SmtpClient() { EnableSsl = true, DeliveryMethod = SmtpDeliveryMethod.Network };

            client.Send(usermail);
        }

        private bool SendEmail(string toad, string body, string subjectcontent)
        {
            var usermail = Mailbodplain(new List<string> { toad }, body, DisplayName, subjectcontent);
            var client = new SmtpClient() { EnableSsl = true, DeliveryMethod = SmtpDeliveryMethod.Network };

            client.Send(usermail);
            return true;
        }
        private bool SendEmail(IEnumerable<string> toad, string body, string subjectcontent)
        {
            var usermail = Mailbodplain(toad, body, DisplayName, subjectcontent);
            var client = new SmtpClient { EnableSsl = true, DeliveryMethod = SmtpDeliveryMethod.Network };
            client.Send(usermail);
            return true;
        }

        private MailMessage Mailbodplain(IEnumerable<string> toadresses, string body, string displayName, string subjectcontent)
        {
            var mail = new MailMessage();
            var from = SettingsService.Email.From;
            foreach (var toad in toadresses)
            {
                mail.To.Add(toad);
            }
            mail.From = new MailAddress(from, displayName, System.Text.Encoding.UTF8);
            mail.Subject = subjectcontent;
            mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.Body = body;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.High;
            return mail;
        }

        public void PasswordChanged(string userEmail, string newPassword)
        {
            SendTemplatedEmail(System.Reflection.MethodInfo.GetCurrentMethod().Name, userEmail, "Ваш пароль було змінено", new[] { newPassword });
        } 
        
        public void SendImportResults(string userEmail, ProductImportResults importResults)
        {
            SendTemplatedEmail(System.Reflection.MethodInfo.GetCurrentMethod().Name, userEmail, 
                "Результати імпорту",
                new[] { string.Join("<br/>", importResults.ProcessedCategories), string.Join("<br/>", importResults.IgnoredCategories), importResults.ProductsAdded.ToString(), importResults.ProductsUpdated.ToString(), importResults.ProductsRemoved.ToString() });
        }

        public void SendLoginFailed(string email, string accontName)
        {
            var body = new StringBuilder();
            body.Append(string.Format("Не удалось залогинится на сайт caribbeanbridge.com под логином {0} с реквизитами, которые вы указали, пожалуйста проверте правильность ввода логина и пароля.", accontName));
            SendEmail(email, body.ToString(), string.Format("[{0}] Login failed to caribbeanbridge.com", accontName));
        }
    }
}