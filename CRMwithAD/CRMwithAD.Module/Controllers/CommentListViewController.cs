using CRMwithAD.Module.BusinessObjects.HelpdeskTickets;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;

namespace CRMwithAD.Module.Controllers
{
    public partial class CommentListViewController : ViewController<ListView>
    {
        public CommentListViewController()
        {
            InitializeComponent();
            TargetViewId = "Ticket_Comments_ListView";
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();

            if (View.Editor is DxGridListEditor gridListEditor)
            {
                // Access the column models (this is the correct way in modern XAF Blazor)
                foreach (var columnModel in gridListEditor.GridDataColumnModels)
                {
                    // Force format on the Date column
                    if (columnModel.FieldName == nameof(Comment.Date) ||
                        columnModel.Caption == "Date")
                    {
                        columnModel.DisplayFormat = "{0:yyyy-MM-dd HH:mm}";
                        // Optional: You can also set width, alignment, etc.
                        // columnModel.Width = 150;
                    }
                }
            }
        }
    }
}