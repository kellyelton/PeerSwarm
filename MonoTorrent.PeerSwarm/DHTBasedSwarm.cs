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
		public DhtBasedSwarm(InfoHash hash, int port):base(hash,port)
		{
			_listener = new DhtListener(new IPEndPoint(IPAddress.Any, Port));
			_engine = new DhtEngine(_listener);
			
			_engine.PeersFound += EnginePeersFound;
			_engine.StateChanged += EngineStateChanged;
			_listener.MessageReceived += ListenerMessageReceived;
			//TODO Should somehow pull this piece out of this file, so it can be genericafied
			if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "DHTNodes.txt"))) return;
			Log("Node File Found.");
			_nodes = File.ReadAllBytes("DHTNodes.txt");
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
			File.WriteAllBytes(Path.Combine(Environment.CurrentDirectory, "DHTNodes.txt"), _engine.SaveNodes());
			_listener.Stop();
			_engine.Stop();
		}
	}

}
