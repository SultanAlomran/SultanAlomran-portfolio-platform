namespace Portfolio.Application.Contact;

public sealed record ContactAnalyticsDto(
    int TotalMessages,
    int NewMessages,
    int ReadMessages,
    int ArchivedMessages,
    int MessagesThisMonth,
    double? AverageResponseTimeHours,
    IReadOnlyList<DailyMessageTrendDto> Trend,
    IReadOnlyList<TopSubjectDto> TopSubjects);

public sealed record DailyMessageTrendDto(
    DateOnly Date,
    int Count);

public sealed record TopSubjectDto(
    string Subject,
    int Count);
