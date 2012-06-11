using System;
using MonoTorrent;

namespace dhttest
{
	public interface IPeerSwarm
	{
		event EventHandler<PeersFoundEventArgs> PeersFound;
		event EventHandler<LogEventArgs> LogOutput;
		void Start();
		void Loop();
		void Stop();
	}
}
