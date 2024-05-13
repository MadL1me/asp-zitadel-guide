using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zitadel.Authentication;
using ZitadelExample.Configuration;

namespace ZitadelExample.Controllers;

[ApiController]
[Route("api/debug/v1")]
public class ExampleController : ControllerBase
{
    [HttpPost("non-authorize")]
    public object NonAuthorize() => Result();
    
    [HttpPost("introspect/valid")]
    [Authorize(AuthenticationSchemes = AuthConstants.Schema)]
    public object IntrospectValidToken() => Result();

    [HttpPost("introspect/requires-role")]
    [Authorize(Policy = AuthConstants.PolicyTest)]
    public object IntrospectRequiresRole() => Result();

    private object Result() => new
    {
        Ping = "Pong",
        Timestamp = DateTime.Now,
        AuthType = User.Identity?.AuthenticationType,
        UserName = User.Identity?.Name,
        UserId = User.FindFirstValue(OidcClaimTypes.Subject),
        Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
        IsInAdminRole = User.IsInRole("Admin"),
        IsInUserRole = User.IsInRole("User"),
        InTestRole = User.IsInRole("test"),
    };
}
