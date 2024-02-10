namespace UserActions.Telemetry;

public class UserActionOptions {
  public string UserActionKeyPrefix { get; set; } = "user-actions";
  public UserActionKeyOptions UserActionKeyOptions { get; set; } = UserActionKeyOptions.FromHeader;
  public string UserActionKey { get; set; } = "x-client-id";
}
