using System.Collections.Generic;
using Datadog.Trace.Logging;
using Datadog.Trace.Logging.LogProviders;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Xunit;

namespace Datadog.Trace.Tests.Logging
{
    [Collection(nameof(Datadog.Trace.Tests.Logging))]
    [TestCaseOrderer("Datadog.Trace.TestHelpers.AlphabeticalOrderer", "Datadog.Trace.TestHelpers")]
    public class NLogLogProviderTests
    {
        private const string ExpectedStringFormat = "\"{0}\": \"{1}\"";

        private readonly ILogProvider _logProvider;
        private readonly ILog _logger;
        private readonly MemoryTarget _target;

        public NLogLogProviderTests()
        {
            var config = new LoggingConfiguration();
            var layout = new JsonLayout();
            layout.IncludeMdc = true;
            layout.Attributes.Add(new JsonAttribute("time", Layout.FromString("${longdate}")));
            layout.Attributes.Add(new JsonAttribute("level", Layout.FromString("${level:uppercase=true}")));
            layout.Attributes.Add(new JsonAttribute("message", Layout.FromString("${message}")));
            _target = new MemoryTarget
            {
                Layout = layout
            };

            config.AddTarget("memory", _target);
            config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, _target));
            LogManager.Configuration = config;
            SimpleConfigurator.ConfigureForTargetLogging(_target, NLog.LogLevel.Trace);

            _logProvider = new NLogLogProvider();
            LogProvider.SetCurrentLogProvider(_logProvider);
            _logger = new LoggerExecutionWrapper(_logProvider.GetLogger("test"));
        }

        [Fact]
        public void LogsInjectionEnabledAddsParentCorrelationIdentifiers()
        {
            // Assert that the NLog log provider is correctly being used
            Assert.IsType<NLogLogProvider>(LogProvider.CurrentLogProvider);

            // Instantiate a tracer for this test with default settings and set LogsInjectionEnabled to TRUE
            var tracer = LoggingProviderTestHelpers.InitializeTracer(enableLogsInjection: true);
            LoggingProviderTestHelpers.LogInParentSpan(tracer, _logger, _logProvider.OpenMappedContext, out var parentScope, out var childScope);

            // Filter the logs
            List<string> filteredLogs = new List<string>(_target.Logs);
            filteredLogs.RemoveAll(log => !log.Contains(LoggingProviderTestHelpers.LogPrefix));
            Assert.All(filteredLogs, e => e.Contains(parentScope));
        }

        [Fact]
        public void LogsInjectionEnabledAddsChildCorrelationIdentifiers()
        {
            // Assert that the NLog log provider is correctly being used
            Assert.IsType<NLogLogProvider>(LogProvider.CurrentLogProvider);

            // Instantiate a tracer for this test with default settings and set LogsInjectionEnabled to TRUE
            var tracer = LoggingProviderTestHelpers.InitializeTracer(enableLogsInjection: true);
            LoggingProviderTestHelpers.LogInChildSpan(tracer, _logger, _logProvider.OpenMappedContext, out var parentScope, out var childScope);

            // Filter the logs
            List<string> filteredLogs = new List<string>(_target.Logs);
            filteredLogs.RemoveAll(log => !log.Contains(LoggingProviderTestHelpers.LogPrefix));
            Assert.All(filteredLogs, e => e.Contains(childScope));
        }

        [Fact]
        public void LogsInjectionEnabledDoesNotAddCorrelationIdentifiersOutsideSpans()
        {
            // Assert that the NLog log provider is correctly being used
            Assert.IsType<NLogLogProvider>(LogProvider.CurrentLogProvider);

            // Instantiate a tracer for this test with default settings and set LogsInjectionEnabled to TRUE
            var tracer = LoggingProviderTestHelpers.InitializeTracer(enableLogsInjection: true);
            LoggingProviderTestHelpers.LogOutsideSpans(tracer, _logger, _logProvider.OpenMappedContext, out var parentScope, out var childScope);

            // Filter the logs
            List<string> filteredLogs = new List<string>(_target.Logs);
            filteredLogs.RemoveAll(log => !log.Contains(LoggingProviderTestHelpers.LogPrefix));
            Assert.All(filteredLogs, e => e.DoesNotContainCorrelationIdentifiers());
        }

        [Fact]
        public void DisabledLibLogSubscriberDoesNotAddCorrelationIdentifiers()
        {
            // Assert that the NLog log provider is correctly being used
            Assert.IsType<NLogLogProvider>(LogProvider.CurrentLogProvider);

            // Instantiate a tracer for this test with default settings and set LogsInjectionEnabled to TRUE
            var tracer = LoggingProviderTestHelpers.InitializeTracer(enableLogsInjection: false);
            LoggingProviderTestHelpers.LogEverywhere(tracer, _logger, _logProvider.OpenMappedContext, out var parentScope, out var childScope);

            // Filter the logs
            List<string> filteredLogs = new List<string>(_target.Logs);
            filteredLogs.RemoveAll(log => !log.Contains(LoggingProviderTestHelpers.LogPrefix));
            Assert.All(filteredLogs, e => e.DoesNotContainCorrelationIdentifiers());
        }
    }
}
