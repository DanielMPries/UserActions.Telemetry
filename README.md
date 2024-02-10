# UserActions.Telemetry

This project provides middleware for endpoints to be decorated with attributes to be captured that are associated with user actions in cases where the application logs and user activity logs have different storage and capture requirements. 

The use case that origionated this is where distributed tracing application logs are sent to a common observabilty platform via OTEL; however, user actions/activities events need to be captured into a different sink with a lower fidelity.

In OTEL situations, the data valume is higher and retention periods is shorter than what is needed.  This aggregation of user events is also captured for appending activites that are not related to application health and distributed transctions but rather common events like, `Sign In`, `Sign Out`, `One Time Passcode`, etc.

When an action is sent, it includes a `status` property of `started`.  On return a new message is fired with a `status` property of `completed`.  If an exception occurs, an action is sent with the status of `faulted` and the exception is rethrown.  The middleware is not responsible for handling faulted events, but rather ensuring that its captured.

## UserActions.Telemetry.Redis

This implementation uses a Redis stream as an event sink.  If a client id is supplied, this can build a user specific stream of events.

This example includes a `docker-compose.yaml` setup for a local redis cluster to play with.  In a production situation, you'd connect your multiplexer to your production ready instances.  In your terminal `docker-compose up -d` will spin up redis and make it available via the `localhost` on port `6379`.

### To Use 

In your `Program.cs`, include a reference to the nuget `StackExchange.Redis`

```csharp
// setup a connection to redis
var multiplexer = ConnectionMultiplexer.Connect("localhost:6379");
// register the multiplexer as a singleton
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
// instrument DI for the IUserActionRepository to use a concrete implementation using redis
builder.Services.AddSingleton<IUserActionRepository, RedisUserActionRepository>();


var app = builder.Build();

app.AddUserActionsMiddleware(new UserActionOptions());
```

Endpoints can then be annotated with a `UserActionAttribute`. The `UserActionAttribute` in supports a few ways to capture from the headers, query params or from the items collected in the `HttpContext` items collection.

```csharp
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
```

## TODO

```
[ ] - Complete setup for nuget packages
[ ] - Unit tests for data capture from context
[ ] - Concrete implementations for other sinks like RabbitMQ, Azure Event Hubs and Kafka
[ ] - Additional optional handling on `faulted` states
[ ] - Consider extending attributes that are not exclusive to endpoints
```

