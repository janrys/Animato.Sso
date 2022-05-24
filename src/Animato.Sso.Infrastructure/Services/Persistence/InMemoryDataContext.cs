namespace Animato.Sso.Infrastructure.Services.Persistence;

using System.Collections.Generic;
using Animato.Sso.Domain.Entities;

public class InMemoryDataContext
{
    public List<User> Users { get; set; } = new List<User>();
    public List<Application> Applications { get; set; } = new List<Application> { };
    public List<ApplicationRole> ApplicationRoles { get; set; } = new List<ApplicationRole> { };
    public List<UserApplicationRole> UserApplicationRoles { get; set; } = new List<UserApplicationRole> { };
    public List<AuthorizationCode> Codes { get; set; } = new List<AuthorizationCode> { };
}
