using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WebMotions.Fake.Authentication.JwtBearer;

namespace AuctionService.IntegrationTests;

// this will create a instance of our web app, and we gon able to add services inside here, and we can able to reuse that amongst each of the service
public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime //the type program is from auction service prog.cs
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services => //means inside our svc prog.cs file and 1st thing remove the Db context instead use the postgres test container
        {
            services.RemoveDbContext<AuctionDbContext>();  //remove db context from service collection extns/utils

            services.AddDbContext<AuctionDbContext>(options =>
            {
                options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
            });

            services.AddMassTransitTestHarness();

            services.EnsureCreated<AuctionDbContext>(); //from service service collection extns/utils

            services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme) //to mock the authentication
                .AddFakeJwtBearer(opt =>
                {
                    opt.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
                });
        });
    }

    // dispose or remove the cust webapp factory once the task is completed 
    Task IAsyncLifetime.DisposeAsync() => _postgreSqlContainer.DisposeAsync().AsTask();
}

internal class PostgresSqlContainer
{
}