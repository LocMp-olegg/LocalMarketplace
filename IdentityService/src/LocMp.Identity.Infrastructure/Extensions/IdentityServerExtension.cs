using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Infrastructure.Options;
using LocMp.Identity.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocMp.Identity.Infrastructure.Extensions;

public static class IdentityServerExtension
{
    public static void AddIdentityServerConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("IdentityServer").Get<IdentityServerSettings>() ??
                       new IdentityServerSettings();

        services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryClients(settings.Clients.Select(c => new Client
            {
                ClientId = c.ClientId,
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                RequireClientSecret = false,
                AllowedScopes = c.AllowedScopes,
                AllowOfflineAccess = c.AllowOfflineAccess,
                AlwaysIncludeUserClaimsInIdToken = true,
                AccessTokenLifetime = c.AccessTokenLifetime,
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding
            }).ToList())
            .AddInMemoryIdentityResources(new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            })
            .AddInMemoryApiScopes(new List<ApiScope>
            {
                new("api"),
                new(IdentityServerConstants.LocalApi.ScopeName)
            })
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<ProfileService>()
            .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
            .AddDeveloperSigningCredential();
    }
}