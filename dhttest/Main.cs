using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Common;

namespace dhttest
{
	public class MainClass
	{
		private static InfoHash _hash;
		private const int Port = 15000;
		private static bool _quit;

		public static PeerSwarmManager Man;

		public static string BasePath = Environment.CurrentDirectory;

		public static void Main (string[] args)
		{
			var sha = new SHA1CryptoServiceProvider();
			_hash = new InfoHash(sha.ComputeHash(Encoding.ASCII.GetBytes("OCTGN")));

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
			var param = new MonoTorrent.Client.Tracker.AnnounceParameters
			{
				InfoHash = _hash,
				BytesDownloaded = 0,
				BytesLeft = 0,
				BytesUploaded = 0,
				PeerId = "OCTGN",
				Ipaddress = IPAddress.Any.ToString(),
				Port = Port,
				RequireEncryption = false,
				SupportsEncryption = true,
				ClientEvent = new TorrentEvent()
			};

			Man = new PeerSwarmManager(Port , param , _hash);
			Man.AddTracker("udp://tracker.openbittorrent.com:80/announce");
			Man.AddTracker("udp://tracker.publicbt.com:80/announce");
			Man.AddTracker("udp://tracker.ccc.de:80/announce");
			Man.AddTracker("udp://tracker.istole.it:80/announce");
			Man.AddTracker("http://announce.torrentsmd.com:6969/announce");
			Man.PeersFound += SwarmPeersFound;
			Man.LogOutput += LogOutput;
		}

		static void LogOutput(object sender, LogEventArgs e)
		{
#if(!DEBUG)
			if(!e.DebugLog)
#endif
				Console.WriteLine(e.Message);
		}

		static void SwarmPeersFound(object sender, PeersFoundEventArgs e)
		{
#if(!DEBUG)
			Console.Clear();
#endif
			Console.WriteLine(Resource1.MainClass_SwarmPeersFound_Line);
			foreach(var p in Man.Peers)
			{
				Console.WriteLine(p.ConnectionUri);
			}
			Console.WriteLine(Resource1.MainClass_SwarmPeersFound_Line);
		}
		static void Loop()
		{
			Man.Start();
			while(!_quit)
			{
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
			Man.Stop();
			Man.Dispose();
			Thread.Sleep(1000);
		}
	}
}