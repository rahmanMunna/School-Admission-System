using System.Net;

namespace SchoolAdmission.Tests;

public class ConcurrencyTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Parallel_applications_never_exceed_total_seats()
    {
        const int seats = 10;
        const int requests = 200;
        var classId = await _client.CreateClassAsync(seats);

        // 200 different students apply at the same moment.
        var responses = await Task.WhenAll(
            Enumerable.Range(0, requests)
                      .Select(_ => _client.ApplyAsync(classId, TestApi.NewStudentId())));

        Assert.Equal(seats, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(requests - seats, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));

        var cls = await _client.GetClassAsync(classId);
        Assert.Equal(seats, cls.HeldSeats);
        Assert.Equal(0, cls.AvailableSeats);
    }

    [Fact]
    public async Task Same_student_applying_in_parallel_gets_only_one_hold()
    {
        var classId = await _client.CreateClassAsync(seats: 10);
        var student = TestApi.NewStudentId();

        // One student double-clicks 20 times at once.
        var responses = await Task.WhenAll(
            Enumerable.Range(0, 20).Select(_ => _client.ApplyAsync(classId, student)));

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(19, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
    }
}