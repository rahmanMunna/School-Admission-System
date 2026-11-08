namespace SchoolAdmission.Data.Entities;

// One row per class+shift, e.g. "Class 6 – Morning Shift".
// Named SchoolClass because "class" is a C# keyword. The table is still "classes".
public class SchoolClass
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public DateTime CreatedAt { get; set; }          // always UTC

    public List<Hold> Holds { get; set; } = [];
}