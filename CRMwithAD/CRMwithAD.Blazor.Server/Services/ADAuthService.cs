using CRMwithAD.Blazor.Server.Configurations;
using CRMwithAD.Blazor.Server.Models.ADModels;
using Microsoft.Extensions.Options;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace CRMwithAD.Blazor.Server.Services
{
    public class ADAuthService
    {
        private readonly ADAuthentication _adAuthConfig;
        public ADAuthService(IOptions<ADAuthentication> adAuthConfig)
        {
            _adAuthConfig = adAuthConfig.Value;
        }

        public bool Login(AdLogin adLogin)
        {
            var ldapServer = _adAuthConfig.Server;
            var ldapDomain = _adAuthConfig.Domain;
            var useLdaps = _adAuthConfig.UseLdaps;
            var useStartTls = _adAuthConfig.UseStartTls;
            var identifier = $"{adLogin.Username}@{ldapDomain}";

            try
            {
                using var connection = new LdapConnection(ldapServer);
                connection.SessionOptions.SecureSocketLayer = useLdaps;
                connection.SessionOptions.ProtocolVersion = 3;

                // Set AuthType based on OS
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    connection.AuthType = AuthType.Negotiate;
                }
                else
                {
                    connection.AuthType = AuthType.Basic;
                }

                if (useStartTls)
                {
                    connection.SessionOptions.StartTransportLayerSecurity(null);
                }

                var credential = new NetworkCredential(identifier, adLogin.Password);
                connection.Bind(credential);

                return true;
            }
            catch (LdapException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool AuthenticateFromAD(AdLogin adLogin, out List<string> roles)
        {
            roles = new List<string>();
            var traceNo = string.Empty;

            var ldapServer = _adAuthConfig.Server;
            var ldapPort = _adAuthConfig.Port;
            var ldapDomain = _adAuthConfig.Domain;
            var useLdaps = _adAuthConfig.UseLdaps;
            var useStartTls = _adAuthConfig.UseStartTls;
            var baseDn = _adAuthConfig.BaseDN;

            // Prefer UPN unless caller already gave UPN or DOMAIN\user
            var identifier = adLogin.Username.Contains('@') || adLogin.Username.Contains('\\')
                ? adLogin.Username
                : $"{adLogin.Username}@{ldapDomain}";

            try
            {
                traceNo = Guid.NewGuid().ToString();

                var id = new LdapDirectoryIdentifier(ldapServer, ldapPort, false, false);
                using var connection = new LdapConnection(id)
                {
                    Timeout = TimeSpan.FromSeconds(100000)
                };

                if (useLdaps || useStartTls)
                {
                    // Secure connections must use simple bind (Basic)
                    connection.AuthType = AuthType.Basic;
                }
                else
                {
                    // Plain LDAP: on Windows you can use Negotiate
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        connection.AuthType = AuthType.Negotiate;
                    }
                    else
                    {
                        connection.AuthType = AuthType.Basic;
                    }
                }

                connection.SessionOptions.ProtocolVersion = 3;

                if (useLdaps)
                {
                    connection.SessionOptions.SecureSocketLayer = true; // ldaps://:636
                }
                else if (useStartTls)
                {
                    // If you flip this to true later, the rest just works.
                    connection.SessionOptions.StartTransportLayerSecurity(null);
                }

                // 1) Bind AS THE USER — this validates the password.
                var credential = new NetworkCredential(identifier, adLogin.Password);
                connection.Bind(credential);

                // 2) Find the user DN (by UPN or sAMAccountName)
                var sam = ExtractSamAccountName(adLogin.Username);
                var userFilter =
                    "(&" +
                        "(objectCategory=person)" +
                        "(objectClass=user)" +
                        "(|" +
                            $"(userPrincipalName={EscapeFilter(identifier)})" +
                            $"(sAMAccountName={EscapeFilter(sam)})" +
                        ")" +
                    ")";

                var userReq = new SearchRequest(
                    baseDn,
                    userFilter,
                    SearchScope.Subtree,
                    new[] { "distinguishedName" }
                );
                var userResp = (SearchResponse)connection.SendRequest(userReq);
                if (userResp.Entries.Count == 0)
                {
                    // Auth was OK but we couldn't locate the user object under BaseDN.
                    // Return true (authenticated) with empty roles.
                    return true;
                }

                var userDn = userResp.Entries[0].DistinguishedName;

                // 3) Get ALL (nested) groups via matching rule in chain
                //    (&(objectClass=group)(member:1.2.840.113556.1.4.1941:=<userDN>))
                byte[]? cookie = null;
                const int pageSize = 500;
                do
                {
                    var grpReq = new SearchRequest(
                        baseDn,
                        $"(&(objectClass=group)(member:1.2.840.113556.1.4.1941:={EscapeFilter(userDn)}))",
                        SearchScope.Subtree,
                        new[] { "cn" }
                    );

                    var page = new PageResultRequestControl(pageSize) { Cookie = cookie };
                    grpReq.Controls.Add(page);

                    var grpResp = (SearchResponse)connection.SendRequest(grpReq);

                    foreach (SearchResultEntry e in grpResp.Entries)
                    {
                        if (e.Attributes["cn"]?.Count > 0)
                        {
                            var name = e.Attributes["cn"][0]?.ToString();
                            if (!string.IsNullOrEmpty(name) && !roles.Contains(name, StringComparer.OrdinalIgnoreCase))
                                roles.Add(name);
                        }
                    }

                    // Next page cookie (if any)
                    cookie = null;
                    foreach (DirectoryControl c in grpResp.Controls)
                    {
                        if (c is PageResultResponseControl prc)
                        {
                            cookie = prc.Cookie;
                            break;
                        }
                    }
                }
                while (cookie != null && cookie.Length > 0);

                roles.Sort(StringComparer.OrdinalIgnoreCase);
                return true;
            }
            catch
            {
                roles = new List<string>();
                return false;
            }
        }
        public List<string> GetUsersFromOU(string ouName)
        {
            var traceNo = string.Empty;
            var ldapServer = _adAuthConfig.Server;
            var ldapPort = _adAuthConfig.Port;
            var useLdaps = _adAuthConfig.UseLdaps;
            var useStartTls = _adAuthConfig.UseStartTls;
            var baseDn = _adAuthConfig.BaseDN;
            var ldapDomain = _adAuthConfig.Domain;

            // Build the distinguishedName for the OU
            // Support nested OUs: "Finance/Users" => "OU=Users,OU=Finance,BaseDN"
            var ouParts = ouName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            var ouDnBuilder = new StringBuilder();
            for (int i = ouParts.Length - 1; i >= 0; i--)
            {
                ouDnBuilder.Append($"OU={ouParts[i]},");
            }
            ouDnBuilder.Append(baseDn);
            var ouDistinguishedName = ouDnBuilder.ToString();

            var users = new List<string>();

            try
            {
                traceNo = Guid.NewGuid().ToString();
                var id = new LdapDirectoryIdentifier(ldapServer, ldapPort, false, false);
                using var connection = new LdapConnection(id)
                {
                    AuthType = AuthType.Negotiate,
                    Timeout = TimeSpan.FromSeconds(30)
                };
                connection.SessionOptions.ProtocolVersion = 3;

                if (useLdaps)
                {
                    connection.SessionOptions.SecureSocketLayer = true;
                }
                else if (useStartTls)
                {
                    connection.SessionOptions.StartTransportLayerSecurity(null);
                }

                // Bind as a service account (read-only)
                var credential = new NetworkCredential($"{_adAuthConfig.UserName}@{ldapDomain}", _adAuthConfig.Password);
                connection.Bind(credential);

                // Search for all user objects in the OU
                var userFilter = "(&(objectCategory=person)(objectClass=user))";
                var searchRequest = new SearchRequest(
                    ouDistinguishedName,
                    userFilter,
                    SearchScope.Subtree,
                    new[] { "sAMAccountName" }
                );

                var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);

                foreach (SearchResultEntry entry in searchResponse.Entries)
                {
                    if (entry.Attributes["sAMAccountName"]?.Count > 0)
                    {
                        var username = entry.Attributes["sAMAccountName"][0]?.ToString();
                        if (!string.IsNullOrEmpty(username))
                            users.Add(username);
                    }
                }
            }
            catch
            {
                return [];
            }

            return users;
        }

        private static string ExtractSamAccountName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            if (input.Contains('\\')) return input.Split('\\', 2)[1];
            if (input.Contains('@')) return input.Split('@', 2)[0];
            return input;
        }
        // RFC 4515 escaping for filter values
        private static string EscapeFilter(string value)
        {
            return value
                .Replace(@"\", @"\5c")
                .Replace("*", @"\2a")
                .Replace("(", @"\28")
                .Replace(")", @"\29")
                .Replace("\0", @"\00");
        }
    }
}
