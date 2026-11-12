using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.ExceptionServices;
using System.Security.Claims;
using System.Text;

namespace CRMwithAD.WebApi.JWT;

public class JwtTokenProviderService : IAuthenticationTokenProvider
{
    readonly SignInManager signInManager;
    readonly IConfiguration configuration;
    readonly IObjectSpaceFactory objectSpaceFactory;

    public JwtTokenProviderService(
        SignInManager signInManager,
        IConfiguration configuration,
        IObjectSpaceFactory objectSpaceFactory
    )
    {
        this.signInManager = signInManager;
        this.configuration = configuration;
        this.objectSpaceFactory = objectSpaceFactory;
    }

    public string Authenticate(object logonParameters)
    {
        var result = signInManager.AuthenticateByLogonParameters(logonParameters);
        if (!result.Succeeded && result.Error is IUserFriendlyException)
        {
            ExceptionDispatchInfo.Throw(result.Error);
        }
        if (!result.Succeeded)
        {
            throw new AuthenticationException("Internal server error");
        }

        var claims = new List<Claim>(result.Principal.Claims);
        //using var objectSpace = objectSpaceFactory.CreateObjectSpace(typeof(ApplicationUser));

        //var user = objectSpace.FindObject<ApplicationUser>(
        //    CriteriaOperator.Parse("UserName = ?", result.Principal.Identity?.Name)
        //);
        //if (user != null)
        //{
        //    foreach (var role in user.Roles)
        //    {
        //        claims.Add(new Claim(ClaimTypes.Role, role.Name));
        //    }
        //}

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            configuration["Authentication:Jwt:IssuerSigningKey"]));
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(2),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

