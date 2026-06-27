using Microsoft.AspNetCore.Authorization;

namespace CreditDefault.Api.Authorization
{
    public class AuthorizePermissionAttribute : AuthorizeAttribute
    {
        public AuthorizePermissionAttribute(string permission)
        {
            Policy = $"{PermissionAuthorizationPolicyProvider.PolicyPrefix}{permission}";
        }
    }
}
