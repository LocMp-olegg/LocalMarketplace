using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Identity;
using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Application.Identity.Commands.Users.RegisterUser;
using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace LocMp.Identity.Application.Tests.Handlers;

public sealed class RegisterUserCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventBus _eventBus;
    private readonly IMapper _mapper;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);

        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _eventBus = Substitute.For<IEventBus>();
        _mapper = Substitute.For<IMapper>();

        _handler = new RegisterUserCommandHandler(_userManager, _db, _eventBus, _mapper);
    }

    public void Dispose() => _db.Dispose();

    private static RegisterUserCommand ValidCommand(bool isSeller = false) => new(
        UserName: "ivanov123",
        Email: "ivan@example.com",
        Password: "Secret123!",
        FirstName: "Иван",
        LastName: "Иванов",
        PhoneNumber: null,
        Gender: null,
        BirthDate: null,
        IsSeller: isSeller);

    private void SetupSuccessfulUserCreation(string? phoneNumber = null)
    {
        _userManager.Users.Returns(
            Array.Empty<ApplicationUser>().AsQueryable());

        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);

        _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);

        _userManager.GetRolesAsync(Arg.Any<ApplicationUser>())
            .Returns(["User"]);

        _mapper.Map<UserDto>(Arg.Any<ApplicationUser>())
            .Returns(new UserDto { UserName = "ivanov123", Email = "ivan@example.com" });
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesUserRegisteredEvent()
    {
        SetupSuccessfulUserCreation();
        var cmd = ValidCommand();

        await _handler.Handle(cmd, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<UserRegisteredEvent>(e => e.Email == cmd.Email),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_IsSeller_PublishesUserBecameSellerEvent()
    {
        SetupSuccessfulUserCreation();
        _userManager.GetRolesAsync(Arg.Any<ApplicationUser>())
            .Returns(["User", "Seller"]);

        var cmd = ValidCommand(isSeller: true);

        await _handler.Handle(cmd, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Any<UserBecameSellerEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotSeller_DoesNotPublishUserBecameSellerEvent()
    {
        SetupSuccessfulUserCreation();
        var cmd = ValidCommand(isSeller: false);

        await _handler.Handle(cmd, CancellationToken.None);

        await _eventBus.DidNotReceive().PublishAsync(
            Arg.Any<UserBecameSellerEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CreateAsyncFails_ThrowsConflictException()
    {
        _userManager.Users.Returns(Array.Empty<ApplicationUser>().AsQueryable());
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError
                { Code = "DuplicateEmail", Description = "Email already in use." }));

        var cmd = ValidCommand();

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DuplicatePhone_ThrowsConflictException()
    {
        var existingUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "existing",
            FirstName = "Existing",
            LastName = "User",
            PhoneNumber = "9991234567"
        };
        _db.Users.Add(existingUser);
        await _db.SaveChangesAsync();

        _userManager.Users.Returns(_db.Users);

        var cmd = ValidCommand() with { PhoneNumber = "9991234567" };

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithCoordinates_CreatesUserAddress()
    {
        SetupSuccessfulUserCreation();
        var cmd = ValidCommand() with { Latitude = 55.75, Longitude = 37.62 };

        await _handler.Handle(cmd, CancellationToken.None);

        var address = _db.UserAddresses.SingleOrDefault();
        Assert.NotNull(address);
        Assert.True(address.IsDefault);
    }

    [Fact]
    public async Task Handle_WithoutCoordinates_DoesNotCreateUserAddress()
    {
        SetupSuccessfulUserCreation();
        var cmd = ValidCommand();

        await _handler.Handle(cmd, CancellationToken.None);

        Assert.Empty(_db.UserAddresses);
    }
}
