namespace Animato.Sso.Infrastructure.Azure.Services.Persistence;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Features.Users;
using Animato.Sso.Domain.Entities;
using Animato.Sso.Domain.Enums;
using Microsoft.Extensions.Logging;

public class AzureTableDataSeeder : IDataSeeder
{
    private readonly AzureTableStorageDataContext dataContext;
    private readonly IPasswordHasher passwordHasher;
    private readonly OidcOptions oidcOptions;
    private readonly ILogger<AzureTableDataSeeder> logger;
    private User testUser;
    private User adminUser;
    private Application testAplication;
    private Application crmAplication;

    private static readonly Guid AdminUserId = Guid.Parse("551845DC-0000-0000-0000-F401AF408965");
    private static readonly Guid TesterUserId = Guid.Parse("661845DC-0000-0000-0000-F401AF408966");

    public AzureTableDataSeeder(AzureTableStorageDataContext dataContext, IPasswordHasher passwordHasher, OidcOptions oidcOptions
        , ILogger<AzureTableDataSeeder> logger)
    {
        this.dataContext = dataContext;
        this.passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        this.oidcOptions = oidcOptions ?? throw new ArgumentNullException(nameof(oidcOptions));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Seed()
    {
        await Clear();
        await SeedUsers();
        await SeedApplications();
        await SeedApplicationRoles();
        await SeedUserApplicationRoles();
    }

    private async Task Clear()
    {
        await dataContext.ThrowExceptionIfTableNotExists(CancellationToken.None);

        await AzureTableStorageDataContext.DeleteAllEntitiesAsync(dataContext.Users);
        await AzureTableStorageDataContext.DeleteAllEntitiesAsync(dataContext.Applications);
    }

    private Task SeedUserApplicationRoles()
    {
        // assing all roles to admin
        foreach (var role in dataContext.ApplicationRoles.Where(r => r.ApplicationId == crmAplication.Id))
        {
            var userApplicationRole = new UserApplicationRole()
            {
                ApplicationRoleId = role.Id,
                UserId = adminUser.Id
            };

            dataContext.UserApplicationRoles.Add(userApplicationRole);
        }

        foreach (var role in dataContext.ApplicationRoles.Where(r => r.ApplicationId == testAplication.Id))
        {
            var userApplicationRole = new UserApplicationRole()
            {
                ApplicationRoleId = role.Id,
                UserId = adminUser.Id
            };

            dataContext.UserApplicationRoles.Add(userApplicationRole);
        }

        // assign all reader roles for test user
        foreach (var role in dataContext.ApplicationRoles.Where(r => r.Name.Contains("reader", StringComparison.InvariantCultureIgnoreCase)))
        {
            var userApplicationRole = new UserApplicationRole()
            {
                ApplicationRoleId = role.Id,
                UserId = testUser.Id
            };

            dataContext.UserApplicationRoles.Add(userApplicationRole);
        }

        logger.LogInformation("User application roles seeded");
        return Task.CompletedTask;
    }

    private Task SeedApplicationRoles()
    {
        foreach (var application in dataContext.Applications)
        {
            var applicationRole = new ApplicationRole()
            {
                Id = ApplicationRoleId.New(),
                ApplicationId = application.Id,
                Name = $"{application.Name}_guest"
            };
            dataContext.ApplicationRoles.Add(applicationRole);

            applicationRole = new ApplicationRole()
            {
                Id = ApplicationRoleId.New(),
                ApplicationId = application.Id,
                Name = $"{application.Name}_reader"
            };
            dataContext.ApplicationRoles.Add(applicationRole);

            applicationRole = new ApplicationRole()
            {
                Id = ApplicationRoleId.New(),
                ApplicationId = application.Id,
                Name = $"{application.Name}_writer"
            };
            dataContext.ApplicationRoles.Add(applicationRole);

            applicationRole = new ApplicationRole()
            {
                Id = ApplicationRoleId.New(),
                ApplicationId = application.Id,
                Name = $"{application.Name}_admin"
            };
            dataContext.ApplicationRoles.Add(applicationRole);
        }

        logger.LogInformation("Application roles seeded");
        return Task.CompletedTask;
    }

    private Task SeedApplications()
    {
        var application = new Application()
        {
            Id = Domain.Entities.ApplicationId.New(),
            Name = "TestApp1",
            Code = "test:app1",
            Secrets = new List<string>(new string[] { "secret1", "secret2" }),
            RedirectUris = new List<string>(new string[] { "https://testapp1.animato.cz", "https://testapp1.animato.com" }),
            AccessTokenExpirationMinutes = oidcOptions.AccessTokenExpirationMinutes,
            RefreshTokenExpirationMinutes = oidcOptions.RefreshTokenExpirationMinutes,
            Use2Fa = false,
            AuthorizationMethod = AuthorizationMethod.Password
        };
        dataContext.Applications.Add(application);
        testAplication = application;

        application = new Application()
        {
            Id = Domain.Entities.ApplicationId.New(),
            Name = "Animato.Crm",
            Code = "animato:crm",
            Secrets = new List<string>(new string[] { "secret1", "secret2" }),
            RedirectUris = new List<string>(new string[] { "https://crm.animato.cz", "https://crm.animato.com" }),
            AccessTokenExpirationMinutes = oidcOptions.AccessTokenExpirationMinutes,
            RefreshTokenExpirationMinutes = oidcOptions.RefreshTokenExpirationMinutes,
            Use2Fa = false,
            AuthorizationMethod = AuthorizationMethod.Password
        };
        dataContext.Applications.Add(application);
        crmAplication = application;

        application = new Application()
        {
            Id = Domain.Entities.ApplicationId.New(),
            Name = "Animato.NoRights",
            Code = "animato:norights",
            Secrets = new List<string>(new string[] { "secret1", "secret2" }),
            RedirectUris = new List<string>(new string[] { "https://norights.animato.cz", "https://norights.animato.com" }),
            AccessTokenExpirationMinutes = oidcOptions.AccessTokenExpirationMinutes,
            RefreshTokenExpirationMinutes = oidcOptions.RefreshTokenExpirationMinutes,
            Use2Fa = false,
            AuthorizationMethod = AuthorizationMethod.Password
        };
        dataContext.Applications.Add(application);

        logger.LogInformation("Applications seeded");
        return Task.CompletedTask;
    }
    private Task SeedUsers()
    {
        var user = new User()
        {
            Id = new UserId(TesterUserId),
            Login = "tester@animato.cz",
            Name = "Tester",
            FullName = "Tester Tester",
            AuthorizationMethod = AuthorizationMethod.Password,
            LastChanged = DateTime.UtcNow,
            TotpSecretKey = TesterUserId.ToString(),
        };
        user.UpdatePasswordAndHash(passwordHasher, "testpass");
        dataContext.Users.Add(user);
        testUser = user;

        user = new User()
        {
            Id = new UserId(AdminUserId),
            Login = "admin@animato.cz",
            Name = "Admin",
            FullName = "Admin Admin",
            AuthorizationMethod = AuthorizationMethod.TotpQrCode,
            LastChanged = DateTime.UtcNow,
            TotpSecretKey = AdminUserId.ToString()
        };
        user.UpdatePasswordAndHash(passwordHasher, "adminpass");
        dataContext.Users.Add(user);
        adminUser = user;

        logger.LogInformation("Users seeded");
        return Task.CompletedTask;
    }
}
