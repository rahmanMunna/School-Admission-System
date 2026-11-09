using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SchoolAdmission.Tests;

// Starts the real API in memory, connected to a separate TEST database.
public class ApiFactory : WebApplicationFactory<Program>
{
    public ApiFactory()
    {
        // Environment variables override appsettings, so the API uses the test DB.
        // The database is created automatically by the migration on startup.
        Environment.SetEnvironmentVariable("ConnectionStrings__Default",
            Environment.GetEnvironmentVariable("TEST_DB")
            ?? "Host=localhost;Port=5433;Database=admission_test;Username=admission;Password=admission;Maximum Pool Size=40");

        Environment.SetEnvironmentVariable("HOLD_DURATION_SECONDS", "600");
    }
}

// Our own copies of the JSON shapes. If a field name in the API changes, these tests break.
public record ClassDto(Guid Id, string Name, int TotalSeats, int HeldSeats, int ConfirmedSeats, int AvailableSeats);
public record HoldDto(Guid HoldId, Guid ClassId, string Status, DateTime ExpiresAt);

// Small helpers so the tests stay short and readable.
public static class TestApi
{
    private static long _counter;

    // A new unique 17-digit student ID each time: 2015 + 13 digits.
    public static string NewStudentId() => $"2015{Interlocked.Increment(ref _counter):D13}";

    public static async Task<Guid> CreateClassAsync(this HttpClient client, int seats)
    {
        var response = await client.PostAsJsonAsync("/classes",
            new { name = $"Test class {Guid.NewGuid():N}", totalSeats = seats });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ClassDto>())!.Id;
    }

    public static async Task<ClassDto> GetClassAsync(this HttpClient client, Guid classId) =>
        (await client.GetFromJsonAsync<ClassDto>($"/classes/{classId}"))!;

    public static Task<HttpResponseMessage> ApplyAsync(this HttpClient client, Guid classId, string studentId) =>
        client.SendAsync(WithStudent(HttpMethod.Post, $"/classes/{classId}/holds", studentId));

    public static Task<HttpResponseMessage> ConfirmAsync(this HttpClient client, Guid holdId, string studentId) =>
        client.SendAsync(WithStudent(HttpMethod.Post, $"/holds/{holdId}/confirm", studentId));

    private static HttpRequestMessage WithStudent(HttpMethod method, string url, string studentId)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-Student-Id", studentId);
        return request;
    }
}