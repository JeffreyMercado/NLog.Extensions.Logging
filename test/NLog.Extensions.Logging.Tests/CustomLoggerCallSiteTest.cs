﻿using System;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class CustomLoggerCallSiteTest : NLogTestBase
    {
        [Fact]
        public void TestCallSiteSayHello()
        {
            SetupTestRunner<CustomLoggerCallSiteTestRunner>(typeof(SameAssemblyLogger<>));
            var target = new Targets.MemoryTarget { Layout = "${callsite}|${message}" };
            ConfigureNLog(target);
            var runner = GetRunner<CustomLoggerCallSiteTestRunner>();

            runner.SayHello();
            
            Assert.Single(target.Logs);
            Assert.Contains("SayHello", target.Logs[0]);
            Assert.Contains("stuff", target.Logs[0]);
        }

        public class SameAssemblyLogger<T> : ILogger<T>
        {
            private readonly Microsoft.Extensions.Logging.ILogger _logger;

            public SameAssemblyLogger(ILoggerFactory loggerFactory)
            {
                _logger = loggerFactory.CreateLogger<T>();
            }

            public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                string Formatter(TState innerState, Exception innerException)
                {
                    // additional logic for all providers goes here
                    var message = formatter(innerState, innerException) ?? string.Empty;
                    return message + " additional stuff in here";
                }

                _logger.Log(logLevel, eventId, state, exception, Formatter);
            }

            public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
            {
                return _logger.IsEnabled(logLevel);
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return _logger.BeginScope(state);
            }
        }

        public class CustomLoggerCallSiteTestRunner
        {
            private readonly ILogger<CustomLoggerCallSiteTestRunner> _logger;

            public CustomLoggerCallSiteTestRunner(ILogger<CustomLoggerCallSiteTestRunner> logger)
            {
                _logger = logger;
            }

            public void SayHello()
            {
                _logger.LogInformation("Hello");
            }
        }
    }
}
