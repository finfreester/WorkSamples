using System.Net;

namespace CodeBridgeSoftware.Infrastructure.Email
{
    public interface IEmailer
    {
        string emailServer { get; set; }
        int emailServerPort { get; set; }
        string from { get; set; }
        string to { get; set; }
        EmailConstants.enumMailPriority priority { get; set; }
        ICredentialsByHost emailAuthenticationCredentials { get; set; }

        void Send(string subject,
                  string body,
                  string to = null,
                  string cc = null,
                  string bcc = null,
                  bool isHTML = false,
                  string[] fileAttachments = null,
                  SecurityProtocolType espSecurityProtocol = SecurityProtocolType.Tls);
    }
}