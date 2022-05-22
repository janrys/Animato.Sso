namespace Animato.Sso.Infrastructure.Services.Persistence;
using System.Collections.Generic;
using Animato.Sso.Domain.Entities;

public class InMemoryDataContext
{
    public List<User> Users { get; set; }
}
