using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Client.Tracker;
using MonoTorrent.Common;

namespace dhttest
{
	public class TrackerBasedSwarm
	{
		private readonly List<Peer> _peers;
		private readonly HTTPTracker _tracker;
		private readonly AnnounceParameters _aParams;
		private readonly InfoHash _hash;

		public TrackerBasedSwarm(InfoHash hash)
		{
			_hash = hash;
			//_tracker = new UdpTracker(new Uri("udp://tracker.openbittorrent.com:80/announce"));
			//_tracker = new UdpTracker(new Uri("udp://tracker.publicbt.com:80"));
			//_tracker = new UdpTracker(new Uri("udp://tracker.ccc.de:80"));
			//_tracker = new UdpTracker(new Uri("udp://tracker.istole.it:80"));
			_tracker = new HTTPTracker(new Uri("http://announce.torrentsmd.com:6969/announce"));
			_tracker.AnnounceComplete += TrackerAnnounceComplete;
			_aParams = new AnnounceParameters
			{
				InfoHash = _hash,
				BytesDownloaded = 0,
				BytesLeft = 0,
				BytesUploaded = 0,
				PeerId = "OCTGN",
				Ipaddress = IPAddress.Any.ToString(),
				Port = 15000,
				RequireEncryption = false,
				SupportsEncryption = true,
				ClientEvent = new TorrentEvent()
			};
			_peers = new List<Peer>();
		}
		void TrackerAnnounceComplete(object sender, AnnounceResponseEventArgs e)
		{
			Debug.WriteLine("Announce Done.");
			foreach (var p in e.Peers)
			{
				if (!_peers.Contains(p)) _peers.Add(p);
			}
		}
		void Announce()
		{
			Debug.WriteLine("Announcing...");
			var id = new TrackerConnectionID(_tracker, true, TorrentEvent.None, new ManualResetEvent(false));
			_tracker.Announce(_aParams, id);
		}
		public List<Peer> Loop()
		{
			Announce();
			return _peers;
		}
	}
}
