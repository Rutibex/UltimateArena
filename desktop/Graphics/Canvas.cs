using System.Numerics;
using UltimateArena.Native;
namespace UltimateArena.Graphics;
internal static class Canvas {
    public static readonly Vector3 Cyan=new(.5f,.9f,.86f),Amber=new(.95f,.73f,.43f),White=new(.85f,.91f,.9f),Muted=new(.48f,.61f,.65f),Dark=new(.025f,.06f,.085f);
    private static readonly Dictionary<char,byte[]> Font=new(){
        ['A']=[14,17,17,31,17,17,17],['B']=[30,17,17,30,17,17,30],['C']=[14,17,16,16,16,17,14],['D']=[30,17,17,17,17,17,30],['E']=[31,16,16,30,16,16,31],['F']=[31,16,16,30,16,16,16],['G']=[14,17,16,23,17,17,15],['H']=[17,17,17,31,17,17,17],['I']=[14,4,4,4,4,4,14],['J']=[7,2,2,2,18,18,12],['K']=[17,18,20,24,20,18,17],['L']=[16,16,16,16,16,16,31],['M']=[17,27,21,21,17,17,17],['N']=[17,25,25,21,19,19,17],['O']=[14,17,17,17,17,17,14],['P']=[30,17,17,30,16,16,16],['Q']=[14,17,17,17,21,18,13],['R']=[30,17,17,30,20,18,17],['S']=[15,16,16,14,1,1,30],['T']=[31,4,4,4,4,4,4],['U']=[17,17,17,17,17,17,14],['V']=[17,17,17,17,17,10,4],['W']=[17,17,17,21,21,21,10],['X']=[17,17,10,4,10,17,17],['Y']=[17,17,10,4,4,4,4],['Z']=[31,1,2,4,8,16,31],
        ['0']=[14,17,19,21,25,17,14],['1']=[4,12,4,4,4,4,14],['2']=[14,17,1,2,4,8,31],['3']=[30,1,1,14,1,1,30],['4']=[2,6,10,18,31,2,2],['5']=[31,16,16,30,1,1,30],['6']=[14,16,16,30,17,17,14],['7']=[31,1,2,4,8,8,8],['8']=[14,17,17,14,17,17,14],['9']=[14,17,17,15,1,1,14],
        ['.']=[0,0,0,0,0,12,12],[':']=[0,12,12,0,12,12,0],['-']=[0,0,0,31,0,0,0],['/']=[1,2,2,4,8,8,16],['+']=[0,4,4,31,4,4,0],['[']=[14,8,8,8,8,8,14],[']']=[14,2,2,2,2,2,14],['(']=[2,4,8,8,8,4,2],[')']=[8,4,2,2,2,4,8],['?']=[14,17,1,2,4,0,4],['_']=[0,0,0,0,0,0,31],['=']=[0,0,31,0,31,0,0],['!']=[4,4,4,4,4,0,4],['>']=[16,8,4,2,4,8,16],['<']=[1,2,4,8,4,2,1],['%']=[17,2,4,8,16,0,17],['#']=[10,31,10,10,31,10,0]
    };
    public static void Begin(){GL.glDisable(GL.DepthTest);GL.glDisable(GL.Lighting);GL.glDisable(GL.Fog);GL.glDisable(GL.Texture2D);GL.glEnable(GL.Blend);GL.glMatrixMode(GL.Projection);GL.glLoadIdentity();GL.glOrtho(0,1600,900,0,-1,1);GL.glMatrixMode(GL.ModelView);GL.glLoadIdentity();}
    public static void End()=>GL.glDisable(GL.Blend);
    public static void Rect(float x,float y,float w,float h,Vector3 color,float alpha=1){GL.glColor4f(color.X,color.Y,color.Z,alpha);GL.glBegin(GL.Quads);Quad(x,y,w,h);GL.glEnd();}
    private static void Quad(float x,float y,float w,float h){GL.glVertex3f(x,y,0);GL.glVertex3f(x+w,y,0);GL.glVertex3f(x+w,y+h,0);GL.glVertex3f(x,y+h,0);}
    public static void Border(float x,float y,float w,float h,Vector3 c,float alpha=1){Rect(x,y,w,1,c,alpha);Rect(x,y+h,w,1,c,alpha);Rect(x,y,1,h,c,alpha);Rect(x+w,y,1,h,c,alpha);}
    public static float Width(string text,float scale)=>text.Length*6*scale;
    public static void Text(string text,float x,float y,float scale,Vector3? color=null,float alpha=1){var col=color??White;GL.glColor4f(col.X,col.Y,col.Z,alpha);GL.glBegin(GL.Quads);foreach(char original in text){char c=char.ToUpperInvariant(original);if(Font.TryGetValue(c,out var rows))for(int yy=0;yy<7;yy++)for(int xx=0;xx<5;xx++)if((rows[yy]&(1<<(4-xx)))!=0)Quad(x+xx*scale,y+yy*scale,scale,scale);x+=6*scale;}GL.glEnd();}
    public static void Center(string text,float y,float scale,Vector3? color=null)=>Text(text,800-Width(text,scale)/2,y,scale,color);
}
