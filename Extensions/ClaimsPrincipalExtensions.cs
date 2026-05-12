using System.Security.Claims;

namespace CheckinLog.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal user, string permissionKey)
        {
            if (user == null) return false;

            // Esta linha verifica se o usuário logado tem o "carimbo" da permissão
            return user.HasClaim("Permission", permissionKey);
        }
    }
}