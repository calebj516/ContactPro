﻿using ConnectPro.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace ConnectPro.Services
{
    public class EmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailSender = _mailSettings.Email ?? Environment.GetEnvironmentVariable("Email");

            MimeMessage newEmail = new();

            newEmail.Sender = MailboxAddress.Parse(emailSender);

            // There could be more than one recipient, hence the loop
            foreach (var emailAddress in email.Split(";"))
            {
                newEmail.To.Add(MailboxAddress.Parse(emailAddress));
            }

            newEmail.Subject = subject;

            BodyBuilder emailBody = new();
            emailBody.HtmlBody = htmlMessage;

            newEmail.Body = emailBody.ToMessageBody();

            // At this point lets log in to our smtp client!
            using SmtpClient smtpClient = new();

            try
            {
                var host = _mailSettings.MailHost ?? Environment.GetEnvironmentVariable("MailHost");
                var port = _mailSettings.MailPort != 0 ? _mailSettings.MailPort : int.Parse(Environment.GetEnvironmentVariable("MailPort"));
                var password = _mailSettings.MailPassword ?? Environment.GetEnvironmentVariable("MailPassword");

                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(emailSender, password);

                await smtpClient.SendAsync(newEmail);
                await smtpClient.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }
        }
    }
}
