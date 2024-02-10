using Microsoft.AspNetCore.Builder;

namespace UserActions.Telemetry;

public static class IApplicationBuilderExtensions {
  public static IApplicationBuilder AddUserActionsMiddleware(this IApplicationBuilder app, UserActionOptions options) {
    return app.UseMiddleware<UserActionsMiddleware>(options);
  }
}
