using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using NotePilot.API.Middleware;
using NotePilot.Repository;
using NotePilot.Repository.Users;
using NotePilot.Service;
using NotePilot.Service.Users;

var builder = WebApplication.CreateBuilder(args);

var assembliesToScan = AppDomain.CurrentDomain.GetAssemblies();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserInfo>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
var profileTypes = assembliesToScan
    .SelectMany(assembly => assembly.GetTypes())
    .Where(type => typeof(Profile).IsAssignableFrom(type) && !type.IsAbstract && type.GetConstructors().Any())
    .ToList();
builder.Services.AddAutoMapper(config =>
{
    foreach (var profileType in profileTypes)
    {
        config.AddProfile((Profile)Activator.CreateInstance(profileType)); 
    }
});
builder.Services.AddDbContext<NotePilotDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<UserInfoMiddleware>();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
