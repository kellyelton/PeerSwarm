using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MonoTorrent;
using MonoTorrent.BEncoding;
using MonoTorrent.Client;
using MonoTorrent.Client.Messages;
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
		private readonly Random _random;
		public DhtBasedSwarm(InfoHash hash)
		{
			_hash = hash;
			_listener = new DhtListener(new IPEndPoint(IPAddress.Any, 15000));
			_engine = new DhtEngine(_listener);
			
			_engine.PeersFound += EnginePeersFound;
			_engine.StateChanged += EngineStateChanged;
			_listener.MessageReceived += ListenerMessageReceived;
			_random = new Random();
			if (File.Exists(Path.Combine(MainClass.BasePath,"DHTNodes.txt")))
			{
				Log("Node File Found.");
				_nodes = File.ReadAllBytes("DHTNodes.txt");
			}
		}

		void ListenerMessageReceived(byte[] buffer, IPEndPoint endpoint)
		{
			try
			{
				var b = BEncodedValue.Decode(buffer);
				var d = b as BEncodedDictionary;
				if (d != null)
				{
					//if (d.ContainsKey("q"))
						//if (Equals(d["q"] , new BEncodedString("get_peers"))) 
							//Debugger.Break();
				}
			}
			catch
			{
				
			}
		}

		void EngineStateChanged(object sender, EventArgs e)
		{
			Log("State Changed");
			Announce();

		}

		void EnginePeersFound(object sender, PeersFoundEventArgs e)
		{
			Log("Peers Found: {0}" , e.Peers.Count);
			if (PeersFound != null) PeersFound.Invoke(this , e);
		}
		void GetPeers()
		{
			Log("Getting Peers");
			byte[] b = new byte[20];
			lock (_random) _random.NextBytes(b);
			_engine.GetPeers(_hash);
		}
		void Announce()
		{
			Log("Announcing");
			_engine.Announce(_hash, 15000);
		}
		public void Start()
		{
			Log("Started");
			_listener.Start();
			_engine.Start(_nodes);
			Announce();
		}
		public void Loop()
		{
			GetPeers();
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
