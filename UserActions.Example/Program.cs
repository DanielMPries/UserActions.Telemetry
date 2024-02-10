using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using UserActions.Telemetry;
using UserActions.Telemetry.Redis;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var multiplexer = ConnectionMultiplexer.Connect("localhost:6379");
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
builder.Services.AddSingleton<IUserActionRepository, RedisUserActionRepository>();
builder.Services.Add(ServiceDescriptor.Singleton<IDistributedCache, RedisCache>());

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.AddUserActionsMiddleware(new UserActionOptions());

// User action explicitly setting header and query param options to None
app.MapGet(
  "/",
  [UserAction("Greeting", headerOptions: DataRetrievalOptions.None, queryParamOptions: DataRetrievalOptions.None)]
  () => "Hello World!"
);

// Captures only the defined header(s)
app.MapPost(
  "/sign-in",
  [UserAction("Sign In", headerOptions: DataRetrievalOptions.Some, queryParamOptions: DataRetrievalOptions.None, "header:x-user-id")]
  () => "Signed In"
);

// Captures only the defined query param(s)
app.MapPost(
  "/sign-out",
  [UserAction("Sign Out", headerOptions: DataRetrievalOptions.None, queryParamOptions: DataRetrievalOptions.Some, "queryParam:x-user-id")]
  () => "Signed Out"
);

// Captures all headers and query params
app.MapPost(
  "/testing",
  [UserAction("Testing", headerOptions: DataRetrievalOptions.All, queryParamOptions: DataRetrievalOptions.All)]
  () => "Well done"
);

// User actions without header or query param options will seek either as DataRetrievalOptions.Some
app.MapGet(
  "/health",
  [UserAction("Health Check")]
  () => "Healthy"
);

app.Run();
