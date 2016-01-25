using System;
using System.Net.Mail;

namespace DiskReporter {
   class MailValidator {
        /// <summary>
        ///  Validates a mail address
        /// </summary>
        /// <param name="emailAddress">String representing a mail address</param>
        public static bool IsValid(string emailAddress) {
            try {
                new MailAddress(emailAddress);
                return true;
            } catch (FormatException) {
                return false;
            }
        }
    }
}