using System.Net;
using System.Net.Http.Json;

namespace SchoolAdmission.Tests;

public class HoldRulesTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_class_returns_201_with_contract_fields()
    {
        var response = await _client.PostAsJsonAsync("/classes", new { name = "Class 6 - Morning Shift", totalSeats = 120 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"id\"", json);
        Assert.Contains("\"name\"", json);
        Assert.Contains("\"totalSeats\":120", json);
    }

    [Fact]
    public async Task Apply_when_class_is_full_returns_409()
    {
        var classId = await _client.CreateClassAsync(seats: 1);

        var first = await _client.ApplyAsync(classId, TestApi.NewStudentId());
        var second = await _client.ApplyAsync(classId, TestApi.NewStudentId());

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Contains("NO_SEATS_AVAILABLE", await second.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Same_student_cannot_apply_twice_to_same_class()
    {
        var classId = await _client.CreateClassAsync(seats: 5);
        var student = TestApi.NewStudentId();

        var first = await _client.ApplyAsync(classId, student);
        var second = await _client.ApplyAsync(classId, student);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Contains("ALREADY_APPLIED", await second.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Confirm_twice_counts_only_once()
    {
        var classId = await _client.CreateClassAsync(seats: 5);
        var student = TestApi.NewStudentId();
        var hold = await (await _client.ApplyAsync(classId, student)).Content.ReadFromJsonAsync<HoldDto>();

        var first = await _client.ConfirmAsync(hold!.HoldId, student);
        var second = await _client.ConfirmAsync(hold.HoldId, student);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var cls = await _client.GetClassAsync(classId);
        Assert.Equal(1, cls.ConfirmedSeats);
        Assert.Equal(0, cls.HeldSeats);
    }

    [Fact]
    public async Task Confirm_by_another_student_returns_403()
    {
        var classId = await _client.CreateClassAsync(seats: 5);
        var hold = await (await _client.ApplyAsync(classId, TestApi.NewStudentId())).Content.ReadFromJsonAsync<HoldDto>();

        var response = await _client.ConfirmAsync(hold!.HoldId, TestApi.NewStudentId());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Invalid_student_id_returns_400()
    {
        var classId = await _client.CreateClassAsync(seats: 5);

        var response = await _client.ApplyAsync(classId, "123");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("INVALID_STUDENT_ID", await response.Content.ReadAsStringAsync());
    }
}