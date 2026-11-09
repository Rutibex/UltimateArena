using System.Numerics;
using System.Text;
using UltimateArena.Core;
namespace UltimateArena.Network;
internal enum Packet:byte { Hello=1,Welcome,Input,Actors,Projectiles,Events,Ping,Pong,Leave }
internal static class Wire {
    public const uint Magic=0x55415232;public const byte Version=1;public const int MaxBytes=1200;
    public static BinaryWriter Create(Packet type,ulong token=0){var w=new BinaryWriter(new MemoryStream(),Encoding.UTF8,true);w.Write(Magic);w.Write(Version);w.Write((byte)type);w.Write(token);return w;}
    public static byte[] Bytes(BinaryWriter w){w.Flush();var b=((MemoryStream)w.BaseStream).ToArray();if(b.Length>MaxBytes)throw new InvalidOperationException("Network packet exceeded MTU budget.");w.Dispose();return b;}
    public static BinaryReader? Open(byte[] b,out Packet type,out ulong token){type=0;token=0;if(b.Length<14||b.Length>MaxBytes)return null;var r=new BinaryReader(new MemoryStream(b));if(r.ReadUInt32()!=Magic||r.ReadByte()!=Version){r.Dispose();return null;}type=(Packet)r.ReadByte();token=r.ReadUInt64();return r;}
    public static void Vector(BinaryWriter w,Vector3 p){w.Write(p.X);w.Write(p.Y);w.Write(p.Z);}
    public static Vector3 Vector(BinaryReader r)=>new(r.ReadSingle(),r.ReadSingle(),r.ReadSingle());
    public static void Name(BinaryWriter w,string name){var bytes=Encoding.ASCII.GetBytes(new string(name.Where(c=>char.IsAsciiLetterOrDigit(c)||c is ' ' or '_' or '-').Take(16).ToArray()));w.Write((byte)bytes.Length);w.Write(bytes);}
    public static string Name(BinaryReader r){int len=r.ReadByte();if(len>16)throw new InvalidDataException("Name too long.");var bytes=r.ReadBytes(len);if(bytes.Length!=len)throw new EndOfStreamException();return Encoding.ASCII.GetString(bytes);}
    public static void Input(BinaryWriter w,InputState i){w.Write(i.Sequence);w.Write(i.Forward);w.Write(i.Side);w.Write(i.Yaw);w.Write(i.Pitch);w.Write((byte)i.Weapon);w.Write((byte)((i.Jump?1:0)|(i.Dodge?2:0)|(i.Crouch?4:0)|(i.Fire?8:0)|(i.Alt?16:0)));}
    public static InputState Input(BinaryReader r){var i=new InputState{Sequence=r.ReadUInt32(),Forward=r.ReadSingle(),Side=r.ReadSingle(),Yaw=r.ReadSingle(),Pitch=r.ReadSingle(),Weapon=r.ReadByte()};byte f=r.ReadByte();i.Jump=(f&1)!=0;i.Dodge=(f&2)!=0;i.Crouch=(f&4)!=0;i.Fire=(f&8)!=0;i.Alt=(f&16)!=0;return i;}
    public static void Fighter(BinaryWriter w,Fighter p){w.Write((ushort)p.Id);Name(w,p.Name);w.Write((byte)((p.Bot?1:0)|(p.Grounded?2:0)));Vector(w,p.Position);Vector(w,p.Velocity);w.Write(p.Yaw);w.Write(p.Pitch);w.Write(p.Health);w.Write(p.Armor);w.Write(p.Cooldown);w.Write(p.Respawn);w.Write(p.Invulnerable);w.Write(p.DodgeCooldown);w.Write((short)p.Score);w.Write((ushort)p.Deaths);w.Write((byte)p.Weapon);foreach(int a in p.Ammo)w.Write((ushort)a);w.Write(p.LastSequence);}
    public static Fighter Fighter(BinaryReader r){var p=new Fighter{Id=r.ReadUInt16(),Name=Name(r)};byte f=r.ReadByte();p.Bot=(f&1)!=0;p.Grounded=(f&2)!=0;p.Position=Vector(r);p.Velocity=Vector(r);p.Yaw=r.ReadSingle();p.Pitch=r.ReadSingle();p.Health=r.ReadSingle();p.Armor=r.ReadSingle();p.Cooldown=r.ReadSingle();p.Respawn=r.ReadSingle();p.Invulnerable=r.ReadSingle();p.DodgeCooldown=r.ReadSingle();p.Score=r.ReadInt16();p.Deaths=r.ReadUInt16();p.Weapon=r.ReadByte();for(int i=0;i<6;i++)p.Ammo[i]=r.ReadUInt16();p.LastSequence=r.ReadUInt32();return p;}
    public static void Event(BinaryWriter w,GameEvent e){w.Write(e.Id);w.Write((byte)e.Kind);w.Write((ushort)e.Owner);w.Write((ushort)e.Target);w.Write((byte)e.Weapon);Vector(w,e.A);Vector(w,e.B);w.Write(e.Value);}
    public static GameEvent Event(BinaryReader r)=>new(r.ReadUInt32(),(EventKind)r.ReadByte(),r.ReadUInt16(),r.ReadUInt16(),r.ReadByte(),Vector(r),Vector(r),r.ReadSingle());
}
