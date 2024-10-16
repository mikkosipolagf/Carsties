using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add mongo client with GetPolicy method.  
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());

// Mass transit for rabbitMQ operations.
builder.Services.AddMassTransit(x =>
{
    // Add the consumer to the container. In this case, the AuctionCreatedConsumer.
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    x.AddConsumersFromNamespaceContaining<AuctionUpdatedConsumer>();
    x.AddConsumersFromNamespaceContaining<AuctionDeletedConsumer>();

    // The endpoint name is used to create the queue name. The queue name is used to create the queue in RabbitMQ. Queue is used to store the messages.
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        // A mechanism to handle transient faults in a resilient manner. e.g. if the mongoDB is down.
        // Go ahead and see from rabbitMQ (localhost:15672) how the messages are stored in the queue.
        // If receiving end is down you can find the message from queue named search-auction-created_error.
        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, 5));

            // Configuring AuctionCreatedConsumer to consume the message from the queue.
            // e.g. when the auction is created, the message is sent to the queue from where this consumer pickup the message and process it.
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
            e.ConfigureConsumer<AuctionUpdatedConsumer>(context);
            e.ConfigureConsumer<AuctionDeletedConsumer>(context);
        });
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

try
{
    await DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e);
}

app.Run();

// The method uses Polly to retry connection. Polly handles transient faults in a resilient manner.
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));