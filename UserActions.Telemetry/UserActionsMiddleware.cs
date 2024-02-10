using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace UserActions.Telemetry;

public class UserActionsMiddleware{

  private readonly RequestDelegate _next;
  private readonly UserActionOptions _options;

  public UserActionsMiddleware(UserActionOptions options, RequestDelegate next) {
    _next = next;
    _options = options;
  }

  public async Task InvokeAsync(HttpContext context, IUserActionRepository userActionRepository) {

    var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
    var attribute = endpoint?.Metadata.GetMetadata<UserActionAttribute>();

    // short circuit if no attribute
    if(attribute is null) {
      await _next(context);
      return;
    }

    string userIdentifier = "";

    // Fetch the user identifier
    switch(_options.UserActionKeyOptions) {
      case UserActionKeyOptions.FromHeader:
        if(context.Request.Headers.ContainsKey(_options.UserActionKey)) {
          userIdentifier = context.Request.Headers[_options.UserActionKey];
        }
        break;
      case UserActionKeyOptions.FromContext:
        if(context.Items.ContainsKey(_options.UserActionKey)) {
          userIdentifier = context.Items.First(item => item.Key as string == _options.UserActionKey).Value.ToString();
        }
        break;
      case UserActionKeyOptions.FromQueryString:
        if(context.Request.Query.ContainsKey(_options.UserActionKey)) {
          userIdentifier = context.Request.Query[_options.UserActionKey];
        }
        break;
    }
    string key = $"{_options.UserActionKeyPrefix}{userIdentifier}";

    var data = new Dictionary<string, string>();

    // All Headers
    if(attribute.HeaderOptions == DataRetrievalOptions.All) {
      foreach(var header in context.Request.Headers) {
        data.Add(header.Key, header.Value);
      }
    }

    // Some Headers
    if(attribute.HeaderOptions == DataRetrievalOptions.Some) {
      foreach(var header in context.Request.Headers) {
        if(attribute.Headers.Contains(header.Key)) {
          data.Add(header.Key, header.Value);
        }
      }
    }

    // All Query Params
    if(attribute.QueryParamOptions == DataRetrievalOptions.All) {
      foreach(var queryParam in context.Request.Query) {
        data.Add(queryParam.Key, queryParam.Value);
      }
    }

    // Some Query Params
    if(attribute.QueryParamOptions == DataRetrievalOptions.Some) {
      foreach(var queryParam in context.Request.Query) {
        if(attribute.QueryParams.Contains(queryParam.Key)) {
          data.Add(queryParam.Key, queryParam.Value);
        }
      }
    }

    // consider a strategy pattern to inject other ways of retrieving data (e.g. from the request body)
    // maybe inject those patterns into the attribute constructor rather than the middleware
    data.Add("status", "started");
    await userActionRepository.AddUserActionAsync(key, attribute.Action, data);

    await _next(context);

    try {
      await userActionRepository.AddUserActionAsync(key, attribute.Action, new Dictionary<string, string>{{"status", "completed"}});
    } catch(Exception) {
      // this is just a best effort to capture that the process failed, its not a handler and will be rethrown
      await userActionRepository.AddUserActionAsync(key, attribute.Action, new Dictionary<string, string>{{"status", "faulted"}});
      throw;
    }
  }
}
