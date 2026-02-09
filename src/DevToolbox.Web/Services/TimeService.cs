namespace DevToolbox.Web.Services;

/// <summary>
/// Time service interface (AI Accept)
/// </summary>
public interface ITimeService
{
    long GetTimestampSeconds();
    long GetTimestampMilliseconds();
    DateTime TimestampToDateTime(long timestamp, bool isMilliseconds = false);
    long DateTimeToTimestamp(DateTime dateTime, bool toMilliseconds = false);
    string FormatDateTime(DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss");
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
}
