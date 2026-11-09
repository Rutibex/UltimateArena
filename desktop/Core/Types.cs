using System.Numerics;
namespace UltimateArena.Core;
public struct InputState {
    public float Forward, Side, Yaw, Pitch; public bool Jump,Dodge,Crouch,Fire,Alt; public int Weapon; public uint Sequence;
    public readonly bool Valid => float.IsFinite(Forward)&&float.IsFinite(Side)&&float.IsFinite(Yaw)&&float.IsFinite(Pitch)&&Weapon>=0&&Weapon<6;
}
public sealed class Fighter {
    public int Id; public string Name="PILOT"; public bool Bot; public Vector3 Position,Velocity; public float Yaw,Pitch,Health=100,Armor=35,Cooldown,DodgeCooldown,Respawn,Invulnerable; public int Score,Deaths,Weapon; public bool Grounded=true; public int[] Ammo=Weapons.All.Select(w=>w.Ammo).ToArray(); public InputState Input; public uint LastSequence; public List<Vector3> Path=[]; public float Think,Repath; public int LastWeapon=1;
    public Fighter Copy() => new(){Id=Id,Name=Name,Bot=Bot,Position=Position,Velocity=Velocity,Yaw=Yaw,Pitch=Pitch,Health=Health,Armor=Armor,Cooldown=Cooldown,DodgeCooldown=DodgeCooldown,Respawn=Respawn,Invulnerable=Invulnerable,Score=Score,Deaths=Deaths,Weapon=Weapon,Grounded=Grounded,Ammo=(int[])Ammo.Clone(),LastSequence=LastSequence};
}
public readonly record struct Solid(Vector3 Center,Vector3 Size,int Material=0,Vector3 Tint=default,float Glow=0) {
    public Vector3 Min=>Center-Size*.5f; public Vector3 Max=>Center+Size*.5f;
    public Vector3 Color=>Tint==default?Vector3.One:Tint;
}
public enum ShotKind:byte { Bullet,Rail,Rocket,Grenade }
public sealed record Weapon(string Name,string Role,string Alternate,ShotKind Kind,float Damage,float Delay,float Spread,int Pellets,int Ammo,Vector3 Color);
public static class Weapons {
    // Original weapon designs and values for this game. No decompiled code.
    public static readonly Weapon[] All=[
        new("KESTREL","PRECISION SIDEARM","FOCUSED SHOT",ShotKind.Bullet,28,.27f,.012f,1,90,new(.3f,.9f,.85f)),
        new("TEMPEST","ROTARY CARBINE","MAGAZINE DISCHARGE",ShotKind.Bullet,10,1/15f,.055f,1,240,new(1,.75f,.3f)),
        new("BREACHER","SCATTER CANNON","TIGHT DOUBLE VOLLEY",ShotKind.Bullet,10,.78f,.27f,9,36,new(1,.5f,.3f)),
        new("LONGBOW","PRECISION RAIL","HOLD TO ZOOM",ShotKind.Rail,82,1.05f,0,1,24,new(.65f,.6f,1)),
        new("COMET","ROCKET LAUNCHER","GUIDED ROCKET",ShotKind.Rocket,92,.85f,0,1,18,new(1,.3f,.15f)),
        new("LATCH","GRENADE PROJECTOR","STICKY PROXIMITY MINE",ShotKind.Grenade,85,.7f,0,1,24,new(.6f,1,.3f))
    ];
}
public sealed class Projectile {
    public int Id,Owner,Weapon; public Vector3 Position,Velocity; public float Life,Age,Damage; public bool Sticky,Homing,Stuck;
}
public enum EventKind:byte { Shot,Trace,Hit,Kill,Explosion,Pickup,Round }
public sealed record GameEvent(uint Id,EventKind Kind,int Owner,int Target,int Weapon,Vector3 A,Vector3 B,float Value);
public sealed class Pickup {
    public int Id,Kind,Weapon; public Vector3 Position; public float Ready;
}
public static class MathEx {
    public static Vector3 Direction(float yaw,float pitch)=>new(MathF.Sin(yaw)*MathF.Cos(pitch),MathF.Sin(pitch),-MathF.Cos(yaw)*MathF.Cos(pitch));
    public static float Angle(float a)=>MathF.IEEERemainder(a,MathF.Tau);
    public static float Horizontal(Vector3 v)=>MathF.Sqrt(v.X*v.X+v.Z*v.Z);
    public static Vector3 SafeNormal(Vector3 v)=>v.LengthSquared()<.000001f?Vector3.Zero:Vector3.Normalize(v);
}
