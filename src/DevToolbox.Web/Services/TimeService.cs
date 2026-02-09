namespace DevToolbox.Web.Services;

/// <summary>
/// Date difference calculation result (AI Accept)
/// </summary>
public class DateDiffResult
{
    public int Years { get; set; }
    public int Months { get; set; }
    public int Days { get; set; }
    public int TotalDays { get; set; }
    public int TotalWeeks { get; set; }
    public int TotalHours { get; set; }
    public long TotalMinutes { get; set; }
    public long TotalSeconds { get; set; }
    public bool IsNegative { get; set; }
}

/// <summary>
/// Time service interface (AI Accept)
/// </summary>
public interface ITimeService
{
    /// <summary>
    /// Get current timestamp in seconds (AI Accept)
    /// </summary>
    long GetTimestampSeconds();

    /// <summary>
    /// Get current timestamp in milliseconds (AI Accept)
    /// </summary>
    long GetTimestampMilliseconds();

    /// <summary>
    /// Convert timestamp to DateTime (AI Accept)
    /// </summary>
    DateTime TimestampToDateTime(long timestamp, bool isMilliseconds = false);

    /// <summary>
    /// Convert DateTime to timestamp (AI Accept)
    /// </summary>
    long DateTimeToTimestamp(DateTime dateTime, bool toMilliseconds = false);

    /// <summary>
    /// Format DateTime to string (AI Accept)
    /// </summary>
    string FormatDateTime(DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// Calculate the difference between two dates (AI Accept)
    /// </summary>
    DateDiffResult CalculateDateDiff(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Add years/months/weeks/days to a date (AI Accept)
    /// </summary>
    DateTime AddToDate(DateTime baseDate, int years, int months, int weeks, int days, bool subtract = false);
}

/// <summary>
/// Time service implementation (AI Accept)
/// </summary>
public class TimeService : ITimeService
{
    /// <summary>
    /// Get current timestamp in seconds (AI Accept)
    /// </summary>
    public long GetTimestampSeconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    /// <summary>
    /// Get current timestamp in milliseconds (AI Accept)
    /// </summary>
    public long GetTimestampMilliseconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Convert timestamp to DateTime (AI Accept)
    /// </summary>
    public DateTime TimestampToDateTime(long timestamp, bool isMilliseconds = false)
    {
        return isMilliseconds
            ? DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime
            : DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
    }

    /// <summary>
    /// Convert DateTime to timestamp (AI Accept)
    /// </summary>
    public long DateTimeToTimestamp(DateTime dateTime, bool toMilliseconds = false)
    {
        var offset = new DateTimeOffset(dateTime);
        return toMilliseconds
            ? offset.ToUnixTimeMilliseconds()
            : offset.ToUnixTimeSeconds();
    }

    /// <summary>
    /// Format DateTime to string (AI Accept)
    /// </summary>
    public string FormatDateTime(DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss")
    {
        return dateTime.ToString(format);
    }

    /// <summary>
    /// Calculate the difference between two dates with precise year/month/day breakdown (AI Accept)
    /// </summary>
    public DateDiffResult CalculateDateDiff(DateTime startDate, DateTime endDate)
    {
        var result = new DateDiffResult();

        // AI Accept [Added]: Normalize direction, track if negative
        var from = startDate;
        var to = endDate;
        if (from > to)
        {
            (from, to) = (to, from);
            result.IsNegative = true;
        }

        // AI Accept [Added]: Calculate total span first
        var span = to - from;
        result.TotalDays = (int)span.TotalDays;
        result.TotalWeeks = result.TotalDays / 7;
        result.TotalHours = (int)span.TotalHours;
        result.TotalMinutes = (long)span.TotalMinutes;
        result.TotalSeconds = (long)span.TotalSeconds;

        // AI Accept [Added]: Calculate precise year/month/day breakdown
        int years = to.Year - from.Year;
        int months = to.Month - from.Month;
        int days = to.Day - from.Day;

        if (days < 0)
        {
            months--;
            // Get days in the previous month of 'to' date
            var prevMonth = to.AddMonths(-1);
            days += DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
        }

        if (months < 0)
        {
            years--;
            months += 12;
        }

        result.Years = years;
        result.Months = months;
        result.Days = days;

        return result;
    }

    /// <summary>
    /// Add or subtract years/months/weeks/days to a date (AI Accept)
    /// </summary>
    public DateTime AddToDate(DateTime baseDate, int years, int months, int weeks, int days, bool subtract = false)
    {
        // AI Accept [Added]: Apply sign based on subtract flag
        int sign = subtract ? -1 : 1;

        var result = baseDate;
        result = result.AddYears(years * sign);
        result = result.AddMonths(months * sign);
        result = result.AddDays((weeks * 7 + days) * sign);

        return result;
    }
}
