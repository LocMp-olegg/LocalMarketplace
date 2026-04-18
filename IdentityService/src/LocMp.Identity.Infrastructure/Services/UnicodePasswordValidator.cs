using LocMp.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Infrastructure.Services;

public sealed class UnicodePasswordValidator : IPasswordValidator<ApplicationUser>
{
    public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
    {
        if (string.IsNullOrEmpty(password))
            return Task.FromResult(IdentityResult.Success);

        var errors = new List<IdentityError>();
        var opts = manager.Options.Password;

        if (password.Length < opts.RequiredLength)
            errors.Add(new IdentityError
            {
                Code = "PasswordTooShort",
                Description = $"Пароль должен содержать не менее {opts.RequiredLength} символов."
            });

        if (opts.RequireDigit && !password.Any(char.IsDigit))
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresDigit",
                Description = "Пароль должен содержать хотя бы одну цифру."
            });

        if (opts.RequireUppercase && !password.Any(char.IsUpper))
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresUpper",
                Description = "Пароль должен содержать хотя бы одну заглавную букву (латинскую или кириллическую)."
            });

        if (opts.RequireLowercase && !password.Any(char.IsLower))
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresLower",
                Description = "Пароль должен содержать хотя бы одну строчную букву (латинскую или кириллическую)."
            });

        if (opts.RequireNonAlphanumeric && !password.Any(c => !char.IsLetterOrDigit(c)))
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresNonAlphanumeric",
                Description = "Пароль должен содержать хотя бы один специальный символ."
            });

        return Task.FromResult(errors.Count == 0
            ? IdentityResult.Success
            : IdentityResult.Failed([.. errors]));
    }
}
