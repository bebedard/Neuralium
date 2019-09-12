using System;
using System.Linq;
using Neuralium.Shell.Classes.Runtime;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Neuralium.Shell.Classes.General {
	public static class RpcEventLogSinkExtensions {
		public static LoggerConfiguration RpcEventLogSink(this LoggerSinkConfiguration loggerConfiguration, IFormatProvider formatProvider = null) {
			return loggerConfiguration.Sink(new RpcEventLogSink(new RpcEventFormatter()));
		}
	}

	/// <summary>
	///     special serilog sink to redirect log messages to the rpc message event
	/// </summary>
	public class RpcEventLogSink : ILogEventSink {
		private readonly IFormatProvider formatProvider;

		public RpcEventLogSink(IFormatProvider formatProvider) {
			this.formatProvider = formatProvider;
		}

		public void Emit(LogEvent logEvent) {

			Bootstrap.RpcProvider?.LogMessage(logEvent.RenderMessage(this.formatProvider), logEvent.Timestamp.DateTime, logEvent.Level.ToString(), logEvent.Properties.Select(p => (object) new {p.Key, Value = p.Value.ToString()}).ToArray());
		}
	}

	public class RpcEventFormatter : IFormatProvider, ICustomFormatter {

		public object GetFormat(Type formatType) {
			if(formatType == typeof(ICustomFormatter)) {
				return this;
			}

			return null;
		}

		public string Format(string format, object arg, IFormatProvider formatProvider) {
			return $"{DateTime.Now}- {arg}";
		}
	}
}