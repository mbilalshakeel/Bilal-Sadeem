using CRMwitAD.Shared.Models;
using CRMwithAD.Module.Services;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using License = CRMwithAD.Module.BusinessObjects.LicenseManagement.License;

namespace CRMwithAD.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class LicenseFilesValidationController : ObjectViewController<DetailView, License>
    {
        // Use CodeRush to create Controllers and Actions with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/403133/
        public LicenseFilesValidationController()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            if (Frame != null)
            {
                var mods = Frame.GetController<ModificationsController>();
                if (mods != null)
                {
                    mods.SaveAction.Executing += SaveAction_Executing;
                    mods.SaveAndCloseAction.Executing += SaveAction_Executing;
                }
            }
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
        protected override void OnDeactivated()
        {
            if (Frame != null)
            {
                var mods = Frame.GetController<ModificationsController>();
                if (mods != null)
                {
                    mods.SaveAction.Executing -= SaveAction_Executing;
                    mods.SaveAndCloseAction.Executing -= SaveAction_Executing;
                }
            }
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }
        private void SaveAction_Executing(object sender, CancelEventArgs e)
        {
            // Grab the current LicenseFiles object
            if (View.CurrentObject is not License license) return;

            // 1) license.json must be named "license.enc"
            if (license.LicenseFile1 == null || license.LicenseFile1.IsEmpty ||
               !license.LicenseFile1.FileName.Equals("license.enc", StringComparison.OrdinalIgnoreCase))
            {
                e.Cancel = true;
                throw new UserFriendlyException(
                  "Please upload the encrypted license as exactly “license.enc”.");
            }
            // 2) signature must be "license.sig"
            if (license.LicenseFile2 == null || license.LicenseFile2.IsEmpty ||
               !license.LicenseFile2.FileName.Equals("license.sig", StringComparison.OrdinalIgnoreCase))
            {
                e.Cancel = true;
                throw new UserFriendlyException(
                  "Please upload the signature as exactly “license.sig”.");
            }

            // 4) If you need to recompute ExpiryUtc/Status on update:

            // Read license key path from appsettings.json using AppSettingsProvider
            var config = AppSettingsProvider.Configuration;
            string licenseDir = config["KeyPath:Path"];
            string json = DecryptToJson(license.LicenseFile1, Path.Join(licenseDir, "private.key"));

            var info = JsonSerializer.Deserialize<LicenseInfo>(json)
                    ?? throw new InvalidOperationException("Bad license JSON");

            license.ExpiryDate = info.ExpiryUtc;
            license.Status = license.ExpiryDate > DateTime.UtcNow ? "Valid" : "Expired";

        }
        private string DecryptToJson(IFileData encryptedFileData, string privateKeyPath)
        {
            if (encryptedFileData == null)
            {
                throw new InvalidOperationException("No encrypted license uploaded.");
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
