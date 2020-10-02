using System;
using System.Diagnostics;

namespace Organica.Common.Logger
{
  public interface ILogger : IDisposable
  {
    void Enrich(string propertyName, object propertyValue, bool destructureObjects = false);
    ILogger ForContext(string propertyName, object propertyValue, bool destructureObjects = false);
    ILogger GetLocalLogger();
    ILogger GetLocalLogger(Guid guid);

    void Trace(string messageTemplate, params object[] parameters);
    void Trace(Exception ex, string messageTemplate, params object[] parameters);
    void Debug(string messageTemplate, params object[] parameters);
    void Debug(Exception ex, string messageTemplate, params object[] parameters);
    void Info(string messageTemplate, params object[] parameters);
    void Info(Exception ex, string messageTemplate, params object[] parameters);
    void Warn(string messageTemplate, params object[] parameters);
    void Warn(Exception ex, string messageTemplate, params object[] parameters);
    void Error(string messageTemplate, params object[] parameters);
    void Error(Exception ex, string messageTemplate, params object[] parameters);
    void Fatal(string messageTemplate, params object[] parameters);
    void Fatal(Exception ex, string messageTemplate, params object[] parameters);
    void Measured(string task, Stopwatch stopper);
    void GeneralError();
    void GeneralError(Exception ex);
  }
}
