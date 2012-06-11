using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using MonoTorrent;
using MonoTorrent.Client;

namespace dhttest
{
	public class MainClass
	{
		private static InfoHash _hash;
		private const int Port = 15000;
		private static bool _quit;
		private static List<Peer> _peers;

		public static List<PeerSwarm> PeerSwarm;

		public static string BasePath = Environment.CurrentDirectory;

		public static void Main (string[] args)
		{
			var sha = new SHA1CryptoServiceProvider();
			_hash = new InfoHash(sha.ComputeHash(Encoding.ASCII.GetBytes("OCTGN")));
			_peers = new List<Peer>();
			PeerSwarm = new List<PeerSwarm>();
			

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
			var d = new DhtBasedSwarm(_hash , Port);
			d.PeersFound += SwarmPeersFound;
			d.LogOutput += LogOutput;
			PeerSwarm.Add(d);

			var t = new TrackerBasedSwarm(_hash , Port);
			t.AddTracker("udp://tracker.openbittorrent.com:80/announce");
			t.AddTracker("udp://tracker.publicbt.com:80/announce");
			t.AddTracker("udp://tracker.ccc.de:80/announce");
			t.AddTracker("udp://tracker.istole.it:80/announce");
			t.AddTracker("http://announce.torrentsmd.com:6969/announce");
			t.PeersFound += SwarmPeersFound;
			t.LogOutput += LogOutput;
			PeerSwarm.Add(t);
		}

		static void LogOutput(object sender, LogEventArgs e)
		{
#if(!DEBUG)
			if(!e.DebugLog)
#endif
				Console.WriteLine(Resource1.MainClass_LogOutput_Format, sender.GetType().Name, e.Message);
		}

		static void SwarmPeersFound(object sender, PeersFoundEventArgs e)
		{
			lock (_peers)
			{
				foreach(var p in e.Peers)
				{
					if(!_peers.Contains(p))
					{
						_peers.Add(p);
					}
				}
#if(!DEBUG)
				Console.Clear();
#endif
				Console.WriteLine(Resource1.MainClass_SwarmPeersFound_Line);
				foreach(var p in _peers)
				{
					Console.WriteLine(p.ConnectionUri);
				}
				Console.WriteLine(Resource1.MainClass_SwarmPeersFound_Line);
			}
		}
		static void Loop()
		{
			foreach(var p in PeerSwarm)
				p.Start();
			while(!_quit)
			{
				foreach(var p in PeerSwarm)
					p.Loop();
				Thread.Sleep(30000);
			}
			Finish();

		}
		public static void Shutdown() { _quit = true; }
		private static void Finish()
		{
			_quit = true;
			foreach (TraceListener lst in Debug.Listeners)
			{
				lst.Flush();
				lst.Close();
			}
			foreach (var p in PeerSwarm)
			{
				p.Stop();
				p.LogOutput -= LogOutput;
				p.PeersFound -= SwarmPeersFound;
			}
			Thread.Sleep(1000);
		}
	}
}
