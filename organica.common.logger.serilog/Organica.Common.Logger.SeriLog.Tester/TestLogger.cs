using System;
using System.Collections;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Serilog.Events;

namespace Organica.Common.Logger.SeriLog.Tester
{
  [TestFixture, TestOf(typeof(Organica.Common.Logger.SeriLog.Logger))]
  public class TestLogger
  {
    private Organica.Common.Logger.SeriLog.Logger _sub;
    private Mock<Serilog.ILogger> _serilogger;

    [SetUp]
    public void SetUp()
    {
      _serilogger = new Mock<Serilog.ILogger>();
      _serilogger.Setup(l => l.ForContext("MethodName", It.IsAny<string>(), It.IsAny<bool>())).Returns(_serilogger.Object);
      _sub = new Logger(_serilogger.Object);
    }

    [Test, TestCaseSource(typeof(TestCases), nameof(TestCases.LogLevelTestCases))]
    public void TestWithNoException(LogLevelTestCase testCase)
    {
      const string msgTemplate = "Unit test";
      var c = testCase.Method.Compile();
      var f = c.Invoke(_sub);
      f.Invoke(msgTemplate, null);
      _serilogger.Verify(l => l.Write(testCase.LogEventLevel, msgTemplate, null), Times.Once);
      AssertForContextWithMethodNameIsCalled(System.Reflection.MethodInfo.GetCurrentMethod().Name);
    }

    [Test, TestCaseSource(typeof(TestCases), nameof(TestCases.LogLevelTestCases))]
    public void TestWithNoExceptionAndParams(LogLevelTestCase testCase)
    {
      const string msgTemplate = "Unit test";
      var c = testCase.Method.Compile();
      var f = c.Invoke(_sub);
      var p = new object[] {"p1", 2, 3.0d};
      f.Invoke(msgTemplate, p);
      _serilogger.Verify(l => l.Write(testCase.LogEventLevel, msgTemplate, p), Times.Once);
      AssertForContextWithMethodNameIsCalled(System.Reflection.MethodInfo.GetCurrentMethod().Name);
    }

    [Test, TestCaseSource(typeof(TestCases), nameof(TestCases.LogLevelTestCasesWithException))]
    public void TestWithException(LogLevelTestCaseWithException testCase)
    {
      const string msgTemplate = "Unit test";
      var c = testCase.Method.Compile();
      var f = c.Invoke(_sub);
      var ex = new NotImplementedException();
      f.Invoke(ex,  msgTemplate, null);
      _serilogger.Verify(l => l.Write(testCase.LogEventLevel, ex, msgTemplate, null), Times.Once);
      AssertForContextWithMethodNameIsCalled(System.Reflection.MethodInfo.GetCurrentMethod().Name);
    }

    [Test, TestCaseSource(typeof(TestCases), nameof(TestCases.LogLevelTestCasesWithException))]
    public void TestWithExceptionAndParams(LogLevelTestCaseWithException testCase)
    {
      const string msgTemplate = "Unit test";
      var c = testCase.Method.Compile();
      var f = c.Invoke(_sub);
      var ex = new NotImplementedException();
      var p = new object[] { "p1", 2, 3.0d };
      f.Invoke(ex,  msgTemplate, p);
      _serilogger.Verify(l => l.Write(testCase.LogEventLevel, ex, msgTemplate, p), Times.Once);
      AssertForContextWithMethodNameIsCalled(System.Reflection.MethodInfo.GetCurrentMethod().Name);
    }

    [Test]
    public void GeneralError()
    {
      _sub.GeneralError();
      _serilogger.Verify(l => l.Write(LogEventLevel.Error, "Error occured in {MethodName}", It.IsAny<object[]>()), Times.Once);
      AssertForContextWithMethodNameIsCalled(System.Reflection.MethodInfo.GetCurrentMethod().Name);
    }

    [Test]
    public void GeneralErrorWithException()
    {
      var ex = new NotImplementedException();
      _sub.GeneralError(ex);
      _serilogger.Verify(l => l.Write(LogEventLevel.Error, ex, "Error occured in {MethodName}", It.IsAny<object[]>()), Times.Once);
      AssertForContextWithMethodNameIsCalled(System.Reflection.MethodInfo.GetCurrentMethod().Name);
    }

    [Test]
    public void Measured()
    {
      _serilogger.Setup(l => l.ForContext("elapsed", It.IsAny<long>(), It.IsAny<bool>())).Returns(_serilogger.Object);

      const string task = "Unit test";
      var stopper = System.Diagnostics.Stopwatch.StartNew();
      stopper.Stop();
      stopper.Reset();
      _sub.Measured(task, stopper);
      _serilogger.Verify(l => l.Write(LogEventLevel.Verbose, "Stopper {task} measured {elapsedHumanized}", task, "no time"), Times.Once);
      AssertForContextWithMethodNameIsCalled(System.Reflection.MethodInfo.GetCurrentMethod().Name);
      _serilogger.Verify(l => l.ForContext("elapsed", stopper.ElapsedMilliseconds, false), Times.Once);
    }

    [Test]
    public void GetCallerMemberNameSafelyFromTestClass()
    {
      var result = _sub.GetCallerMemberNameSafely(0); // must call with 0, to emulate the internal call stack of the logger
      Assert.AreEqual("GetCallerMemberNameSafelyFromTestClass", result);
    }

    [Test]
    public void GetCallerMemberNameSafelyFromTestClassWithWrapper()
    {
      var result = string.Empty;
      var test = new Action(() =>
      {
        result = _sub.GetCallerMemberNameSafely(1); // must call with 0, to emulate the internal call stack of the logger
      });
      test.Invoke();
      Assert.AreEqual("GetCallerMemberNameSafelyFromTestClassWithWrapper", result);
    }

    [Test]
    public void GetCallerMemberNameSafelyFromTestClassWithWrapper2()
    {
      var result = string.Empty;
      var test = new Action(() =>
      {
        result = _sub.GetCallerMemberNameSafely(0);
      });
      test.Invoke();
      Assert.IsTrue(result.StartsWith("<GetCallerMemberNameSafelyFromTestClassWithWrapper2>"));
    }

    void AssertForContextWithMethodNameIsCalled(string methodName)
    {
      _serilogger.Verify(l => l.ForContext("MethodName", methodName, false), Times.Once);
    }
  }

  static class TestCases
  {
    public static IEnumerable LogLevelTestCases
    {
      // https://github.com/nunit/docs/wiki/Generation-of-Test-Names-Spec
      get
      {
        yield return new TestCaseData(new LogLevelTestCase(l => l.Trace, LogEventLevel.Verbose)).SetName("{m} - Trace");
        yield return new TestCaseData(new LogLevelTestCase(l => l.Debug, LogEventLevel.Debug)).SetName("{m} - Debug");
        yield return new TestCaseData(new LogLevelTestCase(l => l.Info, LogEventLevel.Information)).SetName("{m} - Info");
        yield return new TestCaseData(new LogLevelTestCase(l => l.Warn, LogEventLevel.Warning)).SetName("{m} - Warn");
        yield return new TestCaseData(new LogLevelTestCase(l => l.Error, LogEventLevel.Error)).SetName("{m} - Error");
        yield return new TestCaseData(new LogLevelTestCase(l => l.Fatal, LogEventLevel.Fatal)).SetName("{m} - Fatal");
      }
    }

    public static IEnumerable LogLevelTestCasesWithException
    {
      get
      {
        yield return new TestCaseData(new LogLevelTestCaseWithException(l => l.Trace, LogEventLevel.Verbose)).SetName("{m} - Trace - Exception");
        yield return new TestCaseData(new LogLevelTestCaseWithException(l => l.Debug, LogEventLevel.Debug)).SetName("{m} - Debug - Exception");
        yield return new TestCaseData(new LogLevelTestCaseWithException(l => l.Info, LogEventLevel.Information)).SetName("{m} - Info - Exception");
        yield return new TestCaseData(new LogLevelTestCaseWithException(l => l.Warn, LogEventLevel.Warning)).SetName("{m} - Warn - Exception");
        yield return new TestCaseData(new LogLevelTestCaseWithException(l => l.Error, LogEventLevel.Error)).SetName("{m} - Error - Exception");
        yield return new TestCaseData(new LogLevelTestCaseWithException(l => l.Fatal, LogEventLevel.Fatal)).SetName("{m} - Fatal - Exception");
      }
    }
  }

  public abstract class LogLevelTestCaseBase
  {
    public LogEventLevel LogEventLevel { get; }

    public LogLevelTestCaseBase(LogEventLevel logEventLevel)
    {
      LogEventLevel = logEventLevel;
    }
  }

  public class LogLevelTestCase : LogLevelTestCaseBase
  {
    public Expression<Func<Organica.Common.Logger.SeriLog.Logger, Action<string, object[]>>> Method { get; }

    public LogLevelTestCase(Expression<Func<Organica.Common.Logger.SeriLog.Logger, Action<string, object[]>>> method, LogEventLevel logEventLevel) : base(logEventLevel)
    {
      Method = method;
    }
  }

  public class LogLevelTestCaseWithException : LogLevelTestCaseBase
  {
    public Expression<Func<Organica.Common.Logger.SeriLog.Logger, Action<Exception, string, object[]>>> Method { get; }

    public LogLevelTestCaseWithException(Expression<Func<Organica.Common.Logger.SeriLog.Logger, Action<Exception, string, object[]>>> method, LogEventLevel logEventLevel) : base(logEventLevel)
    {
      Method = method;
    }
  }
}
