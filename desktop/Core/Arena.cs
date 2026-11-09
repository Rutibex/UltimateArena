using System.Numerics;
namespace UltimateArena.Core;
public static class Arena {
    public const string Name="THE FOUNDRY";
    public static readonly List<Solid> Solids=[];
    public static readonly List<Solid> Detail=[];
    public static readonly Vector3[] Pads=[new(0,0,11),new(0,0,-11)];
    public static readonly Vector3[] Spawns=[new(-19,0,-8),new(19,0,8),new(-18,0,10),new(18,0,-10),new(-10,0,19),new(10,0,-19),new(-16,3,-18),new(16,3,18)];
    static Arena(){
        void B(float x,float y,float z,float sx,float sy,float sz,int m=0)=>Solids.Add(new(new(x,y,z),new(sx,sy,sz),m));
        void D(float x,float y,float z,float sx,float sy,float sz,Vector3 c,float glow=0)=>Detail.Add(new(new(x,y,z),new(sx,sy,sz),0,c,glow));
        B(0,-.5f,0,48,1,48,1);B(-24,5,0,1,11,49);B(24,5,0,1,11,49);B(0,5,-24,48,11,1);B(0,5,24,48,11,1);
        foreach(int s in new[]{-1,1}) {
            B(0,2.8f,s*18.5f,42,.4f,8,2);
            foreach(int x in new[]{-15,15})for(int i=0;i<8;i++)B(x,(i+1)*.375f/2,s*(6.5f+i),4,(i+1)*.375f,1);
            for(int x=-20;x<=20;x+=4){D(x,5.5f,s*23.35f,.18f,8,.2f,new(.45f,.55f,.58f));D(x,7.9f,s*23.2f,2.8f,.12f,.15f,s<0?new(.1f,.9f,1):new(1,.4f,.12f),1);}
            D(0,3.08f,s*14.52f,42,.12f,.12f,new(.15f,.8f,1),1);
            foreach(int x in new[]{-22,22}){D(x,9,s*13,3,.3f,14,new(.4f,.5f,.55f));D(x,8.8f,s*13,.12f,.1f,13,new(.1f,.5f,.6f),1);}
        }
        B(0,2.5f,0,5.5f,5,5.5f,3);
        foreach(int x in new[]{-9,9})foreach(int z in new[]{-8,8}){B(x,3.2f,z,1.8f,6.4f,1.8f);D(x,3,z+1,.12f,5,.08f,new(.15f,.9f,1),1);}
        foreach(int s in new[]{-1,1}){B(s*15,.8f,0,4,1.6f,3,3);B(s*6,.65f,s*11,3,1.3f,2);D(s*2.81f,2.7f,0,.08f,3,3.5f,new(.1f,1,.85f),1);D(0,2.7f,s*2.81f,3.5f,3,.08f,new(.1f,1,.85f),1);}
        foreach(float x in new[]{-23.35f,23.35f})for(int z=-20;z<=20;z+=5){D(x,4,z,.15f,6,3.6f,new(.5f,.6f,.6f));D(x*.99f,4,z,.12f,5,.1f,new(.2f,.75f,.9f),.5f);}
    }
    public static float? RayBox(Vector3 origin,Vector3 dir,Solid b,float max=100) {
        float lo=0,hi=max;var mn=b.Min;var mx=b.Max;
        for(int i=0;i<3;i++){float o=origin[i],d=dir[i];if(MathF.Abs(d)<1e-8){if(o<mn[i]||o>mx[i])return null;}else{float a=(mn[i]-o)/d,c=(mx[i]-o)/d;if(a>c)(a,c)=(c,a);lo=MathF.Max(lo,a);hi=MathF.Min(hi,c);if(lo>hi)return null;}}
        return lo;
    }
    public static float Trace(Vector3 origin,Vector3 dir,float max=100){foreach(var b in Solids){var t=RayBox(origin,dir,b,max);if(t.HasValue)max=t.Value;}return max;}
    public static bool Visible(Vector3 a,Vector3 b){a.Y+=1;b.Y+=1;var delta=b-a;float d=delta.Length();return d<.001f||Trace(a,delta/d,d)>=d-.05f;}
    public static bool Overlaps(Vector3 p,Solid b)=>MathF.Abs(p.X-b.Center.X)<b.Size.X/2+.36f&&MathF.Abs(p.Z-b.Center.Z)<b.Size.Z/2+.36f&&p.Y<b.Max.Y-.001f&&p.Y+1.65f>b.Min.Y+.001f;
    public static float Surface(float x,float z){float y=0;foreach(var b in Solids)if(MathF.Abs(x-b.Center.X)<b.Size.X/2+.38f&&MathF.Abs(z-b.Center.Z)<b.Size.Z/2+.38f)y=MathF.Max(y,b.Max.Y);return y;}
}
