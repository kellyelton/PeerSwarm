using System;

namespace MonoTorrent.PeerSwarm
{
	public abstract class PeerSwarm : IPeerSwarm,IDisposable
	{
		public event EventHandler<PeersFoundEventArgs> PeersFound;
		public event EventHandler<LogEventArgs> LogOutput;

		protected readonly InfoHash Hash;
		protected readonly int Port;

		protected PeerSwarm(InfoHash hash, int port) 
		{ 
			Hash = hash;
			Port = port;
		}

		internal void InvokePeersFound(PeerSwarm swarm, PeersFoundEventArgs e)
		{
			if(PeersFound != null) PeersFound.Invoke(swarm , e);
		}

		public abstract void Start();
		public abstract void Loop();
		public abstract void Stop();

		protected void Log(string format, params object[] objs)
		{

			if (LogOutput != null)
				LogOutput.Invoke(this, new LogEventArgs(String.Format(format, objs)));
		}

		public void Dispose()
		{
			PeersFound = null;
		}
	}
}
