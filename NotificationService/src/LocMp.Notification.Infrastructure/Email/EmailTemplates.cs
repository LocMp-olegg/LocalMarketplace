using System.Reflection;

namespace LocMp.Notification.Infrastructure.Email;

internal static class EmailTemplates
{
    private static readonly Assembly Assembly = typeof(EmailTemplates).Assembly;

    // ── Order ──────────────────────────────────────────────────────────────────

    public static (string Subject, string Body) OrderPlaced(decimal total, Guid orderId, string? actionUrl = null) =>
        ("Новый заказ",
         Render("OrderPlaced", new()
         {
             ["total"]        = total.ToString("N2"),
             ["orderId"]      = orderId.ToShortId(),
             ["actionButton"] = Button(actionUrl, "Смотреть заказы"),
         }));

    public static (string Subject, string Body) OrderStatusChanged(string statusText, Guid orderId, string? actionUrl = null) =>
        ($"Заказ {statusText}",
         Render("OrderStatusChanged", new()
         {
             ["statusText"]   = statusText,
             ["orderId"]      = orderId.ToShortId(),
             ["actionButton"] = Button(actionUrl, "Смотреть заказ"),
         }));

    public static (string Subject, string Body) OrderCompleted(Guid orderId, string? actionUrl = null) =>
        ("Заказ выполнен",
         Render("OrderCompleted", new()
         {
             ["orderId"]      = orderId.ToShortId(),
             ["actionButton"] = Button(actionUrl, "Смотреть заказ"),
         }));

    // ── Dispute ───────────────────────────────────────────────────────────────

    public static (string Subject, string Body) DisputeOpened(Guid orderId, string? actionUrl = null) =>
        ("Открыт спор по заказу",
         Render("DisputeOpened", new()
         {
             ["orderId"]      = orderId.ToShortId(),
             ["actionButton"] = Button(actionUrl, "Детали заказа"),
         }));

    public static (string Subject, string Body) DisputeResolved(Guid orderId, string outcome, string? actionUrl = null) =>
        ("Спор завершён",
         Render("DisputeResolved", new()
         {
             ["orderId"]      = orderId.ToShortId(),
             ["outcome"]      = outcome,
             ["actionButton"] = Button(actionUrl, "Детали заказа"),
         }));

    // ── Review ────────────────────────────────────────────────────────────────

    public static (string Subject, string Body) ReviewCreated(
        int rating, string subjectType, string reviewUrl, string? subjectName = null, string? productUrl = null) =>
        ("Новый отзыв",
         Render("ReviewCreated", new()
         {
             ["rating"]       = rating.ToString(),
             ["subjectLabel"] = (subjectType, subjectName, productUrl) switch
             {
                 ("Product", { } n, { } url) =>
                     $"ваш товар <a href=\"{url}\" style=\"color:#2a9d8f;font-weight:600;text-decoration:none;\">«{n}»</a>",
                 ("Product", { } n, null) =>
                     $"ваш товар <strong style=\"color:#264653;\">«{n}»</strong>",
                 ("Product", null, { } url) =>
                     $"<a href=\"{url}\" style=\"color:#2a9d8f;font-weight:600;text-decoration:none;\">ваш товар</a>",
                 ("Product", null, null) => "ваш товар",
                 ("Courier", _, _)       => "услугу курьера",
                 _                       => "профиль продавца",
             },
             ["actionButton"] = Button(reviewUrl, "Смотреть отзыв"),
         }));

    // ── Stock ─────────────────────────────────────────────────────────────────

    public static (string Subject, string Body) StockDepleted(string productName, string? actionUrl = null) =>
        ($"Товар «{productName}» закончился",
         Render("StockDepleted", new()
         {
             ["productName"]  = productName,
             ["actionButton"] = Button(actionUrl, "Редактировать товар"),
         }));

    public static (string Subject, string Body) ProductRestocked(string productName, string? actionUrl = null) =>
        ($"Товар «{productName}» снова в наличии",
         Render("ProductRestocked", new()
         {
             ["productName"]  = productName,
             ["actionButton"] = Button(actionUrl, "Смотреть товар"),
         }));

    // ── Account ───────────────────────────────────────────────────────────────

    public static (string Subject, string Body) SellerActivated(string displayName, string? actionUrl = null) =>
        ("Аккаунт продавца активирован",
         Render("SellerActivated", new()
         {
             ["displayName"]  = displayName,
             ["actionButton"] = Button(actionUrl, "Мои магазины"),
         }));

    public static (string Subject, string Body) AccountBlocked(DateTimeOffset blockedUntil) =>
        ("Аккаунт заблокирован",
         Render("AccountBlocked", new()
         {
             ["blockedUntil"] = blockedUntil.ToString("dd.MM.yyyy HH:mm"),
             ["actionButton"] = "",
         }));

    public static (string Subject, string Body) AccountUnblocked() =>
        ("Аккаунт разблокирован",
         Render("AccountUnblocked", new()
         {
             ["actionButton"] = "",
         }));

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string Button(string? url, string label) =>
        url is null ? "" : $"""
            <table cellpadding="0" cellspacing="0" style="margin:20px auto 0;">
              <tr>
                <td style="border-radius:12px;background:#2a9d8f;text-align:center;">
                  <a href="{url}" style="display:inline-block;padding:13px 36px;color:#ffffff;font-size:15px;font-weight:600;text-decoration:none;">{label}</a>
                </td>
              </tr>
            </table>
            """;

    private static string Render(string templateName, Dictionary<string, string> vars)
    {
        var html = Load(templateName);
        foreach (var (key, value) in vars)
            html = html.Replace("{{" + key + "}}", value);
        return html;
    }

    private static string Load(string templateName)
    {
        var resourceName = $"LocMp.Notification.Infrastructure.Email.Templates.{templateName}.html";
        using var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Email template '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
