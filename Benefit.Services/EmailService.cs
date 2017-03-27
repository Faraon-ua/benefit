﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Benefit.Common.Constants;
using Benefit.DataTransfer.ViewModels;
using Benefit.Web.Models.Admin;
using Microsoft.AspNet.Identity;

namespace Benefit.Services
{
    public class EmailService : IIdentityMessageService
    {
        private const string DisplayName = "Benefit Company";
        private const string BenefitBusinessEmail = "benefitforbusiness@gmail.com";
        private const string BenefitInfoEmail = "info.benefitcompany@gmail.com";
        private const string AdminEmail = "faraon.ua@gmail.com";

        private void SendTemplatedEmail(string templateName, string toad, string subjectcontent, params string[] parameters)
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

        private bool SendEmail(string toad, string body, string subjectcontent)
        {
            var usermail = Mailbodplain(new List<string> { toad }, body, DisplayName, subjectcontent);
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

        public void SendLoginFailed(string email, string accontName)
        {
            var body = new StringBuilder();
            body.Append(string.Format("Не удалось залогинится на сайт caribbeanbridge.com под логином {0} с реквизитами, которые вы указали, пожалуйста проверте правильность ввода логина и пароля.", accontName));
            SendEmail(email, body.ToString(), string.Format("[{0}] Login failed to caribbeanbridge.com", accontName));
        }

        public void SendSellerApplication(SellerApplicationViewModel sellerApplication)
        {
            var body =
                string.Format(
                    "Назва компанії або ФОП: {0}</br>ПІБ контактної особи: {1}</br>Телефон: {2}</br>Email: {3}</br>Посилання: {4}</br>",
                    sellerApplication.CompanyName,
                    sellerApplication.FullName,
                    sellerApplication.Phone,
                    sellerApplication.Email,
                    sellerApplication.Link);
            SendEmail(BenefitBusinessEmail, body, "Нова заявка постачальника");
        }

        public Task SendAsync(IdentityMessage message)
        {
            return Task.Run(() => SendEmail(message.Destination, message.Body, message.Subject));
        }
    }
}