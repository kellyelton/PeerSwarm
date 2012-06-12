using System;

namespace MonoTorrent.PeerSwarm
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
