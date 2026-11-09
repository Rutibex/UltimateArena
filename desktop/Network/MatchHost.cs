using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using UltimateArena.Core;
namespace UltimateArena.Network;
internal sealed class MatchHost:IDisposable {
    private sealed class Peer {public required IPEndPoint Endpoint;public required Fighter Fighter;public ulong Token;public double Heard,InputAt,RateWindow;public int Count;}
    private readonly UdpClient socket;private readonly Dictionary<string,Peer> peers=[];private readonly CancellationTokenSource cancel=new();private Task? loop;private uint snapshot;public readonly World World;public readonly int Port;public int PeerCount=>peers.Count;public string Error="";
    public MatchHost(int port=27960,int bots=5,bool loopbackOnly=false){socket=new UdpClient(new IPEndPoint(loopbackOnly?IPAddress.Loopback:IPAddress.Any,port));socket.Client.Blocking=false;if(OperatingSystem.IsWindows()){try{socket.Client.IOControl(unchecked((int)0x9800000C),new byte[4],null);}catch(SocketException){}}Port=((IPEndPoint)socket.Client.LocalEndPoint!).Port;World=new(1701,bots);}
    public void Start(){loop=Task.Run(Run);}
    private void Run(){var clock=Stopwatch.StartNew();double last=clock.Elapsed.TotalSeconds,acc=0;int ticks=0;try{while(!cancel.IsCancellationRequested){double now=clock.Elapsed.TotalSeconds;Receive(now);acc+=Math.Min(.2,now-last);last=now;foreach(var p in peers.Values.ToArray()){if(now-p.Heard>8){World.Remove(p.Fighter.Id);peers.Remove(p.Endpoint.ToString());}else if(now-p.InputAt>.3)p.Fighter.Input=new(){Weapon=p.Fighter.Weapon,Yaw=p.Fighter.Yaw,Pitch=p.Fighter.Pitch};}while(acc>=World.Tick){World.Step();acc-=World.Tick;if(++ticks%3==0)Broadcast();}Thread.Sleep(1);}}catch(Exception ex)when(ex is not OutOfMemoryException){Error=ex.ToString();}}
    private void Send(Peer p,BinaryWriter w){var b=Wire.Bytes(w);try{socket.Send(b,b.Length,p.Endpoint);}catch(SocketException){}}
    private void Welcome(Peer p){var w=Wire.Create(Packet.Welcome,p.Token);w.Write(p.Fighter.Id);Send(p,w);}
    private void Receive(double now){for(int n=0;n<256&&DataWaiting();n++){IPEndPoint endpoint=new(IPAddress.Any,0);byte[] b;try{b=socket.Receive(ref endpoint);}catch(SocketException){break;}using var r=Wire.Open(b,out var type,out var token);if(r==null)continue;string key=endpoint.ToString();try{if(type==Packet.Hello){string name=Wire.Name(r);if(peers.TryGetValue(key,out var existing)){Welcome(existing);continue;}if(peers.Count>=8)continue;var p=new Peer{Endpoint=endpoint,Fighter=World.Add(name.Length>0?name:"PILOT"),Token=BitConverter.ToUInt64(RandomNumberGenerator.GetBytes(8)),Heard=now,InputAt=now};peers.Add(key,p);Welcome(p);continue;}
                if(!peers.TryGetValue(key,out var peer)||peer.Token!=token)continue;peer.Heard=now;if(now-peer.RateWindow>1){peer.RateWindow=now;peer.Count=0;}if(++peer.Count>180)continue;
                if(type==Packet.Input){var i=Wire.Input(r);if(!i.Valid||unchecked((int)(i.Sequence-peer.Fighter.LastSequence))<=0)continue;i.Forward=Math.Clamp(i.Forward,-1,1);i.Side=Math.Clamp(i.Side,-1,1);i.Yaw=MathEx.Angle(i.Yaw);i.Pitch=Math.Clamp(i.Pitch,-1.45f,1.45f);peer.Fighter.Input=i;peer.Fighter.LastSequence=i.Sequence;peer.InputAt=now;}
                else if(type==Packet.Ping){var w=Wire.Create(Packet.Pong,token);w.Write(r.ReadInt64());Send(peer,w);}
                else if(type==Packet.Leave){World.Remove(peer.Fighter.Id);peers.Remove(key);}
            }catch(Exception ex)when(ex is EndOfStreamException or InvalidDataException or ArgumentException){}}
    }
    private void Broadcast(){uint seq=++snapshot;int chunks=(World.Fighters.Count+7)/8;var events=World.Events.Where(e=>e.Kind!=EventKind.Trace).TakeLast(20).Concat(World.Events.Where(e=>e.Kind==EventKind.Trace).TakeLast(4)).OrderBy(e=>e.Id).ToArray();foreach(var peer in peers.Values){
        for(int chunk=0;chunk<chunks;chunk++){var w=Wire.Create(Packet.Actors,peer.Token);w.Write(seq);w.Write((byte)chunk);w.Write((byte)chunks);w.Write(World.Time);w.Write(World.RoundTime);w.Write(World.Restart);Wire.Name(w,World.Winner);foreach(var p in World.Pickups)w.Write(p.Ready);var actors=World.Fighters.Skip(chunk*8).Take(8).ToArray();w.Write((byte)actors.Length);foreach(var p in actors)Wire.Fighter(w,p);Send(peer,w);}
        int parts=Math.Max(1,(World.Projectiles.Count+15)/16);for(int part=0;part<parts;part++){var w=Wire.Create(Packet.Projectiles,peer.Token);w.Write(seq);w.Write((byte)part);w.Write((byte)parts);var shots=World.Projectiles.Skip(part*16).Take(16).ToArray();w.Write((byte)shots.Length);foreach(var p in shots){w.Write(p.Id);w.Write((ushort)p.Owner);w.Write((byte)p.Weapon);Wire.Vector(w,p.Position);Wire.Vector(w,p.Velocity);w.Write((byte)((p.Sticky?1:0)|(p.Homing?2:0)|(p.Stuck?4:0)));}Send(peer,w);}
        var ew=Wire.Create(Packet.Events,peer.Token);ew.Write((byte)events.Length);foreach(var e in events)Wire.Event(ew,e);Send(peer,ew);
    }}
    private bool DataWaiting(){try{return socket.Available>0;}catch(SocketException){return false;}}
    public void Dispose(){cancel.Cancel();loop?.Wait(2000);socket.Dispose();cancel.Dispose();}
}
