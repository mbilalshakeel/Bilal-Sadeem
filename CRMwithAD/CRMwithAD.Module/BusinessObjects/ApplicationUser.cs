using CRMwithAD.Module.BusinessObjects.Configurations;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using System.ComponentModel;

namespace CRMwithAD.Module.BusinessObjects;

[MapInheritance(MapInheritanceType.ParentTable)]
[DefaultProperty(nameof(UserName))]
public class ApplicationUser : PermissionPolicyUser, ISecurityUserWithLoginInfo, ISecurityUserLockout
{
    private int accessFailedCount;
    private DateTime lockoutEnd;

    public ApplicationUser(Session session) : base(session) { }

    [Browsable(false)]
    public int AccessFailedCount
    {
        get { return accessFailedCount; }
        set { SetPropertyValue(nameof(AccessFailedCount), ref accessFailedCount, value); }
    }

    [Browsable(false)]
    public DateTime LockoutEnd
    {
        get { return lockoutEnd; }
        set { SetPropertyValue(nameof(LockoutEnd), ref lockoutEnd, value); }
    }

    [Browsable(false)]
    [DevExpress.Xpo.Aggregated, Association("User-LoginInfo")]
    public XPCollection<ApplicationUserLoginInfo> LoginInfo
    {
        get { return GetCollection<ApplicationUserLoginInfo>(nameof(LoginInfo)); }
    }

    IEnumerable<ISecurityUserLoginInfo> IOAuthSecurityUser.UserLogins => LoginInfo.OfType<ISecurityUserLoginInfo>();

    ISecurityUserLoginInfo ISecurityUserWithLoginInfo.CreateUserLoginInfo(string loginProviderName, string providerUserKey)
    {
        ApplicationUserLoginInfo result = new ApplicationUserLoginInfo(Session);
        result.LoginProviderName = loginProviderName;
        result.ProviderUserKey = providerUserKey;
        result.User = this;
        return result;
    }

    private string _Name;

    public string Name
    {
        get { return _Name; }
        set { SetPropertyValue<string>(nameof(Name), ref _Name, value); }
    }
    private string _Email;


    public string Email
    {
        get { return _Email; }
        set { SetPropertyValue<string>(nameof(Email), ref _Email, value); }
    }

    //private Team _Teams;
    //[Association("Team-Users")]
    //[XafDisplayName("Team")]
    //[ImmediatePostData]

    //public Team Teams
    //{
    //    get { return _Teams; }
    //    set { SetPropertyValue<Team>(nameof(Teams), ref _Teams, value); }
    //}

    //private SubCategory _SubCategory;
    //[Association("SubCategory-Users")]
    //[XafDisplayName("SubCategory")]
    //[ImmediatePostData]
    //[VisibleInDetailView(false)] // Hide SubCategory navigation property in Detail View

    //public SubCategory SubCategories
    //{
    //    get { return _SubCategory; }
    //    set { SetPropertyValue<SubCategory>(nameof(SubCategories), ref _SubCategory, value); }
    //}

    [Association("SubCategory-Users")]
    [XafDisplayName("SubCategory")]
    [ImmediatePostData]
    [VisibleInDetailView(false)] // Hide SubCategory navigation property in Detail View
    public XPCollection<SubCategory> SubCategories
    {
        get { return GetCollection<SubCategory>(nameof(SubCategories)); }
    }

    [Association("Category-Users")]
    [XafDisplayName("Categories")]
    [ImmediatePostData]
    [VisibleInDetailView(false)]
    public XPCollection<Category> Categories
    {
        get { return GetCollection<Category>(nameof(Categories)); }
    }


    //[VisibleInDetailView(false)]  // Hide in DetailView (record creation/editing)
    //[VisibleInListView(false)]
    //[PersistentAlias("SubCategories.Oid")]
    //public Guid? SubCategoryOid
    //{
    //    get
    //    {
    //        return SubCategories != null ? (Guid?)SubCategories.Oid : null;
    //    }
    //}
}
