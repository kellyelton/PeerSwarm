using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MonoTorrent;
using MonoTorrent.BEncoding;
using MonoTorrent.Client;
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
using System.Net;
using System.IO;
using MonoTorrent.Common;

namespace dhttest
{
	public class DhtBasedSwarm
	{
		public event EventHandler<PeersFoundEventArgs> PeersFound;
		private readonly DhtListener _listener;
		private readonly DhtEngine _engine;
		private readonly byte[] _nodes;
		private readonly InfoHash _hash;
		private int _aCounter;
		public DhtBasedSwarm(InfoHash hash)
		{
			_aCounter = 0;
			_hash = hash;
			_listener = new DhtListener(new IPEndPoint(IPAddress.Any, 15000));
			_engine = new DhtEngine(_listener);
			_engine.PeersFound += EnginePeersFound;
			_engine.StateChanged += EngineStateChanged;
			if (File.Exists(Path.Combine(MainClass.BasePath,"DHTNodes.txt")))
			{
				Log("Node File Found.");
				_nodes = File.ReadAllBytes("DHTNodes.txt");
			}
		}

		void EngineStateChanged(object sender, EventArgs e)
		{
			Log("State Changed");
		}

		void EnginePeersFound(object sender, PeersFoundEventArgs e)
		{
			Log("Peers Found: {0}" , e.Peers.Count);
			if (PeersFound != null) PeersFound.Invoke(this , e);
		}
		public void Start()
		{
			Log("Started");
			_listener.Start();
			_engine.Start(_nodes);
			Log("Announcing");
			_engine.Announce(_hash, 15000);
			var list = new BEncodedList();
		}
		public void Loop()
		{
			if(_aCounter == 2)
			{
				Log("Announcing");
				_engine.Announce(_hash, 15000);
				_aCounter = 0;
			}
			Log("Getting Peers");
			_engine.GetPeers(_hash);
			_aCounter++;
		}
		public void Stop()
		{
			Log("Stopping");
			File.WriteAllBytes(Path.Combine(MainClass.BasePath, "DHTNodes.txt"), _engine.SaveNodes());
			_listener.Stop();
			_engine.Stop();
		}
		void Log(string format, params object[] objs)
		{
			var s1 = String.Format(format , objs);
			Debug.WriteLine(String.Format("[DHTBasedSwarm]: {0}" , s1));
		}
	}

}
