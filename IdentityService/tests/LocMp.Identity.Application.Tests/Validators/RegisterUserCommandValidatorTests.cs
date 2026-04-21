using FluentValidation.TestHelper;
using LocMp.Identity.Application.Identity.Commands.Users.RegisterUser;
using Xunit;

namespace LocMp.Identity.Application.Tests.Validators;

public sealed class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    private static RegisterUserCommand ValidCommand() => new(
        UserName: "ivanov123",
        Email: "ivan@example.com",
        Password: "Secret123!",
        FirstName: "Иван",
        LastName: "Иванов",
        Gender: null,
        BirthDate: null,
        PhoneNumber: null,
        IsSeller: false,
        Latitude: null,
        Longitude: null);

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUserName_FailsValidation()
    {
        var cmd = ValidCommand() with { UserName = "" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Validate_TooShortUserName_FailsValidation()
    {
        var cmd = ValidCommand() with { UserName = "ab" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Validate_TooLongUserName_FailsValidation()
    {
        var cmd = ValidCommand() with { UserName = new string('a', 257) };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@tld")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces in@email.com")]
    public void Validate_InvalidEmail_FailsValidation(string email)
    {
        var cmd = ValidCommand() with { Email = email };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_ValidEmail_PassesValidation()
    {
        var cmd = ValidCommand() with { Email = "user.name+tag@sub.domain.com" };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("123456789")]   // 9 digits
    [InlineData("12345678901")] // 11 digits
    [InlineData("abc1234567")]  // letters
    public void Validate_InvalidPhoneNumber_FailsValidation(string phone)
    {
        var cmd = ValidCommand() with { PhoneNumber = phone };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Validate_ValidPhoneNumber_PassesValidation()
    {
        var cmd = ValidCommand() with { PhoneNumber = "9991234567" };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Validate_NullPhoneNumber_PassesValidation()
    {
        var cmd = ValidCommand() with { PhoneNumber = null };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Validate_EmptyFirstName_FailsValidation()
    {
        var cmd = ValidCommand() with { FirstName = "" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Validate_FutureBirthDate_FailsValidation()
    {
        var cmd = ValidCommand() with
        {
            BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
        };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.BirthDate);
    }

    [Fact]
    public void Validate_PastBirthDate_PassesValidation()
    {
        var cmd = ValidCommand() with
        {
            BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20))
        };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.BirthDate);
    }

    [Theory]
    [InlineData(-91.0)]
    [InlineData(91.0)]
    public void Validate_LatitudeOutOfRange_FailsValidation(double lat)
    {
        var cmd = ValidCommand() with { Latitude = lat };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Latitude);
    }

    [Theory]
    [InlineData(-181.0)]
    [InlineData(181.0)]
    public void Validate_LongitudeOutOfRange_FailsValidation(double lon)
    {
        var cmd = ValidCommand() with { Longitude = lon };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Longitude);
    }

    [Fact]
    public void Validate_ValidCoordinates_PassesValidation()
    {
        var cmd = ValidCommand() with { Latitude = 55.75, Longitude = 37.62 };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.Latitude);
        result.ShouldNotHaveValidationErrorFor(x => x.Longitude);
    }
}
