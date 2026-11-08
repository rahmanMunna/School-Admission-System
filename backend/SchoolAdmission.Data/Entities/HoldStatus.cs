namespace SchoolAdmission.Data.Entities;

// Upper-case on purpose: these names are stored in the DB and returned in JSON exactly as the spec says.
#pragma warning disable IDE1006 // naming style
public enum HoldStatus
{
    HELD,
    CONFIRMED,
    EXPIRED,
    CANCELLED
}
#pragma warning restore IDE1006