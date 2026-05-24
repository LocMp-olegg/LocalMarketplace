using LocMp.Chat.Domain.Enums;

namespace LocMp.Chat.Api.Requests;

public sealed record CreateChatRequest(
    ChatType Type,
    Guid? TargetUserId,
    string? TargetUserName,
    Guid? ReferenceId,
    string? InitialMessage
);