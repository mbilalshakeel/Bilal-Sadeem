using CRMwitAD.Shared.Models;
using System.Security.Cryptography;
using System.Text.Json;

namespace CRMwitAD.Shared.Services
{
    public class LicenseValidator
    {

        private byte[] _plainJson;
        public static bool IsSignatureValid { get; private set; }
        public static bool IsExpired => DaysUntilExpiry < 0;
        public static int DaysUntilExpiry { get; private set; }
        public static string ErrorMessage { get; private set; } = string.Empty;

        public static string encPaths { get; private set; } = string.Empty;
        public static string sigPaths { get; private set; } = string.Empty;
        public static string genPublicKeyPaths { get; private set; } = string.Empty;
        public static string appPrivateKeyPaths { get; private set; } = string.Empty;

        public void LoadEncryptedLicense(string encPath, string sigPath, string genPublicKeyPath, string appPrivateKeyPath)
        {
            try
            {
                encPaths = string.Empty;
                sigPaths = string.Empty;
                genPublicKeyPaths = string.Empty;
                appPrivateKeyPaths = string.Empty;

                encPaths = encPath;
                sigPaths = sigPath;
                genPublicKeyPaths = genPublicKeyPath;
                appPrivateKeyPaths = appPrivateKeyPath;

                // 1) Read files
                var encryptedJson = File.ReadAllBytes(encPath);
                var signature = File.ReadAllBytes(sigPath);
                var genPubKey = File.ReadAllBytes(genPublicKeyPath);
                var appPrivKey = File.ReadAllBytes(appPrivateKeyPath);

                // 2) Decrypt JSON with app’s private key
                using var appRsa = RSA.Create();
                appRsa.ImportRSAPrivateKey(appPrivKey, out _);
                _plainJson = appRsa.Decrypt(encryptedJson, RSAEncryptionPadding.OaepSHA256);

                // 3) Verify signature with generator’s public key
                using var genRsa = RSA.Create();
                genRsa.ImportRSAPublicKey(genPubKey, out _);
                IsSignatureValid = genRsa.VerifyData(
                  _plainJson, signature,
                  HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1
                );
                if (!IsSignatureValid)
                {
                    ErrorMessage = "Invalid license signature.";
                    return;
                }

                // 4) Parse the JSON and compute expiry
                var info = JsonSerializer.Deserialize<LicenseInfo>(_plainJson)
                           ?? throw new InvalidOperationException("Malformed license JSON");
                DaysUntilExpiry = (int)(info.ExpiryUtc - DateTime.UtcNow).TotalDays;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"License load error: {ex.Message}";
            }
        }
        public static (string encJsonPath, string sigPath, string pubKey, string privKey) paths()
        {
            return (encPaths, sigPaths, genPublicKeyPaths, appPrivateKeyPaths);
        }
    }
}
