using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Benefit.Common.Constants;
using Benefit.Common.Helpers;
using Benefit.DataTransfer.ViewModels;
using Benefit.Web.Models.Admin;
using Microsoft.AspNet.Identity;

namespace Benefit.Services
{
    public class EmailService : IIdentityMessageService
    {
        private const string DisplayName = "Benefit Company";
        public const string BenefitBusinessEmail = "benefitforbusiness@gmail.com";
        public const string BenefitInfoEmail = "info.benefitcompany@gmail.com";
        private const string AdminEmail = "faraon.ua@gmail.com";

        private async Task SendTemplatedEmail(string templateName, string toad, string subjectcontent, params string[] parameters)
        {
            string body;
            var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
            var filePath = Path.Combine(originalDirectory, "EmailTemplates", templateName + ".html");
            using (var sr = new StreamReader(filePath))
            {
                body = sr.ReadToEnd();
            }

            string messageBody = string.Format(body, parameters);
            var usermail = Mailbodplain(new List<string> { toad }, messageBody, DisplayName, subjectcontent);
            var client = new SmtpClient() { EnableSsl = true, DeliveryMethod = SmtpDeliveryMethod.Network };

            client.Send(usermail);
        }

        public async Task<bool> SendEmail(string toad, string body, string subjectcontent)
        {
            var usermail = Mailbodplain(new List<string> { toad }, body, DisplayName, subjectcontent);
            usermail.IsBodyHtml = true;
            var client = new SmtpClient() { EnableSsl = true, DeliveryMethod = SmtpDeliveryMethod.Network };

            client.Send(usermail);
            return true;
        }

        private bool SendEmail(string toad, string body, string subjectcontent, HttpPostedFileBase attachment)
        {
            var usermail = Mailbodplain(new List<string> { toad }, body, DisplayName, subjectcontent);
            var attachmentFile = new Attachment(attachment.InputStream, attachment.FileName);
            usermail.Attachments.Add(attachmentFile);
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
            mail.SubjectEncoding = Encoding.UTF8;
            mail.Body = body;
            mail.BodyEncoding = Encoding.UTF8;
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.High;
            return mail;
        }

        public void SendUserFeedback(string subject, string message, string userUrl)
        {
            var body =
                string.Format(
                    "<div>Від: <a href='{0}'>{1}</a></div>" +
                    "<h2>{2}</h2>" +
                    "<div>{3}</div>",
                    userUrl,
                    HttpUtility.UrlDecode(HttpContext.Current.Request.Cookies[RouteConstants.FullNameCookieName].Value),
                    subject, message);
            SendEmail(BenefitInfoEmail, body, "Звернення партнера");
        }

        public void PasswordChanged(string userEmail, string newPassword)
        {
            SendTemplatedEmail("PasswordChanged", userEmail, "Ваш пароль було змінено", new[] { newPassword });
        }

        public void SendPricesImportResults(string userEmail, int processedProductsCount)
        {
            SendEmail(userEmail, "Оновлено цін для товарів: " + processedProductsCount.ToString(),
                "Результати імпорту цін товарів");
        }

        public void SendCardVerification(string userUrl, HttpPostedFileBase file)
        {
            var body = string.Format("Підтвердити верифікацію користувача можна за посиланням <a href='{0}'>ТИЦЬ!</a>", userUrl);
            SendEmail(BenefitBusinessEmail, body, "Верифікація карти", file);
        }

        public void SendProfileChangeRequest(string userUrl, string message)
        {
            var body = string.Format("{0}, <br/>Змінити дані користувача можна за посиланням <a href='{1}'>ТИЦЬ!</a>", message, userUrl);
            SendEmail(BenefitInfoEmail, body, "Запит на зміну даних партнера");
        }

        public void SendSellerBlockAlert(string email, int days)
        {
            var body = string.Format("У Вас борг за рахунком, магазин буде заблокований через {0} днів(я)", days);
            SendEmail(email, body, "Попередження про блокування магазину");
        }

        public void SendBonusesRozrahunokResults(string result)
        {
            SendEmail(BenefitInfoEmail, result, "Результати розрахунку бонусів");
        }
        public void SendImportResults(string userEmail, ProductImportResults importResults)
        {
            SendTemplatedEmail("SendImportResults", userEmail,
                "Результати імпорту",
                new[] { importResults.ProductsAdded.ToString(), importResults.ProductsUpdated.ToString(), importResults.ProductsRemoved.ToString() });
        }

        public void SendSellerApplication(AnketaViewModel sellerApplication)
        {
            var body =
                string.Format(
                    "Вид діяльності: {0} </br> " +
                    "Інтернет магазин: {1} </br> " +
                    "Пакет розміщення: {2} </br>" +
                    "ПІБ контактної особи: {3} </br>" +
                    "Назва компанії або ФОП: {4}</br>" +
                    "Телефон: {5}</br>" +
                    "Email: {6}</br>" +
                    "Коментар: {7}</br>" +
                    "Пакет: {8} </br> ",
                    sellerApplication.BusinessType,
                    sellerApplication.HasEcommerceWebSite ? sellerApplication.WebSiteAddress : "немає",
                    Enumerations.GetEnumDescription(sellerApplication.Status),
                    sellerApplication.FullName,
                    sellerApplication.CompanyName,
                    sellerApplication.Phone,
                    sellerApplication.Email,
                    sellerApplication.Comment,
                    Enumerations.GetEnumDescription(sellerApplication.Status));
            SendEmail(BenefitBusinessEmail, body, "Нова заявка постачальника");
        }

        public Task SendAsync(IdentityMessage message)
        {
            return Task.Run(() => SendEmail(message.Destination, message.Body, message.Subject));
        }
    }
}