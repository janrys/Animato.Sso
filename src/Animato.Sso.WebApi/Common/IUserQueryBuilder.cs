namespace Animato.Sso.WebApi.Common;

using Animato.Sso.Application.Models;
using Animato.Sso.Domain.Entities;

public interface IUserQueryBuilder
{
    Task<IEnumerable<User>> GetAll();
    Task<User> GetByLogin(string login);
    Task<User> GetById(UserId id);
    Task<IEnumerable<ApplicationRole>> GetRoles(UserId userId);
    Task<IEnumerable<UserClaim>> GetClaims(UserId userId);
    Task<UserInfoResult> GetUserInfo(string accessToken);
}
