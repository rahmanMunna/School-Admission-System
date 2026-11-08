using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Http;
using SchoolAdmission.Business.Contracts;
using SchoolAdmission.Business.Services;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("classes")]
public class ClassesController(IClassService classes, IHoldService holds) : ControllerBase
{
    // 1. POST /classes  (admin)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request, CancellationToken ct)
    {
        var created = await classes.CreateAsync(request, ct);
        return Created($"/classes/{created.Id}", created);              // 201
    }

    // 2. GET /classes
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(await classes.ListAsync(ct));                                 // 200

    // 3. GET /classes/{classId}
    [HttpGet("{classId}")]
    public async Task<IActionResult> Get(string classId, CancellationToken ct) =>
        Ok(await classes.GetAsync(classId, ct));                         // 200

    // 4. POST /classes/{classId}/holds  (guardian, header required)
    [HttpPost("{classId}/holds")]
    public async Task<IActionResult> CreateHold(string classId, CancellationToken ct)
    {
        var studentId = StudentIdHeader.Require(Request);
        var hold = await holds.CreateAsync(classId, studentId, ct);
        return Created($"/holds/{hold.HoldId}", hold);                  // 201
    }
}