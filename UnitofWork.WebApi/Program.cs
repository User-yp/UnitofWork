using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using UnitofWork.Extensions;
using UnitofWork.WebApi.Data;
using UnitofWork.WebApi.Domain;

var builder = WebApplication.CreateBuilder(args);

// 配置数据库连接
var conn = builder.Configuration
    .GetSection(nameof(ConnectionOption))
    .Get<ConnectionOption>()
    ?? throw new InvalidOperationException("Missing 'ConnectionOption' configuration section.");

// 注册 TestContext 及其事务单元
builder.Services.AddUnitOfWork<TestContext>(opt =>
{
    opt.UseMySql(
        conn.TestConnStr,
        ServerVersion.Create(8, 0, 36, Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql),
        mysqlOpt =>
        {
            mysqlOpt.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
});

// 注册 QuartzContext 及其事务单元
builder.Services.AddUnitOfWork<QuartzContext>(opt =>
{
    opt.UseMySql(
        conn.QuartzConnStr,
        ServerVersion.Create(8, 0, 36, Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql),
        mysqlOpt =>
        {
            mysqlOpt.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
});

// 注册业务服务
builder.Services.AddScoped<DomainService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
