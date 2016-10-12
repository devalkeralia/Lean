/*
* QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
* Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
* 
*/

using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using QuantConnect.Logging;

namespace DataValidator
{
    /// <summary>
    /// Email helper class for data processors.
    /// </summary>
    public class Mail
    {
        public static string Host = "smtp.sendgrid.net";
        public static string Username = "quantconnect-server";
        public static string Password = "WEqsx2V7uDcky4rbU63HLgzE";

        private string _host;
        private string _username;
        private string _password;
        private SmtpClient _emailClient;

        /// <summary>
        /// New mail helper class.
        /// </summary>
        public Mail(string host, string username, string password)
        {
            _host = host;
            _username = username;
            _password = password;

            //Setup Email:
            _emailClient = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = host,
                Port = 25,
                EnableSsl = false,
                Credentials = new NetworkCredential(username, password),
                Timeout = 10000
            };
        }

        /// <summary>
        /// Email error message to admin
        /// </summary>
        public void Error(string subject, string message)
        {
            Email("admin@quantconnect.com", "Data Error: " + subject, message);
        }

        /// <summary>
        /// Send a rate limited email notification triggered during live trading from a user algorithm
        /// </summary>
        public void Email(string to, string subject, string message)
        {
            try
            {
                using (var mail = new MailMessage("noreply@quantconnect.com", to, subject, message))
                {
                    mail.CC.Add("michael@quantconnect.com");
                    mail.BodyEncoding = Encoding.UTF8;
                    mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                    _emailClient.Send(mail);
                }
                Log.Trace("Mail.Email(): Sending email sent: " + subject);
            }
            catch (Exception err)
            {
                Log.Error("Mail.Email(): Error sending web request: " + err.Message);
            }
        }


    }
}