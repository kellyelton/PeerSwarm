using System;
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
using System.Net;
using System.IO;

namespace MonoTorrent.PeerSwarm
{
	public class DhtBasedSwarm : PeerSwarm
	{
		private readonly DhtListener _listener;
		private readonly DhtEngine _engine;
		private readonly byte[] _nodes;
		private readonly string _nodeSavePath;
		public DhtBasedSwarm(InfoHash hash, int port, string nodeSavePath):base(hash,port)
		{
			_nodeSavePath = nodeSavePath;
			_listener = new DhtListener(new IPEndPoint(IPAddress.Any, Port));
			_engine = new DhtEngine(_listener);
			
			_engine.PeersFound += EnginePeersFound;
			_engine.StateChanged += EngineStateChanged;
			_listener.MessageReceived += ListenerMessageReceived;
			if (!String.IsNullOrWhiteSpace(_nodeSavePath) && File.Exists(_nodeSavePath))
			{
				Log("Node File Found.");
				_nodes = File.ReadAllBytes(_nodeSavePath);
			}
		}

		void ListenerMessageReceived(byte[] buffer, IPEndPoint endpoint)
		{

		}

		void EngineStateChanged(object sender, EventArgs e)
		{
			Log("State Changed");
			Announce();

		}

		void EnginePeersFound(object sender, PeersFoundEventArgs e)
		{
			Log("Peers Found: {0}" , e.Peers.Count);
			InvokePeersFound(this , e);
		}
		void GetPeers()
		{
			Log("Getting Peers");
			_engine.GetPeers(Hash);
		}
		void Announce()
		{
			Log("Announcing");
			_engine.Announce(Hash, 15000);
		}
		public override void Start()
		{
			Log("Started");
			_listener.Start();
			_engine.Start(_nodes);
			Announce();
		}
		public override void Loop()
		{
			GetPeers();
		}
		public override void Stop()
		{
			Log("Stopping");
			File.WriteAllBytes(_nodeSavePath, _engine.SaveNodes());
			_listener.Stop();
			_engine.Stop();
		}
	}

}
