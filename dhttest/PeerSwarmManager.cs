using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Client.Tracker;

namespace dhttest
{
	public class PeerSwarmManager : IDisposable
	{
		public event EventHandler<LogEventArgs> LogOutput;
		public event EventHandler<PeersFoundEventArgs> PeersFound;

		public ConcurrentBag<Peer> Peers { get; private set; }

		private readonly InfoHash _hash;
		private readonly AnnounceParameters _aParams;
		private readonly int _port;
		private readonly ConcurrentBag<PeerSwarm> _peerSwarm;

		private Thread _thread;
		private bool _running;

		#region Constructor

		public PeerSwarmManager(int port, AnnounceParameters param, InfoHash hash)
		{
			Peers = new ConcurrentBag<Peer>();
			_peerSwarm = new ConcurrentBag<PeerSwarm>();
			_hash = hash;
			_aParams = param;
			_port = port;

			ConstructSwarm();
		}
		
		private void ConstructSwarm()
		{
			var d = new DhtBasedSwarm(_hash , _port);
			d.PeersFound += SwarmPeersFound;
			d.LogOutput += SwarmLogOutput;
			_peerSwarm.Add(d);

			var t = new TrackerBasedSwarm(_hash , _port , _aParams);
			t.PeersFound += SwarmPeersFound;
			t.LogOutput += SwarmLogOutput;
			_peerSwarm.Add(t);
		}

		#endregion

		#region API

		public void AddTracker(string url)
		{
			_peerSwarm.AsParallel().ForAll(delegate(PeerSwarm x)
			{
				var p = x as TrackerBasedSwarm;
				if(p != null)
					p.AddTracker(url);
			});
		}

		#endregion

		#region Controls

		public void Start()
		{
			if (_running) return;
			_thread = new Thread(Run);
			_thread.Start();
		}

		public void Stop()
		{
			_running = false;
		}

		#endregion

		#region Movement

		private void Run()
		{
			_running = true;
			_peerSwarm.AsParallel().ForAll(x => x.Start());
			while(_running)
			{
				_peerSwarm.AsParallel().ForAll(x => x.Loop());
				Thread.Sleep(30000);
			}
			_peerSwarm.AsParallel().ForAll(x => x.Stop());
		}

		#endregion

		#region PeerSwarmEvents

		void SwarmPeersFound(object sender, PeersFoundEventArgs e)
		{
			e.Peers.AsParallel().ForAll(delegate(Peer p)
			{
				if (!Peers.AsParallel().Contains(p)) Peers.Add(p);
			});
			OnPeersFound(e);
		}

		void SwarmLogOutput(object sender, LogEventArgs e)
		{
			e = new LogEventArgs(String.Format("[{0}] {1}" , sender.GetType().Name , e.Message) , e.DebugLog);
			OnLogOutput(e);
		}

		#endregion

		#region Invokers

		private void OnPeersFound(PeersFoundEventArgs e)
		{
			EventHandler<PeersFoundEventArgs> handler = PeersFound;
			if (handler != null) handler(this, e);
		}

		private void OnLogOutput(LogEventArgs e)
		{
			EventHandler<LogEventArgs> handler = LogOutput;
			if (handler != null) handler(this, e);
		}

		#endregion

		public void Dispose()
		{
			_peerSwarm.AsParallel().ForAll(x => x.LogOutput -= SwarmLogOutput);
			_peerSwarm.AsParallel().ForAll(x => x.PeersFound -= SwarmPeersFound);
			LogOutput = null;
			PeersFound = null;
		}
	}
}
