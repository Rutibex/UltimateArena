using System.Numerics;
namespace UltimateArena.Core;
public static class Movement {
    public static void Step(Fighter p,InputState input,float dt){
        if(p.Health<=0)return;
        float f=input.Forward,s=input.Side,len=MathF.Sqrt(f*f+s*s);if(len>1){f/=len;s/=len;}
        float speed=input.Crouch?4:9.4f;var wish=new Vector3(MathF.Sin(p.Yaw)*f+MathF.Cos(p.Yaw)*s,0,-MathF.Cos(p.Yaw)*f+MathF.Sin(p.Yaw)*s);
        if(p.Grounded){float friction=MathF.Max(0,1-9*dt);p.Velocity.X*=friction;p.Velocity.Z*=friction;}
        float add=MathF.Min((p.Grounded?75:23)*dt,MathF.Max(0,speed-Vector3.Dot(p.Velocity,wish)));p.Velocity+=wish*add;
        float h=MathEx.Horizontal(p.Velocity);if(h>15){p.Velocity.X*=15/h;p.Velocity.Z*=15/h;}
        p.DodgeCooldown=MathF.Max(0,p.DodgeCooldown-dt);
        if(input.Dodge&&p.Grounded&&p.DodgeCooldown==0){var d=wish.LengthSquared()>.01f?Vector3.Normalize(wish):MathEx.Direction(p.Yaw,0);p.Velocity=d*16;p.Velocity.Y=3.6f;p.Grounded=false;p.DodgeCooldown=.9f;}
        if(input.Jump&&p.Grounded){p.Velocity.Y=8;p.Grounded=false;}
        p.Velocity.Y-=22*dt;int steps=Math.Max(1,(int)MathF.Ceiling(p.Velocity.Length()*dt/.18f));float ds=dt/steps;p.Grounded=false;
        for(int i=0;i<steps;i++){
            for(int axis=0;axis<3;axis+=2){float old=p.Position[axis];p.Position[axis]+=p.Velocity[axis]*ds;foreach(var b in Arena.Solids)if(Arena.Overlaps(p.Position,b)){float top=b.Max.Y;if(top-p.Position.Y<=.42f&&top>=p.Position.Y-.01f&&p.Velocity.Y<=1){p.Position.Y=top;p.Grounded=true;}else{p.Position[axis]=old;p.Velocity[axis]=0;break;}}}
            float oldY=p.Position.Y;p.Position.Y+=p.Velocity.Y*ds;
            foreach(var b in Arena.Solids)if(Arena.Overlaps(p.Position,b)){if(p.Velocity.Y<=0&&oldY>=b.Max.Y-.02f){p.Position.Y=b.Max.Y;p.Velocity.Y=0;p.Grounded=true;}else if(p.Velocity.Y>0){p.Position.Y=b.Min.Y-1.65f;p.Velocity.Y=0;}}
        }
        if(p.Position.Y<.08f&&p.Velocity.Y<=0){p.Position.Y=0;p.Velocity.Y=0;p.Grounded=true;}
        foreach(var pad in Arena.Pads)if(MathEx.Horizontal(p.Position-pad)<1.25f&&p.Position.Y<.25f&&p.Velocity.Y<=0){p.Velocity.Y=13;p.Grounded=false;}
    }
}
