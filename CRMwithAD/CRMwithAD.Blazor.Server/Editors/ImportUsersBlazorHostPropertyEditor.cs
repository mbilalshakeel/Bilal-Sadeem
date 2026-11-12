using CRMwithAD.Blazor.Server.Pages;
using DevExpress.ExpressApp.Blazor.Components.Models;
using DevExpress.ExpressApp.Blazor.Editors;
using DevExpress.ExpressApp.Blazor.Editors.Adapters;
using DevExpress.ExpressApp.Model;

namespace CRMwithAD.Blazor.Server.Editors
{
    public class ImportUsersBlazorHostPropertyEditor : BlazorPropertyEditorBase
    {
        public ImportUsersBlazorHostPropertyEditor(Type objectType, IModelMemberViewItem model) : base(objectType, model) { }

        protected override IComponentAdapter CreateComponentAdapter()
        {
            // Use a custom model that overrides ComponentType
            return new ObjectComponentAdapter(new ImportUsersBlazorHostComponentModel());
        }
    }

    public class ImportUsersBlazorHostComponentModel : ObjectComponentModel
    {
        public override Type ComponentType => typeof(ImportUsersPopup); // Use your actual component type here
    }
}
