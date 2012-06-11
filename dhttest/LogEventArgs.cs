using System;

namespace dhttest
{
	public class LogEventArgs : EventArgs
	{
		public String Message { get; private set; }

		public bool DebugLog { get; private set; }

		public LogEventArgs(string mess, bool debugLog = true)
		{
			Message = mess;
			DebugLog = debugLog;
		}
	}
}
