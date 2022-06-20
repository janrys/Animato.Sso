namespace Animato.Sso.Infrastructure.Services.Persistence;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Animato.Sso.Application.Common;
using Animato.Sso.Application.Common.Interfaces;
using Animato.Sso.Application.Features.Users;
using Animato.Sso.Domain.Entities;
using Animato.Sso.Domain.Enums;
using Microsoft.Extensions.Logging;

public class DataSeeder : IDataSeeder
{

    private readonly IPasswordHasher passwordHasher;
    private readonly OidcOptions oidcOptions;
    private readonly IUserRepository userRepository;
    private readonly IApplicationRepository applicationRepository;
    private readonly IApplicationRoleRepository applicationRoleRepository;
    private readonly IDateTimeService dateTime;
    private readonly ILogger<DataSeeder> logger;
    private User testUser;
    private User adminUser;
    private Application testAplication;
    private Application crmAplication;
    private readonly List<Application> seededApplications = new();
    private readonly List<ApplicationRole> seededApplicationRoles = new();

    private static readonly Guid AdminUserId = Guid.Parse("551845DC-0000-0000-0000-F401AF408965");
    private static readonly Guid TesterUserId = Guid.Parse("661845DC-0000-0000-0000-F401AF408966");

    public DataSeeder(IPasswordHasher passwordHasher
        , OidcOptions oidcOptions
        , IUserRepository userRepository
        , IApplicationRepository applicationRepository
        , IApplicationRoleRepository applicationRoleRepository
        , IDateTimeService dateTime
        , ILogger<DataSeeder> logger)
    {
        this.passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        this.oidcOptions = oidcOptions ?? throw new ArgumentNullException(nameof(oidcOptions));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
        this.applicationRoleRepository = applicationRoleRepository ?? throw new ArgumentNullException(nameof(applicationRoleRepository));
        this.dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
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
        await userRepository.ClearRoles(CancellationToken.None);
        await userRepository.Clear(CancellationToken.None);
        await applicationRoleRepository.Clear(CancellationToken.None);
        await applicationRepository.Clear(CancellationToken.None);
    }

    private async Task SeedUserApplicationRoles()
    {
        // assing all roles to admin
        foreach (var role in seededApplicationRoles.Where(r => r.ApplicationId == crmAplication.Id))
        {
            await userRepository.AddUserRole(adminUser.Id, role.Id, CancellationToken.None);
        }

        foreach (var role in seededApplicationRoles.Where(r => r.ApplicationId == testAplication.Id))
        {
            await userRepository.AddUserRole(adminUser.Id, role.Id, CancellationToken.None);
        }

        // assign all reader roles for test user
        foreach (var role in seededApplicationRoles.Where(r => r.Name.Contains("reader", StringComparison.InvariantCultureIgnoreCase)))
        {
            await userRepository.AddUserRole(testUser.Id, role.Id, CancellationToken.None);
        }

        logger.LogInformation("User application roles seeded");
    }

    private async Task SeedApplicationRoles()
    {
        foreach (var application in seededApplications)
        {
            var applicationRole = new ApplicationRole()
            {
                Id = ApplicationRoleId.New(),
                ApplicationId = application.Id,
                Name = $"{application.Name}_guest"
            };
            seededApplicationRoles.Add(await applicationRoleRepository.Create(applicationRole, CancellationToken.None));

            applicationRole = new ApplicationRole()
            {
                Id = ApplicationRoleId.New(),
                ApplicationId = application.Id,
                Name = $"{application.Name}_reader"
            };
            seededApplicationRoles.Add(await applicationRoleRepository.Create(applicationRole, CancellationToken.None));

            applicationRole = new ApplicationRole()
            {
                Id = ApplicationRoleId.New(),
                ApplicationId = application.Id,
                Name = $"{application.Name}_writer"
            };
            seededApplicationRoles.Add(await applicationRoleRepository.Create(applicationRole, CancellationToken.None));

            applicationRole = new ApplicationRole()
            {
                Id = ApplicationRoleId.New(),
                ApplicationId = application.Id,
                Name = $"{application.Name}_admin"
            };
            seededApplicationRoles.Add(await applicationRoleRepository.Create(applicationRole, CancellationToken.None));
        }

        logger.LogInformation("Application roles seeded");
    }

    private async Task SeedApplications()
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
        testAplication = await applicationRepository.Create(application, CancellationToken.None);
        seededApplications.Add(testAplication);

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
        crmAplication = await applicationRepository.Create(application, CancellationToken.None);
        seededApplications.Add(crmAplication);

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
        seededApplications.Add(await applicationRepository.Create(application, CancellationToken.None));

        logger.LogInformation("Applications seeded");
    }
    private async Task SeedUsers()
    {
        var user = new User()
        {
            Login = "tester@animato.cz",
            Name = "Tester",
            FullName = "Tester Tester",
            AuthorizationMethod = AuthorizationMethod.Password,
            LastChanged = dateTime.UtcNow,
            TotpSecretKey = TesterUserId.ToString(),
        };
        user.UpdatePasswordAndHash(passwordHasher, "testpass");
        testUser = await userRepository.Create(user, new UserId(TesterUserId), CancellationToken.None);

        user = new User()
        {
            Login = "admin@animato.cz",
            Name = "Admin",
            FullName = "Admin Admin",
            AuthorizationMethod = AuthorizationMethod.TotpQrCode,
            LastChanged = dateTime.UtcNow,
            TotpSecretKey = AdminUserId.ToString()
        };
        user.UpdatePasswordAndHash(passwordHasher, "adminpass");
        adminUser = await userRepository.Create(user, new UserId(AdminUserId), CancellationToken.None);

        logger.LogInformation("Users seeded");
    }
}
