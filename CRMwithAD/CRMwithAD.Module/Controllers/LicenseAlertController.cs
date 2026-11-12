using CRMwitAD.Shared.Services;
using DevExpress.ExpressApp;

namespace CRMwithAD.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class LicenseAlertController : WindowController
    {
        // Use CodeRush to create Controllers and Actions with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/403133/
        public LicenseAlertController()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            //var licenseSvc = Application.ServiceProvider.GetRequiredService<LicenseEmbedder>();

            if (!LicenseValidator.IsSignatureValid)
            {
                //Application.ShowViewStrategy
                //  .ShowMessage("Invalid license signature!", InformationType.Error);
                Application.ShowViewStrategy.ShowMessage(
                       "Your license is expired. Please contact your service provider for new license",
                       InformationType.Warning,
                       5000,
                       InformationPosition.Bottom);
                return;
            }
            if (LicenseValidator.DaysUntilExpiry is >= 0 and <= 7)
            {

                Application.ShowViewStrategy.ShowMessage(
                       $"Your license will expires in {LicenseValidator.DaysUntilExpiry} days",
                       InformationType.Info,
                       5000,
                       InformationPosition.Bottom);

                //Application.ShowViewStrategy
                //      .ShowMessage(
                //        $"Your license expires in {licenseSvc.DaysUntilExpiry} day(s). Please renew soon.",
                //        InformationType.Warning
                //      );
            }
            if (LicenseValidator.DaysUntilExpiry is >= 0 and <= 7)
            {

                Application.ShowViewStrategy.ShowMessage(
                       $"Your license will expires in {LicenseValidator.DaysUntilExpiry} days",
                       InformationType.Info,
                       5000,
                       InformationPosition.Bottom);

                //Application.ShowViewStrategy
                //      .ShowMessage(
                //        $"Your license expires in {licenseSvc.DaysUntilExpiry} day(s). Please renew soon.",
                //        InformationType.Warning
                //      );
            }
            // Perform various tasks depending on the target Window.
        }

        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }
    }
}
