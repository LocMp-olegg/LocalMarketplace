using FluentValidation.TestHelper;
using LocMp.Order.Application.Orders.Commands.Orders.Checkout;
using LocMp.Order.Domain.Enums;
using Xunit;

namespace LocMp.Order.Application.Tests.Validators;

public sealed class CheckoutCommandValidatorTests
{
    private readonly CheckoutCommandValidator _validator = new();

    private static GroupDeliverySettings PickupGroup(Guid? sellerId = null) => new(
        SellerId: sellerId ?? Guid.NewGuid(),
        ShopId: null,
        DeliveryType: DeliveryType.Pickup,
        DeliveryAddress: null);

    private static GroupDeliverySettings CourierGroup(DeliveryAddressData? address = null) => new(
        SellerId: Guid.NewGuid(),
        ShopId: Guid.NewGuid(),
        DeliveryType: DeliveryType.NeighborCourier,
        DeliveryAddress: address ?? ValidAddress());

    private static DeliveryAddressData ValidAddress() => new(
        City: "Москва",
        Street: "Ленина",
        HouseNumber: "10",
        Apartment: null,
        Entrance: null,
        Floor: null,
        Latitude: 55.75,
        Longitude: 37.62,
        RecipientName: "Иван Иванов",
        RecipientPhone: "79991234567");

    [Fact]
    public void Validate_ValidPickupCommand_PassesValidation()
    {
        var cmd = new CheckoutCommand(
            UserId: Guid.NewGuid(),
            BuyerComment: null,
            Groups: [PickupGroup()]);

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidCourierCommand_PassesValidation()
    {
        var cmd = new CheckoutCommand(
            UserId: Guid.NewGuid(),
            BuyerComment: null,
            Groups: [CourierGroup()]);

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUserId_FailsValidation()
    {
        var cmd = new CheckoutCommand(
            UserId: Guid.Empty,
            BuyerComment: null,
            Groups: [PickupGroup()]);

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_EmptyGroups_FailsValidation()
    {
        var cmd = new CheckoutCommand(
            UserId: Guid.NewGuid(),
            BuyerComment: null,
            Groups: []);

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Groups);
    }

    [Fact]
    public void Validate_CourierDeliveryWithoutAddress_FailsValidation()
    {
        var group = new GroupDeliverySettings(
            SellerId: Guid.NewGuid(),
            ShopId: Guid.NewGuid(),
            DeliveryType: DeliveryType.NeighborCourier,
            DeliveryAddress: null);

        var cmd = new CheckoutCommand(
            UserId: Guid.NewGuid(),
            BuyerComment: null,
            Groups: [group]);

        var result = _validator.TestValidate(cmd);

        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_CourierDeliveryWithEmptyCity_FailsValidation()
    {
        var address = ValidAddress() with { City = "" };
        var cmd = new CheckoutCommand(
            UserId: Guid.NewGuid(),
            BuyerComment: null,
            Groups: [CourierGroup(address)]);

        var result = _validator.TestValidate(cmd);

        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_CourierDeliveryWithEmptyRecipientName_FailsValidation()
    {
        var address = ValidAddress() with { RecipientName = "" };
        var cmd = new CheckoutCommand(
            UserId: Guid.NewGuid(),
            BuyerComment: null,
            Groups: [CourierGroup(address)]);

        var result = _validator.TestValidate(cmd);

        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_EmptySellerIdInGroup_FailsValidation()
    {
        var cmd = new CheckoutCommand(
            UserId: Guid.NewGuid(),
            BuyerComment: null,
            Groups: [PickupGroup(Guid.Empty)]);

        var result = _validator.TestValidate(cmd);

        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_PickupWithoutAddress_PassesValidation()
    {
        var cmd = new CheckoutCommand(
            UserId: Guid.NewGuid(),
            BuyerComment: null,
            Groups: [PickupGroup()]);

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
