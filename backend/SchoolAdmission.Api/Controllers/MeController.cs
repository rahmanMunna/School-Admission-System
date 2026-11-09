using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Http;
using SchoolAdmission.Business.Services;

namespace SchoolAdmission.Api.Controllers;

//[ApiController]
//[Route("me")]
//public class MeController(IHoldService holds) : ControllerBase
//{
//    [HttpGet("holds")]
//    public async Task<IActionResult> MyHolds(CancellationToken ct) =>
//        Ok(await holds.ListMineAsync(StudentIdHeader.Require(Request), ct));
//}


[ApiController]
[Route("me")]
public class MeController : ControllerBase
{
    private readonly IHoldService _holds;

    // Constructor
    public MeController(IHoldService holds)
    {
        _holds = holds;
    }

    // GET: /me/holds
    [HttpGet("holds")]
    public async Task<IActionResult> MyHolds(CancellationToken ct)
    {
        var studentId = StudentIdHeader.Require(Request);

        var result = await _holds.ListMineAsync(studentId, ct);

        return Ok(result);
    }
}
