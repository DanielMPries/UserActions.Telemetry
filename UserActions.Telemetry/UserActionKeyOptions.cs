namespace UserActions.Telemetry;

public enum UserActionKeyOptions {

  /// <summary>
  /// Retrieves the user identifier from the header
  /// </summary>
  FromHeader,

  /// <summary>
  /// Retrieves the user identifier from the context request items collection
  /// </summary>
  FromContext,

  /// <summary>
  /// Retrieves the user identifier from the query string
  /// </summary>
  FromQueryString
}
