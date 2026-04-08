namespace LocMp.BuildingBlocks.Application.Common;

/// <summary>
/// Маркерный интерфейс. Реализовать в Command-классах, которые должны выполняться
/// в транзакции БД. TransactionBehavior в Infrastructure-слое каждого сервиса
/// оборачивает такие команды в BeginTransaction / Commit / Rollback.
/// </summary>
public interface ITransactional;
