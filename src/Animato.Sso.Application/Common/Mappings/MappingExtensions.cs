namespace Animato.Sso.Application.Common.Mappings;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Animato.Sso.Application.Models;

public static class MappingExtensions
{
    public static PaginatedList<TDestination> ToPaginatedList<TDestination>(this IQueryable<TDestination> queryable, int pageNumber, int pageSize)
        => PaginatedList<TDestination>.Create(queryable, pageNumber, pageSize);

    public static List<TDestination> ProjectToList<TDestination>(this IQueryable queryable, IConfigurationProvider configuration)
        => queryable.ProjectTo<TDestination>(configuration).ToList();
}
