using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SplitBill.Api.Controllers;

public abstract class BaseController : ControllerBase
{
    protected int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
