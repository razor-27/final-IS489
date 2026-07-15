using System;

namespace WariSalud.Core.Utils;

public static class TimeHelper
{
    private static readonly TimeZoneInfo PeruTimeZone = 
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "SA Pacific Standard Time" : "America/Lima");

    /// <summary>
    /// Gets the current time in Peru.
    /// </summary>
    public static DateTime GetPeruTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PeruTimeZone);
    }

    /// <summary>
    /// Converts a Peru local DateTime to UTC for database storage.
    /// </summary>
    public static DateTime ToUtc(DateTime peruTime)
    {
        if (peruTime.Kind == DateTimeKind.Utc) return peruTime;
        
        var unspecified = DateTime.SpecifyKind(peruTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, PeruTimeZone);
    }

    /// <summary>
    /// Converts a UTC DateTime from the database to Peru local time.
    /// </summary>
    public static DateTime ToPeruTime(DateTime utcTime)
    {
        var utc = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, PeruTimeZone);
    }
}
