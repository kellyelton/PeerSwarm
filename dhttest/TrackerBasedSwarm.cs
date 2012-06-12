using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using MonoTorrent;
using MonoTorrent.Client.Tracker;
using MonoTorrent.Common;

namespace dhttest
{
	public class TrackerBasedSwarm : PeerSwarm
	{
		private readonly List<Tracker> _trackers;
		public AnnounceParameters AParams { get; set; }

		public TrackerBasedSwarm(InfoHash hash, int port, AnnounceParameters param):base(hash,port)
		{
			AParams = param;
			_trackers = new List<Tracker>();
		}
		public void AddTracker(string track)
		{
			Uri res;
			if(Uri.TryCreate(track , UriKind.Absolute , out res))
			{
				if(res.Scheme == "udp")
				{
					var t = new UdpTracker(res);
					t.AnnounceComplete += TrackerAnnounceComplete;
					_trackers.Add(t);
				}
				else if(res.Scheme == Uri.UriSchemeHttp)
				{
					var t = new HTTPTracker(res);
					t.AnnounceComplete += TrackerAnnounceComplete;
					_trackers.Add(t);
				}
			}
		}
		void TrackerAnnounceComplete(object sender, AnnounceResponseEventArgs e)
		{
			if(e.Peers.Count > 0)
			{
				Log("Announce Success: {0}", e.Tracker.Uri);
				InvokePeersFound(this,new PeersFoundEventArgs(Hash,e.Peers));
				return;
			}
			if(e.Successful ==false)
			{
				Log("Announce Failed: {0}" , e.Tracker.Uri);
			}
		}
		void Announce()
		{
			foreach(var t in _trackers)
			{
				var id = new TrackerConnectionID(t, true, TorrentEvent.None, new ManualResetEvent(false));
				Log("Announcing: {0}" , t.Uri);
				t.Announce(AParams , id);
			}
		}

		public override void Start()
		{
			
		}

		public override void Loop()
		{
			Announce();
		}

		public override void Stop()
		{
			
		}
	}
}
