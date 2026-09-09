using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace SkyBook.Api.Services;

public static class ControllerUserExtensions
{
    /// <summary>
    /// Reads the current user's id from the "sub" claim of their JWT
    /// (see JwtTokenService.GenerateToken). Every [Authorize]-protected
    /// endpoint uses this instead of trusting a user id from the request
    /// body, so a caller can never act on someone else's data.
    /// </summary>
    public static Guid GetUserId(this ControllerBase controller)
    {
        var sub = controller.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (sub == null || !Guid.TryParse(sub, out var id))
        {
            throw new UnauthorizedAccessException("Missing or invalid user id claim.");
        }
        return id;
    }
}
