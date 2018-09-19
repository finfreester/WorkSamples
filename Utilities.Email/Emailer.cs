using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeBridgeSoftware.Infrastructure.Email
{
    public class Emailer : IEmailer
    {
        private int _port = 25;
        private EmailConstants.enumMailPriority _priority = EmailConstants.enumMailPriority.Normal;
        private ICredentialsByHost _creds = null;

        public string emailServer { get; set; }

        public int emailServerPort
        {
            get
            {
                return _port;
            }
            set
            {
                if (value > 0)
                {
                    _port = value;
                }
            }
        }

        public bool isSecure { get; set; }
        public string from { get; set; }
        public string to { get; set; }

        public EmailConstants.enumMailPriority priority
        {
            get
            {
                return _priority;
            }

            set
            {
                _priority = value;
            }
        }

        public ICredentialsByHost emailAuthenticationCredentials
        {
            get
            {
                return _creds;
            }
            set
            {
                _creds = value;
            }
        }

        private Emailer()
        {
        }

        public Emailer(string emailServer, int emailServerPort, string from)
            : this()
        {
            this.emailServer = emailServer;
            this.emailServerPort = emailServerPort;
            this.from = from;
        }

        public Emailer(string emailServer, int emailServerPort, string from, ICredentialsByHost emailAuthenticationCredentials)
            : this(emailServer, emailServerPort, from)
        {
            this.emailAuthenticationCredentials = emailAuthenticationCredentials;
        }

        public void Send(string subject,
                         string body,
                         string to = null,
                         string cc = null,
                         string bcc = null,
                         bool isHTML = false,
                         string[] fileAttachments = null,
                         SecurityProtocolType espSecurityProtocol = SecurityProtocolType.Tls)
        {
            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body) || (string.IsNullOrEmpty(to) && string.IsNullOrEmpty(this.to)))
            {
                throw new Exception("The subject, body or to parameters were not set.  These are required by the Emailer Send method.");
            }

            var msg = new MailMessage();

            if (from.Contains("<"))
                msg.From = new MailAddress(from.Split("<".ToCharArray())[1].Replace(">", String.Empty), from.Split("<".ToCharArray())[0]);
            else
                msg.From = new MailAddress(from);

            this.parseEmalAddresses((string.IsNullOrEmpty(to) ? this.to : to), msg, EmailConstants.enumEmailAddressType.To);

            if (cc != null)
                this.parseEmalAddresses(cc, msg, EmailConstants.enumEmailAddressType.CC);

            if (bcc != null)
                this.parseEmalAddresses(bcc, msg, EmailConstants.enumEmailAddressType.BCC);

            msg.Subject = subject;
            msg.Body = body;
            msg.IsBodyHtml = isHTML;
            msg.Priority = (MailPriority)_priority;
            msg.BodyEncoding = Encoding.GetEncoding("iso-8859-1");

            if (fileAttachments != null)
            {
                foreach (var f in fileAttachments)
                {
                    if (File.Exists(f))
                        msg.Attachments.Add(new Attachment(f));
                }
            }

            using (SmtpClient smtp = new SmtpClient(this.emailServer, this.emailServerPort))
            {

                smtp.ServicePoint.MaxIdleTime = System.Threading.Timeout.Infinite;
                smtp.EnableSsl = this.isSecure;

                ServicePointManager.SecurityProtocol = espSecurityProtocol;

                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = true;

                if (_creds != null)
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = _creds;
                }

                smtp.Send(msg);

            }

        }

        public static bool isEmailValid(string szEmail)
        {

            if (szEmail == null)
                return false;

            szEmail = szEmail.Trim();

            if (string.IsNullOrEmpty(szEmail))
                return false;

            return Regex.IsMatch(szEmail,
                                  @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                                  @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                                  RegexOptions.IgnoreCase);

        }

        private void parseEmalAddresses(string addresses,
                                        MailMessage msg,
                                        EmailConstants.enumEmailAddressType addressType)
        {
            char[] delimiterChars = { ';', ',' };
            string add = string.Empty;

            foreach (var address in addresses.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries))
            {

                if (address.Contains("<"))
                    add = address.Split("<".ToCharArray())[1].Replace(">", String.Empty).Trim();
                else
                    add = address;

                if (!isEmailValid(add)) continue;

                switch (addressType)
                {
                    case EmailConstants.enumEmailAddressType.To:
                        msg.To.Add(new MailAddress(add));
                        break;

                    case EmailConstants.enumEmailAddressType.CC:
                        msg.CC.Add(new MailAddress(add));
                        break;

                    case EmailConstants.enumEmailAddressType.BCC:
                        msg.Bcc.Add(new MailAddress(add));
                        break;
                }
            }
        }
    }
}