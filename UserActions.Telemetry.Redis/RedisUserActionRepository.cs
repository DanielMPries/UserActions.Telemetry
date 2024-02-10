using StackExchange.Redis;

namespace UserActions.Telemetry.Redis;

public class RedisUserActionRepository(IConnectionMultiplexer multiplexer) : IUserActionRepository
{
  private readonly IConnectionMultiplexer _multiplexer = multiplexer;

  public Task AddUserActionAsync(string key, string userAction, Dictionary<string, string> contextData)
  {
    IDatabase db = _multiplexer.GetDatabase();
    contextData.Add("action", userAction);
    contextData.Add("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
    var values = contextData.Select(x => new NameValueEntry(x.Key, x.Value)) ?? [];
    return db.StreamAddAsync(key, values.ToArray());
  }
}
