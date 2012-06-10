using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using MonoTorrent;
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
using MonoTorrent.Tracker.Listeners;

namespace dhttest
{
	public class MainClass
	{
		public static void Main (string[] args)
		{
			var listener = new DhtListener(new IPEndPoint(IPAddress.Any , 15000));
			var engine = new DhtEngine(listener);
			SHA1 sha = new SHA1CryptoServiceProvider();
			var hash = new InfoHash(sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes("OCTGN")));

			byte[] nodes = null;
			if (File.Exists("mynodes"))
				nodes = File.ReadAllBytes("mynodes");

			engine.PeersFound += (o, e) => Console.WriteLine("I FOUND PEERS: {0}", e.Peers.Count);
			listener.Start();
			engine.Announce(hash,15000);
			engine.Start(nodes);
			
			while(Console.ReadLine() != "q")
			{
				engine.GetPeers(hash);
				Thread.Sleep(100);
			}
			File.WriteAllBytes("mynodes", engine.SaveNodes());
		}
	}
}
