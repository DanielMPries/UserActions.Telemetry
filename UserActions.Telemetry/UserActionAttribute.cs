namespace UserActions.Telemetry;

[AttributeUsage(AttributeTargets.Method)]
public class UserActionAttribute : Attribute {
  public string Action { get; set; }
  public List<string> Headers { get; } = [];
  public DataRetrievalOptions HeaderOptions { get; } = DataRetrievalOptions.None;
  public List<string> QueryParams { get; } = [];
  public DataRetrievalOptions QueryParamOptions { get; } = DataRetrievalOptions.None;
  private const string headerParamPrefix = "header:";
  private const string queryParamPrefix = "queryParam:";
  public UserActionAttribute(string action, params string[] contextValues):
    this(action, DataRetrievalOptions.Some, DataRetrievalOptions.Some, contextValues) {}

  public UserActionAttribute(
    string action,
    DataRetrievalOptions headerOptions = DataRetrievalOptions.None,
    DataRetrievalOptions queryParamOptions = DataRetrievalOptions.None,
    params string[] contextValues) {

    Action = action;
    HeaderOptions = headerOptions;
    QueryParamOptions = queryParamOptions;

    if(contextValues.Length == 0) return;

    foreach(var contextValue in contextValues) {
      if(contextValue.StartsWith(headerParamPrefix)) {
        Headers.Add(contextValue.Replace(headerParamPrefix, ""));
        continue;
      }

      if(contextValue.StartsWith(queryParamPrefix)) {
        QueryParams.Add(contextValue.Replace(queryParamPrefix, ""));
        continue;
      }
    }
  }
}
