using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UnitofWork.Abstractions;
using UnitofWork.Core;

namespace UnitofWork.Extensions;

/// <summary>
/// Unit of Work DI 注册扩展方法。
/// </summary>
public static class UnitOfWorkServiceExtensions
{
    /// <summary>
    /// 注册指定 <typeparamref name="TContext"/> 的数据库上下文及其事务单元。
    /// 核心库不依赖具体数据库提供者 — 调用方在 <paramref name="optionsAction"/> 中配置。
    /// </summary>
    /// <example>
    /// services.AddUnitOfWork&lt;MyContext&gt;(opt => opt.UseMySql(connStr, ServerVersion.Create(8,0,36,MySql)));
    /// </example>
    public static IServiceCollection AddUnitOfWork<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction)
        where TContext : DbContext
    {
        return services.AddUnitOfWork<TContext>((_, opt) => optionsAction(opt));
    }

    /// <summary>
    /// 注册指定 <typeparamref name="TContext"/> 的数据库上下文及其事务单元（可访问 IServiceProvider）。
    /// </summary>
    public static IServiceCollection AddUnitOfWork<TContext>(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> optionsAction)
        where TContext : DbContext
    {
        // 注册 DbContext（Scoped，与请求生命周期一致）
        services.AddDbContext<TContext>(optionsAction);

        // 注册类型化的 UoW
        services.TryAddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();

        // 多上下文协调器（只注册一次）
        services.TryAddScoped<IUnitOfWorkManager, UnitOfWorkManager>();

        return services;
    }

    /// <summary>
    /// 自动发现 <paramref name="assembly"/> 中所有 <see cref="DbContext"/> 子类，
    /// 并用统一的 <paramref name="configureEach"/> 回调注册它们。
    /// </summary>
    /// <example>
    /// services.AddUnitOfWorkFromAssembly(Assembly.GetExecutingAssembly(),
    ///     opt => opt.UseMySql(connectionString, ServerVersion.Create(8,0,36,MySql)));
    /// </example>
    public static IServiceCollection AddUnitOfWorkFromAssembly(
        this IServiceCollection services,
        System.Reflection.Assembly assembly,
        Action<DbContextOptionsBuilder> configureEach)
    {
        var dbContextTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(DbContext)))
            .ToList();

        foreach (var type in dbContextTypes)
        {
            // 通过反射调用 AddUnitOfWork<TContext> 泛型方法
            var method = typeof(UnitOfWorkServiceExtensions)
                .GetMethod(nameof(AddUnitOfWork), new[] { typeof(IServiceCollection), typeof(Action<DbContextOptionsBuilder>) })!
                .MakeGenericMethod(type);

            method.Invoke(null, new object[] { services, configureEach });
        }

        return services;
    }

    /// <summary>
    /// 自动发现 <paramref name="assembly"/> 中所有 <see cref="DbContext"/> 子类，
    /// 并用可访问 IServiceProvider 的 <paramref name="configureEach"/> 回调注册它们。
    /// </summary>
    public static IServiceCollection AddUnitOfWorkFromAssembly(
        this IServiceCollection services,
        System.Reflection.Assembly assembly,
        Action<IServiceProvider, DbContextOptionsBuilder> configureEach)
    {
        var dbContextTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(DbContext)))
            .ToList();

        foreach (var type in dbContextTypes)
        {
            var method = typeof(UnitOfWorkServiceExtensions)
                .GetMethod(nameof(AddUnitOfWork), new[] { typeof(IServiceCollection), typeof(Action<IServiceProvider, DbContextOptionsBuilder>) })!
                .MakeGenericMethod(type);

            method.Invoke(null, new object[] { services, configureEach });
        }

        return services;
    }
}
