using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Http;
using SchoolAdmission.Business.Services;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("me")]
public class MeController(IHoldService holds) : ControllerBase
{
    // 8. GET /me/holds  (header required)
    [HttpGet("holds")]
    public async Task<IActionResult> MyHolds(CancellationToken ct) =>
        Ok(await holds.ListMineAsync(StudentIdHeader.Require(Request), ct));
}