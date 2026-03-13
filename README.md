# PeerSwarm

*Originally named "dhttest"*

## What It Is

PeerSwarm is a BitTorrent DHT and tracker-based peer discovery library and test application built on the MonoTorrent library. It finds peers on the BitTorrent network via two methods: DHT (Distributed Hash Table) and traditional tracker announcements. The project wraps these two approaches behind a common `IPeerSwarm` interface with a `PeerSwarmManager` that runs both in parallel and aggregates discovered peers.

The intended use case was repurposing BitTorrent's existing decentralized infrastructure as a free, serverless peer discovery mechanism for custom P2P applications.

## How It Works

The `PeerSwarmManager` orchestrates multiple `IPeerSwarm` implementations (`DhtBasedSwarm` and `TrackerBasedSwarm`). On start, it launches both on a background thread:

- **DHT Swarm** -- Starts a `DhtListener`, boots a `DhtEngine` with saved nodes, and periodically calls `GetPeers`/`Announce` to discover peers through the distributed hash table.
- **Tracker Swarm** -- Announces to configured UDP and HTTP trackers to find peers through traditional BitTorrent tracker infrastructure.

Both implementations raise `PeersFound` events, which the manager aggregates into a shared `ConcurrentBag` of peers.

The console test application (`PeerSwarmTester`) generates a deterministic info hash from its assembly name, configures trackers, and runs the manager in a loop.

## Tech Stack

- .NET 4.0
- C#
- MonoTorrent (included as source)
- Visual Studio 2010 / MonoDevelop
- NCrunch test runner

## Development Timeline

PeerSwarm was developed over approximately 3 days (June 9-12, 2012) across roughly 4 work sessions totaling about 11.5 hours of active coding. A final improvement was made about 5 weeks later on July 22, 2012.

The project evolved from a simple test harness into a cleanly abstracted library with interface-based design over the course of those sessions.

## AI Code Review

| Criterion | Grade | Notes |
|-----------|-------|-------|
| **Completeness** | B | Both DHT and tracker discovery work. Missing: no actual torrent downloading, no peer connection handling -- but that wasn't the goal. The MonoTorrent.PeerSwarm library is a clean, reusable abstraction. |
| **Functionality** | B | Should work as intended -- discovers peers via DHT and trackers. Empty ListenerMessageReceived handler and empty Start()/Stop() on TrackerBasedSwarm are minor gaps. |
| **Patterns & Practices** | B+ | Good use of interfaces and abstraction for 2012. Strategy pattern with IPeerSwarm. Proper event patterns. ConcurrentBag usage is appropriate for the threading model. |
| **Code Quality** | B | Clean, readable code. Good naming conventions. Some minor issues: AsParallel().ForAll() used where a simple foreach would do, and the Peers dedup check in PeerSwarmManager has a race condition (check-then-add on ConcurrentBag). |
| **Architecture** | A- | Well-structured for a test project. Clean separation: IPeerSwarm interface -> PeerSwarm abstract base -> DhtBasedSwarm/TrackerBasedSwarm implementations -> PeerSwarmManager orchestrator -> console app. |
| **Ambition** | B+ | DHT is a non-trivial protocol. Building a peer discovery abstraction on top of MonoTorrent shows solid understanding of the BitTorrent ecosystem. |
| **Time Investment** | A- | Built a well-architected peer discovery library with two implementations in ~11.5 hours across a weekend. Good velocity for the complexity. |

**Overall Grade: B+**

This is a well-structured experiment in BitTorrent peer discovery. The developer clearly understood both the protocol and good OOP design patterns. The architecture -- abstracting DHT and tracker-based discovery behind a common interface with a manager pattern -- is more thoughtful than most test projects. Built quickly over a weekend.

*Code review performed by AI (Claude) and graded relative to the era the code was written in.*
