using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Http;
using SchoolAdmission.Business.Contracts;
using SchoolAdmission.Business.Services;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("classes")]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classes;
    private readonly IHoldService _holds;

    public ClassesController(IClassService classes, IHoldService holds)
    {
        _classes = classes;
        _holds = holds;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateClassRequest request,
        CancellationToken ct)
    {
        var created = await _classes.CreateAsync(request, ct);

        return Created($"/classes/{created.Id}", created); 
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await _classes.ListAsync(ct);

        return Ok(result); 
    }

    [HttpGet("{classId}")]
    public async Task<IActionResult> Get(string classId, CancellationToken ct)
    {
        var result = await _classes.GetAsync(classId, ct);

        return Ok(result); 
    }

    [HttpPost("{classId}/holds")]
    public async Task<IActionResult> CreateHold(
        string classId,
        CancellationToken ct)
    {
        var studentId = StudentIdHeader.Require(Request);

        var hold = await _holds.CreateAsync(classId, studentId, ct);

        return Created($"/holds/{hold.HoldId}", hold); // HTTP 201
    }
}
