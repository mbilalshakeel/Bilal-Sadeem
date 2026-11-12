using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Model.DomainLogics;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using CRMwithAD.Module.BusinessObjects.LicenseManagement;
using License = CRMwithAD.Module.BusinessObjects.LicenseManagement.License;
using CRMwitAD.Shared.Services;

namespace CRMwithAD.Module;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.ModuleBase.
public sealed class CRMwithADModule : ModuleBase {
    public CRMwithADModule() {
		// 
		// CRMwithADModule
		// 
		AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.ModelDifference));
		AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.ModelDifferenceAspect));
        AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.BaseObject));
        AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.AuditDataItemPersistent));
        AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.AuditedObjectWeakReference));
        AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.FileData));
        AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.FileAttachmentBase));
        AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.Event));
        AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.Resource));
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.SystemModule.SystemModule));
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Security.SecurityModule));
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.AuditTrail.AuditTrailModule));
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Objects.BusinessClassLibraryCustomizationModule));
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.CloneObject.CloneObjectModule));
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.ConditionalAppearance.ConditionalAppearanceModule));
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Dashboards.DashboardsModule));
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Notifications.NotificationsModule));
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Office.OfficeModule));
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.ReportsV2.ReportsModuleV2));
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Scheduler.SchedulerModuleBase));
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.StateMachine.StateMachineModule));
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Validation.ValidationModule));
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.ViewVariantsModule.ViewVariantsModule));
    }
    public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB) {
        ModuleUpdater updater = new DatabaseUpdate.Updater(objectSpace, versionFromDB);
        return new ModuleUpdater[] { updater };
    }
    public override void Setup(XafApplication application) {
        base.Setup(application);
        application.CustomProcessShortcut += Application_CustomProcessShortcut;
        application.ObjectSpaceCreated += Application_ObjectSpaceCreated;
        // Manage various aspects of the application UI and behavior at the module level.
    }

    private void Application_CustomProcessShortcut(object sender, CustomProcessShortcutEventArgs e)
    {
        var app = (XafApplication)sender;
        //var licenseSvc = app.ServiceProvider.GetRequiredService<LicenseService>();

        // Only intercept the built-in navigation shortcuts that open BO views:
        var viewId = e.Shortcut?.ViewId;
        bool isBusinessView =
            viewId?.EndsWith("_ListView") == true ||
            viewId?.EndsWith("_DetailView") == true;
        if (!isBusinessView)
        {
            return; // allow dashboards, reports, etc.
        }
        var licenseListViewId = nameof(License) + "_ListView";
        var licenseDetailViewId = nameof(License) + "_DetailView";

        if (!LicenseValidator.IsSignatureValid)
        {

            if (viewId == licenseListViewId || viewId == licenseDetailViewId)
            {
                return;
            }
            // Block everything else
            e.Handled = true;
            app.ShowViewStrategy.ShowMessage(
                "The license file appears to be tampered" +
                "Contact your service provider.",
                InformationType.Error
            );
            return;
            //e.Handled = true;  // this stops XAF from opening the view
            //app.ShowViewStrategy.ShowMessage(
            //    "Your license has expired. Please contact your service provider for a new license key.",
            //    InformationType.Error
            //);

            //// If the user is navigating to the LicenseFiles ListView or DetailView, allow it:
            //if (viewId == licenseListViewId || viewId == licenseDetailViewId)
            //{
            //    return;
            //}

            //return;
        }


        int daysUntilExpiry = LicenseValidator.DaysUntilExpiry;
        // 1) Not yet expired
        if (daysUntilExpiry > 0)
        {
            if (daysUntilExpiry <= 7)
            {
                app.ShowViewStrategy.ShowMessage(
                  $"Your license expires in {daysUntilExpiry} day(s). Please renew soon.",
                  InformationType.Warning
                );
            }
            return;
        }

        int daysAfterExpiry = -daysUntilExpiry;            // e.g. if DaysUntilExpiry = -3, daysAfterExpiry = 3
        int graceLeft = 30 - daysAfterExpiry;        // e.g. 30 - 3 = 27 days of grace remaining
        if (daysAfterExpiry < 30)
        {
            app.ShowViewStrategy.ShowMessage(
              $"Your license expired {daysAfterExpiry} day(s) ago. " +
              $"You have {graceLeft} day(s) of grace period remaining.",
              InformationType.Warning
            );
            return;
        }


        // If the user is navigating to the LicenseFiles ListView or DetailView, allow it:
        if (viewId == licenseListViewId || viewId == licenseDetailViewId)
        {
            return;
        }

        e.Handled = true;
        app.ShowViewStrategy.ShowMessage(
          "Your license and grace period have expired. " +
          "Application access is now restricted. " +
          "Please contact your service provider for renewal.",
          InformationType.Error
        );
    }
    private void Application_ObjectSpaceCreated(object sender, ObjectSpaceCreatedEventArgs e)
    {
        if (e.ObjectSpace is XPObjectSpace xpSpace)
        {
            //xpSpace.Committing += XpSpace_Committing;
            // …hook its commit event
            //xpSpace.Committed += XpSpace_Committed;
        }
    }
    private void XpSpace_Committing(object sender, CancelEventArgs e)
    {
        var xpSpace = (XPObjectSpace)sender;
        // Find all LicenseFiles instances that are part of this commit
        var licensesToSave = xpSpace
          .GetObjectsToSave(true)              // true = include modifications
          .OfType<License>();

        foreach (var license in licensesToSave)
        {
            // 1) license.json attachment must be named license.enc
            if (license.LicenseFile1 == null || license.LicenseFile1.IsEmpty ||
                !license.LicenseFile1.FileName.Equals("license.enc", StringComparison.OrdinalIgnoreCase))
            {
                throw new UserFriendlyException(
                  "Please upload the encrypted license as exactly “license.enc”."
                );
            }
            // 2) signature must be license.sig
            if (license.LicenseFile2 == null || license.LicenseFile2.IsEmpty ||
                !license.LicenseFile2.FileName.Equals("license.sig", StringComparison.OrdinalIgnoreCase))
            {
                throw new UserFriendlyException(
                  "Please upload the signature as exactly “license.sig”."
                );
            }
            // 3) public key must be public.key

        }
    }
    public override void CustomizeTypesInfo(ITypesInfo typesInfo) {
        base.CustomizeTypesInfo(typesInfo);
        CalculatedPersistentAliasHelper.CustomizeTypesInfo(typesInfo);
    }
}
