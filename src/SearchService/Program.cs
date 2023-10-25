using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>(); // since we register our consumer, effectively other consumers that we created(with the same namespace ie searchService) will be automatically register by the auto mapper

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false)); //if other service uses the same consumer, we need to add some extra stuff so its diff from the other services that consuming this consumer.. kebab case(s-d-s), we just only want to prefix the search, ex - search-Auction-created..false - flag .. we don't want to incl formated name.

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseRetry(r =>
        {
            r.Handle<RabbitMqConnectionException>();
            r.Interval(5, TimeSpan.FromSeconds(10));
        });

        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });

        // here we re configuring the retry policy on per endpoint basis
        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, 5)); // 5 times with 5s interval

            e.ConfigureConsumer<AuctionCreatedConsumer>(context); // and this retry config only for the auction created consumer
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    await Policy.Handle<TimeoutException>()
        .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(10))
        .ExecuteAndCaptureAsync(async () =>  await DbInitializer.InitDb(app));

});

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy() //http polly for the synch communication
    => HttpPolicyExtensions
        .HandleTransientHttpError() //the null ref exception.. if the connection refused it will handle that type of errro
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3)); // keep trying every 3 seconds until the auction service alive