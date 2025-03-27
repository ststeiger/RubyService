
namespace StartRuby
{


    public static class CertificateCallback
    {


        public static void Initialize()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = AcceptSelfSignedValidationCallback!;
        }


        public static void TrustAll()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = AcceptAnythingValidationCallback!;
        }


        static CertificateCallback()
        {
            // Initialize();
        }


        public static bool AcceptAnythingValidationCallback(
            object sender,
            System.Security.Cryptography.X509Certificates.X509Certificate? cert,
            System.Security.Cryptography.X509Certificates.X509Chain? chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors
        )
        {
            return true; // Always accept the certificate
        }


        // https://stackoverflow.com/questions/9058096/how-to-call-the-default-certificate-check-when-overriding-servicepointmanager-se
        // https://github.com/microsoft/referencesource/blob/master/System/net/System/Net/Internal.cs
        public static bool AcceptSelfSignedValidationCallback(
             object sender,
             System.Security.Cryptography.X509Certificates.X509Certificate certificate,
             System.Security.Cryptography.X509Certificates.X509Chain chain,
             System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            // Always allow localhost 
            System.Net.HttpWebRequest? request = sender as System.Net.HttpWebRequest;
            if (request != null && "localhost".Equals(request.Host, System.StringComparison.InvariantCultureIgnoreCase))
                return true;

            // If there are errors in the certificate chain, look at each error to determine the cause.
            if ((sslPolicyErrors & System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                if (chain != null && chain.ChainStatus != null)
                {
                    foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status in chain.ChainStatus)
                    {
                        if ((certificate.Subject == certificate.Issuer) &&
                           (status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot))
                        {
                            // Self-signed certificates with an untrusted root are valid. 
                            continue;
                        } // End if self-signed certificate 

                        if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError)
                        {
                            // If there are any other errors in the certificate chain, the certificate is invalid,
                            // so the method returns false.
                            return false;
                        } // End if return false on any other errors 

                    } // Next status 

                } // End if (chain != null && chain.ChainStatus != null) 

                // When processing reaches this line, the only errors in the certificate chain are 
                // untrusted root errors for self-signed certificates. These certificates are valid
                // for default Exchange server installations, so return true.
                return true;
            } // End if ((sslPolicyErrors & System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors) != 0) 

            // In all other cases, return false.
            return false;
        } // End Function CertificateValidationCallBack 


    } // End Class CertificateCallback 


} // End Namespace LdapService 
