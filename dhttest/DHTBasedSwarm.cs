using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MonoTorrent;
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
		public DhtBasedSwarm(InfoHash hash)
		{
			_hash = hash;
			_listener = new DhtListener(new IPEndPoint(IPAddress.Any, 15000));
			_engine = new DhtEngine(_listener);
			_engine.PeersFound += new EventHandler<PeersFoundEventArgs>(EnginePeersFound);
			if (File.Exists(Path.Combine(MainClass.BasePath,"DHTNodes.txt")))
			{
				Log("No Node File Found.");
				_nodes = File.ReadAllBytes("DHTNodes.txt");
			}
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
			_engine.Start();
			Log("Announcing");
			_engine.Announce(_hash, 15000);
		}
		public void Loop()
		{
			Log("Getting Peers");
			_engine.GetPeers(_hash);
		}
		public void Stop()
		{
			Log("Stopping");
			_listener.Stop();
			_engine.Stop();
			File.WriteAllBytes(Path.Combine(MainClass.BasePath, "DHTNodes.txt"), _engine.SaveNodes());
		}
		void Log(string format, params object[] objs)
		{
			var s1 = String.Format(format , objs);
			Debug.WriteLine(String.Format("[DHTBasedSwarm]: {0}" , s1));
		}
	}

}
