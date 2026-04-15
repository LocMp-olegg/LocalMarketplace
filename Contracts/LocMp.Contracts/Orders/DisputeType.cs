namespace LocMp.Contracts.Orders;

public enum DisputeType
{
    NotDelivered = 1, // товар не был доставлен / продавец не вышел на связь
    WrongItem = 2, // прислали не тот товар
    DefectiveItem = 3, // товар бракованный / не соответствует описанию
    CourierIssue = 4, // проблема на стороне курьера (повредил, опоздал)
    Other = 5 // прочее
}