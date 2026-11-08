using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Identity;
using SchoolAdmission.Api.Services;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("me")]
public class MeController(IHoldService holds) : ControllerBase
{
    // 8. GET /me/holds  (header required)
    [HttpGet("holds")]
    public async Task<IActionResult> MyHolds(CancellationToken ct) =>
        Ok(await holds.ListMineAsync(StudentId.Require(Request), ct));
}