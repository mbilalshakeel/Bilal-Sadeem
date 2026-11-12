using CRMwitAD.Shared.Services;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor;

namespace CRMwithAD.Blazor.Server;

public class CRMwithADBlazorApplication : BlazorApplication
{
    public static LicenseValidator LicenseSvc { get; } = new LicenseValidator();
    public static string KeyDirectory = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("KeyPath")["Path"]!;
    public CRMwithADBlazorApplication()
    {


        ApplicationName = "CRMwithAD";
        CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
        DatabaseVersionMismatch += CRMwithADBlazorApplication_DatabaseVersionMismatch;

        //var contentRoot = "D:\\Licenses\\"; //Environment.CurrentDirectory; //AppDomain.CurrentDomain.BaseDirectory;
        //var KeyDirectory = "D:\\Licenses\\";
        LicenseSvc.LoadEncryptedLicense(
            //Path.Combine(contentRoot, "license.enc"),
            Path.Join(KeyDirectory, "license.enc"),
            Path.Join(KeyDirectory, "license.sig"),
            Path.Join(KeyDirectory, "public.key"),
            Path.Join(KeyDirectory, "private.key")
        );
    }
    protected override void OnSetupStarted()
    {
        base.OnSetupStarted();
#if DEBUG
        if (System.Diagnostics.Debugger.IsAttached && CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema)
        {
            DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
        }
#endif
    }
    private void CRMwithADBlazorApplication_DatabaseVersionMismatch(object sender, DatabaseVersionMismatchEventArgs e)
    {
#if EASYTEST
        e.Updater.Update();
        e.Handled = true;
#else
        if (System.Diagnostics.Debugger.IsAttached)
        {
            e.Updater.Update();
            e.Handled = true;
        }
        else
        {
            string message = "The application cannot connect to the specified database, " +
                "because the database doesn't exist, its version is older " +
                "than that of the application or its schema does not match " +
                "the ORM data model structure. To avoid this error, use one " +
                "of the solutions from the https://www.devexpress.com/kb=T367835 KB Article.";

            if (e.CompatibilityError != null && e.CompatibilityError.Exception != null)
            {
                message += "\r\n\r\nInner exception: " + e.CompatibilityError.Exception.Message;
            }
            throw new InvalidOperationException(message);
        }
#endif
    }
}
