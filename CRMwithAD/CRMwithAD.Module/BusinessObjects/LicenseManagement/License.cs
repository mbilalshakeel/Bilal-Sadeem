using CRMwitAD.Shared.Models;
using CRMwitAD.Shared.Services;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CRMwithAD.Module.BusinessObjects.LicenseManagement
{
    [DefaultClassOptions]
    public class License : BaseObject
    {
        public License(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }
        [VisibleInListView(true)]
        [VisibleInDetailView(false)]
        public string Action => "Change/Update License";


        FileData licenseFile1;
        [VisibleInListView(false)]
        public FileData LicenseFile1
        {
            get => licenseFile1;
            set => SetPropertyValue(nameof(LicenseFile1), ref licenseFile1, value);
        }

        FileData licenseFile2;
        [VisibleInListView(false)]
        public FileData LicenseFile2
        {
            get => licenseFile2;
            set => SetPropertyValue(nameof(LicenseFile2), ref licenseFile2, value);
        }

        private DateTime enablementDate;
        [ModelDefault("AllowEdit", "False")]
        [ModelDefault("DisplayFormat", "{0:yyyy-MM-dd}")]
        [VisibleInListView(true), VisibleInDetailView(true)]
        public DateTime EnablementDate
        {
            get { return enablementDate; }
            set { SetPropertyValue<DateTime>(nameof(EnablementDate), ref enablementDate, value); }
        }

        private DateTime expiryDate;
        [ModelDefault("AllowEdit", "False")]
        [ModelDefault("DisplayFormat", "{0:yyyy-MM-dd}")]
        [VisibleInListView(true), VisibleInDetailView(true)]
        public DateTime ExpiryDate
        {
            get { return expiryDate; }
            set { SetPropertyValue<DateTime>(nameof(ExpiryDate), ref expiryDate, value); }
        }

        private string status;
        [ModelDefault("AllowEdit", "False")]
        [VisibleInListView(true), VisibleInDetailView(true)]
        public string Status
        {

            get { return status; }
            set { SetPropertyValue<string>(nameof(Status), ref status, value); }
        }
        protected override void OnSaving()
        {


            if (Session.IsNewObject(this))
            {
                // ...and there's already one in the DB, block it
                if (Session.Query<License>().Any())
                {
                    throw new UserFriendlyException(
                      "Only updated record not allowed to add new record.");
                }
            }
            //System.Diagnostics.Debugger.Launch();
            if (licenseFile1 == null || licenseFile1.IsEmpty || !string.Equals(licenseFile1.FileName, "license.enc", StringComparison.OrdinalIgnoreCase))
            {
                throw new UserFriendlyException("Please upload the encrypted license file named exactly “license.enc”.");
            }
            if (licenseFile2 == null || licenseFile2.IsEmpty || !string.Equals(licenseFile2.FileName, "license.sig", StringComparison.OrdinalIgnoreCase))
            {
                throw new UserFriendlyException("Please upload the signature file named exactly “license.sig”.");
            }

            // 3.1) Dump each blob to disk
            var paths = LicenseValidator.paths();
            //var baseDir = "D:\\Licenses\\"; //Environment.CurrentDirectory;  //AppDomain.CurrentDomain.BaseDirectory;
            //WriteFile(LicenseFile1, Path.Combine(baseDir, "license.enc"));
            WriteFile(LicenseFile1, Path.Join(paths.encJsonPath, ""));
            WriteFile(LicenseFile2, Path.Join(paths.sigPath, ""));
            //WriteFile(PublicKey, Path.Combine("D:\\Licenses\\", "public.key"));
            //WriteFile(PrivateKey, Path.Combine("D:\\Licenses\\", "private.key"));

            // 3.2) Read and parse the JSON for expiry
            //string json;
            //using (var msJson = new MemoryStream())
            //{
            //    // Write the FileData into our MemoryStream
            //    if(LicenseJson==null)
            //        return;
            //    LicenseJson.SaveToStream(msJson);
            //    msJson.Position = 0;
            //    using var reader = new StreamReader(msJson);
            //    json = reader.ReadToEnd();
            //}

            string json = DecryptToJson(LicenseFile1, Path.Join(paths.privKey, ""));


            // --- parse it ---
            var info = JsonSerializer.Deserialize<LicenseInfo>(json)
                       ?? throw new InvalidOperationException("Bad license JSON");

            ExpiryDate = info.ExpiryUtc;
            EnablementDate = DateTime.Now;
            Status = ExpiryDate > DateTime.UtcNow ? "Valid" : "Expired";

            base.OnSaving();
            // 3.3) (Optional) reload your global LicenseService:
            // var svc = Application.ServiceProvider.GetRequiredService<LicenseService>();
            // svc.LoadLicense(Path.Combine(baseDir,"license.json"), /*etc*/);
        }

        private void WriteFile(IFileData data, string path)
        {
            // data == null or IsEmpty if no file was attached
            if (data == null) return;
            // Create or overwrite the file on disk
            using var fs = File.Open(path, FileMode.Create, FileAccess.Write);
            // Stream the contents of the FileData into the FileStream
            data.SaveToStream(fs);
        }
        protected override void OnDeleting()
        {

            throw new UserFriendlyException("Deleting the license files is not allowed. If accidently deleted application will stop working");
        }

        private string DecryptToJson(IFileData encryptedFileData, string privateKeyPath)
        {
            if (encryptedFileData == null)
            {
                throw new InvalidOperationException("No license uploaded.");
            }
            // 1) Load the encrypted bytes
            byte[] encryptedBytes;
            using (var ms = new MemoryStream())
            {
                encryptedFileData.SaveToStream(ms);
                encryptedBytes = ms.ToArray();
            }
            // 2) Load your app's RSA private key (PEM or PKCS#1 DER)
            byte[] privateKeyBytes = File.ReadAllBytes(privateKeyPath);
            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
            // 3) Decrypt
            byte[] plainBytes = rsa.Decrypt(
                encryptedBytes,
                RSAEncryptionPadding.OaepSHA256
            );
            // 4) Turn into a string
            return Encoding.UTF8.GetString(plainBytes);
        }

    }
}