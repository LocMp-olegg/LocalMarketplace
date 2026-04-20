namespace LocMp.Notification.Infrastructure.Email;

internal static class EmailTemplates
{
    private const string BaseStyle = """
                                     font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;
                                     background: #f9f9f9; border-radius: 8px; overflow: hidden;
                                     """;

    public static string Wrap(string title, string bodyHtml) => $"""
                                                                 <!DOCTYPE html>
                                                                 <html lang="ru">
                                                                 <head><meta charset="utf-8"><title>{title}</title></head>
                                                                 <body style="margin:0;padding:16px;background:#ececec;">
                                                                   <div style="{BaseStyle}">
                                                                     <div style="background:#4f46e5;padding:20px 24px;">
                                                                       <span style="color:#fff;font-size:18px;font-weight:bold;">Районный Маркетплейс</span>
                                                                     </div>
                                                                     <div style="padding:24px;background:#fff;">
                                                                       {bodyHtml}
                                                                     </div>
                                                                     <div style="padding:12px 24px;background:#f3f3f3;font-size:12px;color:#888;text-align:center;">
                                                                       Это автоматическое письмо. Не отвечайте на него.
                                                                     </div>
                                                                   </div>
                                                                 </body>
                                                                 </html>
                                                                 """;

    // ── Order ──────────────────────────────────────────────────────────────────

    public static (string Subject, string Body) OrderPlaced(decimal total, Guid orderId) =>
        ("Новый заказ", Wrap("Новый заказ", $"""
                                             <h2 style="margin-top:0;">Новый заказ</h2>
                                             <p>На вашем аккаунте продавца оформлен новый заказ на сумму <strong>{total:N2} ₽</strong>.</p>
                                             <p>Номер заказа: <code>{orderId}</code></p>
                                             <p>Пожалуйста, подтвердите его в ближайшее время.</p>
                                             """));

    public static (string Subject, string Body) OrderStatusChanged(string statusText, Guid orderId) =>
        ($"Заказ {statusText}", Wrap("Статус заказа изменён", $"""
                                                               <h2 style="margin-top:0;">Статус заказа изменён</h2>
                                                               <p>Статус вашего заказа <code>{orderId}</code> изменён: <strong>{statusText}</strong>.</p>
                                                               """));

    public static (string Subject, string Body) OrderCompleted(Guid orderId) =>
        ("Заказ выполнен", Wrap("Заказ выполнен", $"""
                                                   <h2 style="margin-top:0;">Заказ выполнен</h2>
                                                   <p>Ваш заказ <code>{orderId}</code> успешно получен. Спасибо за покупку!</p>
                                                   <p>Поделитесь впечатлением — оставьте отзыв о продавце.</p>
                                                   """));

    // ── Dispute ───────────────────────────────────────────────────────────────

    public static (string Subject, string Body) DisputeOpened(Guid orderId) =>
        ("Открыт спор по заказу", Wrap("Открыт спор", $"""
                                                       <h2 style="margin-top:0;">По вашему заказу открыт спор</h2>
                                                       <p>Спор по заказу <code>{orderId}</code> был открыт. Мы рассмотрим ситуацию и свяжемся с вами.</p>
                                                       """));

    public static (string Subject, string Body) DisputeResolved(Guid orderId, string outcome) =>
        ("Спор завершён", Wrap("Спор завершён", $"""
                                                 <h2 style="margin-top:0;">Спор по заказу завершён</h2>
                                                 <p>Спор по заказу <code>{orderId}</code> завершён.</p>
                                                 <p>Решение: <strong>{outcome}</strong></p>
                                                 """));

    // ── Review ────────────────────────────────────────────────────────────────

    public static (string Subject, string Body) ReviewCreated(int rating) =>
        ("Новый отзыв", Wrap("Новый отзыв", $"""
                                             <h2 style="margin-top:0;">Вы получили новый отзыв</h2>
                                             <p>Покупатель оставил отзыв с оценкой <strong>{rating} ★</strong>.</p>
                                             """));

    // ── Stock ─────────────────────────────────────────────────────────────────

    public static (string Subject, string Body) StockDepleted(string productName) =>
        ($"Товар «{productName}» закончился", Wrap("Товар закончился", $"""
                                                                        <h2 style="margin-top:0;">Товар закончился</h2>
                                                                        <p>Остаток товара <strong>«{productName}»</strong> достиг нуля.</p>
                                                                        <p>Обновите наличие, чтобы принимать новые заказы.</p>
                                                                        """));

    public static (string Subject, string Body) ProductRestocked(string productName) =>
        ($"Товар «{productName}» снова в наличии", Wrap("Товар в наличии", $"""
                                                                            <h2 style="margin-top:0;">Товар снова в наличии</h2>
                                                                            <p>Товар <strong>«{productName}»</strong>, который вы добавляли в избранное, снова доступен.</p>
                                                                            """));

    // ── Account ───────────────────────────────────────────────────────────────

    public static (string Subject, string Body) SellerActivated(string displayName) =>
        ("Аккаунт продавца активирован", Wrap("Аккаунт продавца активирован", $"""
                                                                               <h2 style="margin-top:0;">Добро пожаловать, {displayName}!</h2>
                                                                               <p>Ваш аккаунт продавца на Районном Маркетплейсе активирован.</p>
                                                                               <p>Теперь вы можете добавлять товары и принимать заказы.</p>
                                                                               """));

    public static (string Subject, string Body) AccountBlocked(DateTimeOffset blockedUntil) =>
        ("Аккаунт заблокирован", Wrap("Аккаунт заблокирован", $"""
                                                               <h2 style="margin-top:0;">Ваш аккаунт заблокирован</h2>
                                                               <p>Ваш аккаунт был заблокирован до <strong>{blockedUntil:dd.MM.yyyy HH:mm}</strong>.</p>
                                                               <p>Если вы считаете это ошибкой, обратитесь в поддержку.</p>
                                                               """));

    public static (string Subject, string Body) AccountUnblocked() =>
        ("Аккаунт разблокирован", Wrap("Аккаунт разблокирован", $"""
                                                                 <h2 style="margin-top:0;">Ваш аккаунт разблокирован</h2>
                                                                 <p>Блокировка снята. Вы снова можете пользоваться платформой.</p>
                                                                 """));
}