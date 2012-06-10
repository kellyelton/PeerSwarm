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
		public static void Main (string[] args)
		{
			var sha = new SHA1CryptoServiceProvider();
			_hash = new InfoHash(sha.ComputeHash(Encoding.ASCII.GetBytes("OCTGN")));

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
		}
		static void Loop()
		{
			while(!_quit)
			{
				_peers = TrackerSwarm.Loop();
				Console.WriteLine(Resource1.MainClass_Loop_Peers);
				foreach (var p in _peers)
				{
					Console.WriteLine(p.ConnectionUri);
				}
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
			Thread.Sleep(2000);
		}
	}
}
