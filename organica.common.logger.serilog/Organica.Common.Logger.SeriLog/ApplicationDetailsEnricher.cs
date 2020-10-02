using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Serilog.Core;
using Serilog.Events;

namespace Organica.Common.Logger.SeriLog
{
  internal class ApplicationDetailsEnricher : ILogEventEnricher
  {
    private readonly string _assemblyName;
    private readonly string _assemblyVersion;
    private readonly string _machineName;
    private readonly string _ipAddress;

    public ApplicationDetailsEnricher(string applicationName, Version applicationVersion) : this(applicationName, applicationVersion.ToString()) { }

    public ApplicationDetailsEnricher(string applicationName, string applicationVersion)
    {
      if (applicationName != null)
      {
        _assemblyName = applicationName;
      }
      else
      {
        var applicationAssembly = Assembly.GetEntryAssembly();
        if (applicationAssembly != null)
        {
          _assemblyName = applicationAssembly.GetName().Name;
        }
      }
      if (applicationVersion != null)
      {
        _assemblyVersion = applicationVersion;
      }
      else
      {
        var applicationAssembly = Assembly.GetEntryAssembly();
        if (applicationAssembly != null)
        {
          _assemblyVersion = applicationAssembly.GetName().Version.ToString();
        }
      }

      _machineName = Dns.GetHostName();
      _ipAddress = Dns.GetHostEntry(_machineName).AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
      logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ApplicationName", _assemblyName));
      logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ApplicationVersion", _assemblyVersion));
      logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("MachineName", _machineName));
      logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IP address", _ipAddress));
    }
  }
}