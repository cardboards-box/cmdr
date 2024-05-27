namespace Commander.Database;

using Commander.Models;
using Handlers;
using Services;

public static class DiExtensions
{
    public static IDependencyResolver AddDatabase(this IDependencyResolver resolver)
    {
        return resolver
            .Add<IRequestLogDbService, RequestLogDbService, RequestLog>()
            .Add<IRoleDbService, RoleDbService, Role>()

            .Transient<IDbService, DbService>()

            .Mapping(c => c
                .TypeHandler<Guid[], GuidArrayHandler>());
    }

    private static IDependencyResolver Add<TInterface, TConcrete, TModel>(this IDependencyResolver resolver)
        where TInterface : class, IOrmMap<TModel>
        where TConcrete : class, TInterface
        where TModel : DbObject
    {
        return resolver
            .Model<TModel>()
            .Transient<TInterface, TConcrete>();
    }

    private static ITypeMapBuilder Enum<T>(this ITypeMapBuilder builder)
        where T : struct, Enum
    {
        return builder.TypeHandler<T, EnumHandler<T>>();
    }
}
