using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Humanizer;
using Serilog;
using Serilog.Events;

namespace Organica.Common.Logger.SeriLog
{
  public class Logger : Organica.Common.Logger.ILogger
  {
    private Serilog.ILogger _logger;
    private readonly int _stackFrameCount = 1;
    private readonly Regex anonymMethodNameRegex = new Regex(@"<(?<methodName>\w+)>\w*");

    public Logger(LoggerConfiguration loggerConfiguration, string applicationName, string applicationVersion, int stackFrameCount = 1) : this(loggerConfiguration, LogEventLevel.Verbose, applicationName: applicationName, applicationVersion: applicationVersion, stackFrameCount: stackFrameCount) { }

    public Logger(LoggerConfiguration loggerConfiguration, LogEventLevel minimumLevel = LogEventLevel.Verbose, bool logToSeq = true, string seqUrl = "http://seq.organica.local:5341", string applicationName = null, string applicationVersion = null, int stackFrameCount = 1)
    {
      loggerConfiguration = loggerConfiguration.MinimumLevel.Is(minimumLevel);
      loggerConfiguration = loggerConfiguration.Enrich.With(new ApplicationDetailsEnricher(applicationName, applicationVersion));
      _stackFrameCount = stackFrameCount;

      if (logToSeq)
      {
        loggerConfiguration.WriteTo.Seq(seqUrl);
      }

      _logger = loggerConfiguration.CreateLogger();
#if DEBUG
      Serilog.Debugging.SelfLog.Enable(Console.Out);
#endif
    }

    internal Logger(Serilog.ILogger logger)
    {
      _logger = logger;
    }

    #region Interface implementation

    /// <summary>
    /// Temporary enrichment of a new property
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="propertyValue"></param>
    /// <param name="destructureObjects"></param>
    /// <returns></returns>
    public Organica.Common.Logger.ILogger ForContext(string propertyName, object propertyValue, bool destructureObjects = false) => 
      new Logger(_logger.ForContext(propertyName, propertyValue, destructureObjects));

    public Organica.Common.Logger.ILogger GetLocalLogger() =>
      GetLocalLogger(Guid.NewGuid());

    public Organica.Common.Logger.ILogger GetLocalLogger(Guid guid) =>
      new Logger(_logger.ForContext("guid", guid));

    /// <summary>
    /// Enrich the logger with a new permanent property value
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="propertyValue"></param>
    /// <param name="destructureObjects"></param>
    /// <remarks>Adds a new property to a temporary logger, and assigns this new temp logger to the original one</remarks>
    public void Enrich(string propertyName, object propertyValue, bool destructureObjects = false) => 
      _logger = _logger.ForContext(propertyName, propertyValue, destructureObjects);

    // Since we are using params, it is very inconvenient to add the [CallerMemberName] attribute (as it must have a deafult value)
    // Because of this, we are getting the caller method name the old fashioned way - which is slower
    // And it must be added to all calls, or the compiler might optimise it out

    public void Trace(string messageTemplate, params object[] parameters) => 
      LogEvent(LogEventLevel.Verbose, messageTemplate, parameters);

    public void Trace(Exception ex, string messageTemplate, params object[] parameters) => 
      LogEvent(LogEventLevel.Verbose, ex, messageTemplate, parameters);

    public void Debug(string messageTemplate, params object[] parameters) => 
      LogEvent(LogEventLevel.Debug, messageTemplate, parameters);

    public void Debug(Exception ex, string messageTemplate, params object[] parameters) => 
      LogEvent(LogEventLevel.Debug, ex, messageTemplate, parameters);

    public void Info(string messageTemplate, params object[] parameters) => 
      LogEvent(LogEventLevel.Information, messageTemplate, parameters);

    public void Info(Exception ex, string messageTemplate, params object[] parameters) => 
      LogEvent(LogEventLevel.Information, ex, messageTemplate, parameters);

    public void Warn(string messageTemplate, params object[] parameters) => 
      LogEvent(LogEventLevel.Warning, messageTemplate, parameters);

    public void Warn(Exception ex, string messageTemplate, params object[] parameters) => 
      LogEvent(LogEventLevel.Warning, ex, messageTemplate, parameters);

    public void Error(string messageTemplate, params object[] parameters) => 
      LogEvent(LogEventLevel.Error, messageTemplate, parameters);

    public void Error(Exception ex, string messageTemplate, params object[] parameters) => 
      LogEvent(LogEventLevel.Error, ex, messageTemplate, parameters);

    public void Fatal(string messageTemplate, params object[] parameters) => 
      LogEvent(LogEventLevel.Fatal, messageTemplate, parameters);

    public void Fatal(Exception ex, string messageTemplate, params object[] parameters) => 
      LogEvent(LogEventLevel.Fatal, ex, messageTemplate, parameters);

    public void Measured(string task, Stopwatch stopper) => 
      _logger
        .ForContext("MethodName", GetCallerMemberNameSafely(1))
        .ForContext("elapsed", stopper.ElapsedMilliseconds)
        .Write(LogEventLevel.Verbose, "Stopper {task} measured {elapsedHumanized}", task, stopper.Elapsed.Humanize());

    public void GeneralError() => 
      LogEvent(LogEventLevel.Error, "Error occured in {MethodName}");

    public void GeneralError(Exception ex) => 
      LogEvent(LogEventLevel.Error, ex, "Error occured in {MethodName}");

    #endregion

    private void LogEvent(LogEventLevel level, string messageTemplate, params object[] parameters) => 
      _logger.ForContext("MethodName", GetCallerMemberNameSafely()).Write(level, messageTemplate, parameters);

    private void LogEvent(LogEventLevel level, Exception ex, string messageTemplate, params object[] parameters) => 
      _logger.ForContext("MethodName", GetCallerMemberNameSafely()).Write(level, ex, messageTemplate, parameters);

    /// <summary>
    /// Will traverse the stack frame, and find the _stackFrameCount frame, or last one, if the _stackFrameCount is null
    /// </summary>
    /// <param name="offset">The stack frame offset to move to get the calling method name</param>
    /// <returns></returns>
    /// <remarks>offset's default value is 2, as it is mostly called from LogEvent
    /// GetCallerMemberNameSafely is not part of the StackFrame, when called inside this method
    /// The 1st layer is LogEvent
    /// The 2nd layer is the actual logging method called in this class</remarks>
    internal string GetCallerMemberNameSafely(int offset = 2)
    {
      // must increase the stack frame count, as this method is adding 1 to it
      var sfc = _stackFrameCount + offset;
      //var sf = new StackFrame();
      var sf = new StackFrame(sfc);

      while (sf.GetMethod() == null)
      {
        sfc--;
        sf = new StackFrame(sfc);
      }
      var callerMethod = sf.GetMethod();
      var callerMethodName = callerMethod.Name;

      if (callerMethodName == "MoveNext")
      {
        if (callerMethod.ReflectedType != null)
        {
          callerMethodName = callerMethod.ReflectedType.Name; // MoveNext is called from an anonymus method, and ReflectedType contains the calling method's name, with <methodname>x_x format (anonym...)
          var match = anonymMethodNameRegex.Match(callerMethodName);
          if (match.Success)
            callerMethodName = match.Groups["methodName"].Value;
        }
          
      }
      return callerMethodName;
    }

    public void Dispose() => 
      Serilog.Log.CloseAndFlush();

    ~Logger() => 
      Serilog.Log.CloseAndFlush();
  }
}
