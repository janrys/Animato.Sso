namespace Animato.Sso.Application.Security;
using System;
using System.Security.Cryptography;
using System.Text;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Microsoft.IdentityModel.Tokens;

public class LocalFileCertificateManager : ICertificateManager
{
    private readonly RSA rsa;
    private readonly string privateKey;
    private readonly string publicKey;
    private const string PublicKeyHeader = "PUBLIC KEY";
    private const string PrivateKeyHeader = "RSA PRIVATE KEY";
    private readonly DateTime startTime;

    public LocalFileCertificateManager(GlobalOptions globalOptions, IDateTimeService dateTimeService)
    {
        if (globalOptions is null)
        {
            throw new ArgumentNullException(nameof(globalOptions));
        }


        rsa = RSA.Create();

        if (!File.Exists(globalOptions.TokenSigningCertificatePath))
        {
            throw new ArgumentException($"File with token signing certificate does not exist - {globalOptions.TokenSigningCertificatePath}", nameof(globalOptions));
        }

        try
        {
            rsa.ImportFromPem(File.ReadAllText(globalOptions.TokenSigningCertificatePath));
            publicKey = MakePem(rsa.ExportSubjectPublicKeyInfo(), null);
            privateKey = MakePem(rsa.ExportRSAPrivateKey(), null);
            startTime = dateTimeService.UtcNow;
        }
        catch (Exception eception)
        {
            throw new IOException("Error loading token signing certificate", eception);
        }
    }

    public SecurityKey GetTokenSigningKey() => new RsaSecurityKey(rsa);
    public string GetTokenSigningAlghorithm() => SecurityAlgorithms.RsaSha256;
    public string GetPublicKey() => publicKey;
    public string GetPrivateKey() => privateKey;


#pragma warning disable IDE0051 // Remove unused private members
    private static string MakePemWithPublicHeader(byte[] ber) => MakePem(ber, PublicKeyHeader);
    private static string MakePemWithPrivateHeader(byte[] ber) => MakePem(ber, PrivateKeyHeader);

    private static string MakePem(byte[] ber, string header)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(header))
        {
            builder.Append("-----BEGIN ");
            builder.Append(header);
            builder.AppendLine("-----");
        }

        var base64 = Convert.ToBase64String(ber);
        var offset = 0;
        const int lineLength = 64;

        while (offset < base64.Length)
        {
            var lineEnd = Math.Min(offset + lineLength, base64.Length);

            if (!string.IsNullOrEmpty(header))
            {
                builder.AppendLine(base64[offset..lineEnd]);
            }
            else
            {
                builder.Append(base64[offset..lineEnd]);
            }

            offset = lineEnd;
        }

        if (!string.IsNullOrEmpty(header))
        {
            builder.Append("-----END ");
            builder.Append(header);
            builder.AppendLine("-----");
        }
        return builder.ToString();
    }

    private static byte[] PemToBerWithPublicHeader(string pem) => PemToBer(pem, PublicKeyHeader);
    private static byte[] PemToBerWithPrivateHeader(string pem) => PemToBer(pem, PrivateKeyHeader);

    private static byte[] PemToBer(string pem, string header)
    {
        var base64Start = 0;
        var endIndex = pem.Length;

        if (!string.IsNullOrEmpty(header))
        {
            // Technically these should include a newline at the end,
            // and either newline-or-beginning-of-data at the beginning.
            var begin = $"-----BEGIN {header}-----";
            var end = $"-----END {header}-----";

            var beginIndex = pem.IndexOf(begin, StringComparison.OrdinalIgnoreCase);
            base64Start = beginIndex + begin.Length;
            endIndex = pem.IndexOf(end, base64Start, StringComparison.OrdinalIgnoreCase);
        }

        return Convert.FromBase64String(pem[base64Start..endIndex]);
    }
#pragma warning restore IDE0051 // Remove unused private members


    public Models.JsonWebKey GetJsonWebKey()
    {
        var jsonWebKey = new Models.JsonWebKey
        {
            Kty = "RSA",
            N = publicKey,
            Alg = "RS256",
            Use = "sig",
            Kid = startTime.ToString("yyyy-MM-dd", GlobalOptions.Culture)
        };
        return jsonWebKey;
    }
}
