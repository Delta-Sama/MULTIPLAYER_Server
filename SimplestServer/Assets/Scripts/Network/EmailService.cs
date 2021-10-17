using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Mail;
using System.Net;

public class EmailService
{
    static public void SendEmailMessage(string recipientAddress, string subject, string passedMessage, string[] attachments = null)
    {
        MailAddress Notifier = new MailAddress("deltas.notifier@gmail.com", "Delta's Notifier");
        MailAddress Recipient = new MailAddress(recipientAddress);

        MailMessage message = new MailMessage(Notifier, Recipient);
        message.Subject = subject;
        message.Body = passedMessage;

        if (attachments != null && attachments.Length > 0)
        {
            foreach (string attachment in attachments)
            {
                message.Attachments.Add(new Attachment(attachment));
            }
        }

        SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
        smtp.Credentials = new NetworkCredential("deltas.notifier@gmail.com", "Rimskogo-Korsakova1kv19");
        smtp.EnableSsl = true;
        smtp.Send(message);
    }
}
