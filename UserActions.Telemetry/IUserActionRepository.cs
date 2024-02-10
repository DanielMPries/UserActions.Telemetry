namespace UserActions.Telemetry;

public interface IUserActionRepository {
  Task AddUserActionAsync(string Key, string userAction, Dictionary<string, string> contextData);
}
