namespace LocMp.Chat.Api.Requests;

public sealed class SendMessageRequest
{
    public string? Body { get; set; }
    public IFormFileCollection? Attachments { get; set; }
}