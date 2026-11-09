using UltimateArena.Native;
namespace UltimateArena.Graphics;
internal sealed class Materials {
    public uint[] Textures=new uint[4];
    public Materials(){GL.glGenTextures(4,Textures);for(int k=0;k<4;k++){var pixels=Generate(k);GL.glBindTexture(GL.Texture2D,Textures[k]);if(GL.gluBuild2DMipmaps(GL.Texture2D,3,256,256,0x1907,0x1401,pixels)!=0)throw new InvalidOperationException("Texture generation failed.");GL.glTexParameteri(GL.Texture2D,0x2801,0x2703);GL.glTexParameteri(GL.Texture2D,0x2800,0x2601);GL.glTexParameteri(GL.Texture2D,0x2802,0x2901);GL.glTexParameteri(GL.Texture2D,0x2803,0x2901);}}
    public static byte[] Generate(int material){var bytes=new byte[256*256*3];float[,] lo={{28,45,57},{14,25,36},{80,91,100},{45,25,17}},hi={{112,137,146},{61,77,94},{169,180,176},{154,103,56}};
        static float Hash(int x,int y,int seed){uint h=unchecked((uint)(x*374761393+y*668265263+seed*1447));h=unchecked((h^(h>>13))*1274126177);return (h^(h>>16))/4294967296f;}
        static float Noise(float x,float y,int seed){int ix=(int)MathF.Floor(x),iy=(int)MathF.Floor(y);float u=x-ix,v=y-iy;u=u*u*(3-2*u);v=v*v*(3-2*v);return (Hash(ix,iy,seed)*(1-u)+Hash(ix+1,iy,seed)*u)*(1-v)+(Hash(ix,iy+1,seed)*(1-u)+Hash(ix+1,iy+1,seed)*u)*v;}
        for(int y=0;y<256;y++)for(int x=0;x<256;x++){float noise=0,weight=0,amp=1;for(int oct=0;oct<6;oct++){float freq=(1<<oct)*4/256f;noise+=Noise(x*freq,y*freq,1701+material)*amp;weight+=amp;amp*=.55f;}noise/=weight;int panel=material==1?64:128;float factor=x%panel<2||y%panel<2?.25f:1;if(material==1&&(x+y)%16<3)factor*=.8f;if((x%128==7||x%128==120)&&(y%128==7||y%128==120))factor=1.6f;for(int c=0;c<3;c++)bytes[(y*256+x)*3+c]=(byte)Math.Clamp((lo[material,c]+(hi[material,c]-lo[material,c])*noise)*factor,0,255);}return bytes;
    }
}
