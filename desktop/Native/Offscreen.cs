using System.Numerics;
using System.Runtime.InteropServices;
using UltimateArena.Core;
using UltimateArena.Graphics;
namespace UltimateArena.Native;
internal static class Offscreen {
    public static void Render(string file){
        if(!OperatingSystem.IsLinux())throw new PlatformNotSupportedException("Offscreen QA uses Linux Mesa EGL. Play normally on Windows.");
        Environment.SetEnvironmentVariable("EGL_PLATFORM","surfaceless");NativeLibrary.SetDllImportResolver(typeof(GL).Assembly,(name,assembly,search)=>name=="opengl32.dll"?NativeLibrary.Load("libGL.so.1"):name=="glu32.dll"?NativeLibrary.Load("libGLU.so.1"):IntPtr.Zero);
        var display=eglGetPlatformDisplay(0x31DD,IntPtr.Zero,IntPtr.Zero);if(!eglInitialize(display,out int major,out int minor))throw new InvalidOperationException("EGL initialization failed.");if(!eglBindAPI(0x30A2))throw new InvalidOperationException("OpenGL API unavailable.");int[] attributes=[0x3024,8,0x3023,8,0x3022,8,0x3025,24,0x3033,1,0x3040,8,0x3038];if(!eglChooseConfig(display,attributes,out var config,1,out int count)||count==0)throw new InvalidOperationException("EGL configuration unavailable.");int[] surfaceAttributes=[0x3057,1440,0x3056,900,0x3038];var surface=eglCreatePbufferSurface(display,config,surfaceAttributes);var context=eglCreateContext(display,config,IntPtr.Zero,[0x3038]);if(!eglMakeCurrent(display,surface,surface,context))throw new InvalidOperationException("Offscreen context unavailable.");
        try{var renderer=new Renderer();var world=new World();for(int i=0;i<100;i++)world.Step();renderer.Draw(world,new(18,1.45f,8),-.95f,.05f,0,1440,900,1,true,4,0,false);Canvas.Begin();Canvas.Rect(0,0,1600,85,Canvas.Dark,.8f);Canvas.Text("ULTIMATE ARENA / THE FOUNDRY",35,28,3,Canvas.Cyan);Canvas.Text("NATIVE C# / OPENGL",1140,35,1.6f,Canvas.Amber);Canvas.Rect(0,800,1600,100,Canvas.Dark,.92f);Canvas.Text("100",30,825,6,Canvas.Cyan);Canvas.Text("35",190,825,6,Canvas.Amber);Canvas.Text("COMET / GUIDED ROCKET",1050,850,2,Canvas.Amber);Canvas.Rect(798,435,2,8,Canvas.White);Canvas.Rect(798,457,2,8,Canvas.White);Canvas.Rect(785,449,8,2,Canvas.White);Canvas.Rect(807,449,8,2,Canvas.White);Canvas.End();uint error=GL.glGetError();if(error!=0)throw new InvalidOperationException($"OpenGL error: {error:X}");var bytes=new byte[1440*900*3];GL.glReadPixels(0,0,1440,900,0x1907,0x1401,bytes);using var output=File.Create(file);var header=System.Text.Encoding.ASCII.GetBytes("P6\n1440 900\n255\n");output.Write(header);for(int row=899;row>=0;row--)output.Write(bytes,row*1440*3,1440*3);Console.WriteLine($"PASS native renderer: OpenGL {Marshal.PtrToStringAnsi(GL.glGetString(0x1F02))}; EGL {major}.{minor}; no GL errors; frame {file}");}
        finally{eglMakeCurrent(display,IntPtr.Zero,IntPtr.Zero,IntPtr.Zero);eglDestroyContext(display,context);eglDestroySurface(display,surface);eglTerminate(display);}
    }
    [DllImport("libEGL.so.1")]private static extern IntPtr eglGetPlatformDisplay(uint platform,IntPtr display,IntPtr attributes);
    [DllImport("libEGL.so.1")]private static extern bool eglInitialize(IntPtr display,out int major,out int minor);
    [DllImport("libEGL.so.1")]private static extern bool eglBindAPI(uint api);
    [DllImport("libEGL.so.1")]private static extern bool eglChooseConfig(IntPtr display,int[] attributes,out IntPtr config,int count,out int matched);
    [DllImport("libEGL.so.1")]private static extern IntPtr eglCreatePbufferSurface(IntPtr display,IntPtr config,int[] attributes);
    [DllImport("libEGL.so.1")]private static extern IntPtr eglCreateContext(IntPtr display,IntPtr config,IntPtr shared,int[] attributes);
    [DllImport("libEGL.so.1")]private static extern bool eglMakeCurrent(IntPtr display,IntPtr draw,IntPtr read,IntPtr context);
    [DllImport("libEGL.so.1")]private static extern bool eglDestroyContext(IntPtr display,IntPtr context);
    [DllImport("libEGL.so.1")]private static extern bool eglDestroySurface(IntPtr display,IntPtr surface);
    [DllImport("libEGL.so.1")]private static extern bool eglTerminate(IntPtr display);
}
