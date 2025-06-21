using System;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace BlackBook.Security;

public static class SecurityManager {

    private const X509KeyUsageFlags DefaultKeyUsages =
        X509KeyUsageFlags.DigitalSignature |
        X509KeyUsageFlags.NonRepudiation |
        X509KeyUsageFlags.KeyEncipherment |
        X509KeyUsageFlags.DataEncipherment |
        X509KeyUsageFlags.KeyAgreement |
        X509KeyUsageFlags.KeyCertSign |
        X509KeyUsageFlags.CrlSign;

    private static readonly OidCollection DefaultEnhancedUsages = new() {
        new("1.3.6.1.5.5.7.3.1"),    // Server Authentication
        new("1.3.6.1.5.5.7.3.2"),    // Client Authentication
        new("1.3.6.1.5.5.7.3.3"),    // Code Signing
        new("1.3.6.1.5.5.7.3.4"),    // Email Protection
        new("1.3.6.1.5.5.7.3.8"),    // Time Stamping
        new("1.3.6.1.4.1.311.10.3.4") // Encrypting File System (EFS)
    };

    public static X509Certificate2 GenerateCertificate (
        string commonName,
        string organization,
        string organizationalUnit,
        string country,
        string state,
        string locality,
        string password
    ) {
        var distinguishedName = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(commonName))
            distinguishedName.Append($"CN={commonName}, ");
        if (!string.IsNullOrWhiteSpace(organization))
            distinguishedName.Append($"O={organization}, ");
        if (!string.IsNullOrWhiteSpace(organizationalUnit))
            distinguishedName.Append($"OU={organizationalUnit}, ");
        if (!string.IsNullOrWhiteSpace(country))
            distinguishedName.Append($"C={country}, ");
        if (!string.IsNullOrWhiteSpace(state))
            distinguishedName.Append($"ST={state}, ");
        if (!string.IsNullOrWhiteSpace(locality))
            distinguishedName.Append($"L={locality}, ");

        // Remove trailing comma and space
        var dn = distinguishedName.ToString().TrimEnd(',', ' ');

        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP521);

        var request = new CertificateRequest(dn, ecdsa, HashAlgorithmName.SHA512);

        request.CertificateExtensions.Add(new X509KeyUsageExtension(DefaultKeyUsages, false));
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(DefaultEnhancedUsages, false));

        var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(10)
        );

        return new X509Certificate2(
            certificate.Export(X509ContentType.Pkcs12, password),
            password,
            X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable
        );
    }

    public static void ExportCertificate (X509Certificate2 certificate, string filePath, string password) {
        var bytes = certificate.Export(X509ContentType.Pkcs12, password);
        System.IO.File.WriteAllBytes(filePath, bytes);
    }

    public static X509Certificate2 LoadCertificate (string filePath, string password) {
        return new X509Certificate2(filePath, password, X509KeyStorageFlags.EphemeralKeySet);
    }

    public static string CreateCertPassword (string userName, string password) {
        var combined = $"{userName}  {password}";
        using var sha512 = SHA512.Create();
        var hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexString(hash);
    }


}
