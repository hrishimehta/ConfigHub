using ConfigHub.Domain.Interface;
using ConfigHub.Shared;
using System.Security.Cryptography.X509Certificates;

namespace ConfigHub.API.Middleware
{
    public class CertificateAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CertificateAuthenticationMiddleware> _logger;
        private readonly IConfigService _configService;

        public CertificateAuthenticationMiddleware(RequestDelegate next, ILogger<CertificateAuthenticationMiddleware> logger, IConfigService configService)
        {
            _next = next;
            _logger = logger;
            _configService = configService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var applicationId = context.Request.Headers[Constants.ApplicationNameHeader].ToString();

            // Map client certificate to application ID
            // Save it in the HttpContext for later use
            context.Items["ApplicationId"] = applicationId;

            //string clientCertBase64 = context.Request.Headers[Constants.ClientCertificateHeader];
            //byte[] certData = Convert.FromBase64String(clientCertBase64);
            //// Get client certificate from the request
            //X509Certificate2 clientCert = new X509Certificate2(certData);

            //if (clientCert == null || !IsValidClientCertificate(clientCert))
            //{
            //    context.Response.StatusCode = StatusCodes.Status403Forbidden;
            //    return;
            //}

            //// Check if the client certificate is mapped to the correct application ID
            //if (!await _configService.IsValidApplicationCertificateMappingAsync(clientCert.Thumbprint, applicationId))
            //{
            //    context.Response.StatusCode = StatusCodes.Status403Forbidden;
            //    return;
            //}


            await _next(context);
        }

        private bool IsValidClientCertificate(X509Certificate2 certificate)
        {
            // Check if the certificate is null
            if (certificate == null)
            {
                return false;
            }

            // Check if the certificate is valid (not expired)
            if (certificate.NotAfter < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }
    }
}
