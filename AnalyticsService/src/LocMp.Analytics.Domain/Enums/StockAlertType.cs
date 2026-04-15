namespace LocMp.Analytics.Domain.Enums;

public enum StockAlertType
{
    OutOfStock = 1, // остаток = 0 (StockDepletedEvent)
    LowStock = 2    // остаток ниже порогового значения
}
