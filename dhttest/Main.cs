using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using MonoTorrent;
using MonoTorrent.BEncoding;
using MonoTorrent.Client;
using MonoTorrent.Client.Encryption;
using MonoTorrent.Client.Tracker;
using MonoTorrent.Common;
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
using MonoTorrent.Tracker.Listeners;

namespace dhttest
{
	public class MainClass
	{
		private static InfoHash _hash;
		private const int Port = 15000;
		private static bool _quit;
		private static List<Peer> _peers;

		public static TrackerBasedSwarm TrackerSwarm;
		public static DhtBasedSwarm DhtSwarm;
		public static string BasePath = Environment.CurrentDirectory;
		public static void Main (string[] args)
		{
			var sha = new SHA1CryptoServiceProvider();
			_hash = new InfoHash(sha.ComputeHash(Encoding.ASCII.GetBytes("OCTGN")));
			_peers = new List<Peer>();
			

			Debug.Listeners.Add(new ConsoleTraceListener());
			Console.CancelKeyPress += delegate { Shutdown(); };
			AppDomain.CurrentDomain.ProcessExit += delegate { Shutdown(); };
			AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) { Debug.WriteLine(e.ExceptionObject); Shutdown(); };
			Thread.GetDomain().UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) { Debug.WriteLine(e.ExceptionObject); Shutdown(); };

			Setup();
			Loop();
		}
		static void Setup()
		{
			TrackerSwarm = new TrackerBasedSwarm(_hash);
			DhtSwarm = new DhtBasedSwarm(_hash);

			DhtSwarm.PeersFound += DhtSwarmPeersFound;
		}

		static void DhtSwarmPeersFound(object sender, PeersFoundEventArgs e)
		{
			lock (_peers)
			{
				foreach(var p in _peers)
				{
					if(!e.Peers.Contains(p))
					{
						_peers.Remove(p);
					}
				}
				foreach(var p in e.Peers)
				{
					if(!_peers.Contains(p))
					{
						_peers.Add(p);
					}
				}
				foreach(var p in _peers)
				{
					Console.WriteLine(p.ConnectionUri);
				}
			}
		}
		static void Loop()
		{
			DhtSwarm.Start();
			while(!_quit)
			{
				DhtSwarm.Loop();
				//_peers = TrackerSwarm.Loop();
				Thread.Sleep(30000);
			}
			
		}
		public static void Shutdown()
		{
			_quit = true;
			foreach (TraceListener lst in Debug.Listeners)
			{
				lst.Flush();
				lst.Close();
			}
			DhtSwarm.Stop();
			Thread.Sleep(1000);
		}
	}
}
