using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Microsoft.Owin.Security.OAuth;

namespace Benefit.RestApi.Providers
{
    public class ReaderLicenseKeyAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
            Seller seller = null;
            using (var db = new ApplicationDbContext())
            {
                seller = await db.Sellers.FirstOrDefaultAsync(entry=>entry.TerminalLicense == context.UserName);

                if (seller == null)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }
            }

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, seller.Id));
            identity.AddClaim(new Claim("sub", seller.Id));
            identity.AddClaim(new Claim("role", "user"));
            context.Validated(identity);
        }
    }
}