using FikaShared.Requests;

namespace FikaWebApp.Models;

public sealed record MessageResponseDto(string Message);

public sealed record DeleteTimerRequestDto(long Ticks);

public sealed record ScheduleSingleRequestDto(SendItemRequest Request, DateTime SendDate);

public sealed record ScheduleAllRequestDto(SendItemToAllRequest Request, DateTime SendDate);

public sealed record QueuedSingleTimerDto(
    long Ticks,
    string ProfileId,
    string ItemTemplate,
    string ItemName,
    int Amount,
    string Message,
    bool FoundInRaid,
    string SendDate
);

public sealed record QueuedAllTimerDto(
    long Ticks,
    string ItemTemplate,
    string ItemName,
    int Amount,
    string Message,
    bool FoundInRaid,
    IEnumerable<string> ProfileIds,
    string SendDate
);

public sealed record QueuedItemsResponseDto(
    IEnumerable<QueuedSingleTimerDto> SingleTimers,
    IEnumerable<QueuedAllTimerDto> AllTimers
);

public sealed record ResolvedItemDto(
    string TemplateId,
    string Name,
    string Description,
    int MaxItems
);

public sealed record DataSearchResultDto(string TemplateId, string Name);