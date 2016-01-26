using System;
using System.Net.Mail;
using System.Text;
using System.Net.Mime;

namespace DiskReporter.Utilities {
    public class ndrMailMessage {
        private MailMessage mailMessage = new MailMessage();
        private SmtpClient SmtpServer;
        public ndrMailMessage(String smtpServer, String fromAddress, String toAddress) {
         SmtpServer = new SmtpClient(smtpServer);
         mailMessage.From = new MailAddress(fromAddress);
         mailMessage.To.Add(toAddress);
        }
        public ndrMailMessage(String smtpServer, String fromAddress, String[] toAddresses) {
            SmtpServer = new SmtpClient(smtpServer);
            mailMessage.From = new MailAddress(fromAddress);
            foreach (String address in toAddresses) {
                if (!String.IsNullOrEmpty(address)) mailMessage.To.Add(address);
            }
        }
        /// <summary>
        /// Add a attachment to the mail
        /// </summary>
        /// <param name="filePath">The path to the file to attach</param>
        public void addAttachment(string filePath) {
            Attachment data = new Attachment(filePath, MediaTypeNames.Application.Octet);
            ContentDisposition disposition = data.ContentDisposition;
            disposition.CreationDate = System.IO.File.GetCreationTime(filePath);
            disposition.ModificationDate = System.IO.File.GetLastWriteTime(filePath);
            disposition.ReadDate = System.IO.File.GetLastAccessTime(filePath);
            mailMessage.Attachments.Add(data);
        }
        /// <summary>
        /// Sets the SMTP server to use for sending mails
        /// </summary>
        /// <param name="smtpServer">String representing the name an relative location of xml configuration file</param>
        public void setSMTPServer(String smtpServer) {
            SmtpServer = new SmtpClient(smtpServer);
        }
        /// <summary>
        /// Sets the body of the mail to be a html based mail body
        /// </summary>
        public void setMailAsHTML() {
            mailMessage.IsBodyHtml = true;
        }
        /// <summary>
        /// Adds a mail receiver
        /// </summary>
        /// <param name="toAddress">The mail address to send to</param>
        public void addRegularToAddress(String toAddress) {
            mailMessage.To.Add(toAddress);
        }
        /// <summary>
        /// Changes what address the mail is sent from
        /// </summary>
        /// <param name="fromAddress">The address we are sending as</param>
        public void setRegularFromAddress(String fromAddress) {
            mailMessage.From = new MailAddress(fromAddress);
        }
        /// <summary>
        /// Sets the subject of the mail
        /// </summary>
        /// <param name="subject">Sets the subject of the mail</param>
        public void setRegularSubject(String subject) {
            mailMessage.Subject = subject;
        }
        /// <summary>
        /// Sets the content of the mail
        /// </summary>
        /// <param name="messageBody">The content of the mail with a string</param>
        public void setMailBody(String messageBody) {
            mailMessage.Body = messageBody;
        }
        /// <summary>
        /// Sets the content of the mail
        /// </summary>
        /// <param name="messageBody">Sets the content of the mail with a StringBuilder</param>
        public void setMailBody(StringBuilder messageBody) {
            mailMessage.Body = messageBody.ToString();
        }
        /// <summary>
        /// Sends the mail we have created with this object
        /// </summary>
        public void sendMessage() {
            try {
                SmtpServer.Send(mailMessage);
            } catch (Exception ex) {
                throw ex;
            }
        }
   }
}
