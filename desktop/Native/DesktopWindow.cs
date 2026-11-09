using System.Runtime.InteropServices;
namespace UltimateArena.Native;
internal sealed class DesktopWindow:IDisposable {
    public int Width=1440,Height=900,Wheel;public int MouseX,MouseY,DeltaX,DeltaY;public bool Running=true,Active=true,Captured;public readonly List<char> Characters=[];
    public IntPtr Handle {get;private set;}private IntPtr dc,context;private readonly WndProc procedure;private bool fullscreen;private Rect oldRect;
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]private delegate IntPtr WndProc(IntPtr h,uint msg,IntPtr wp,IntPtr lp);
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]private struct WndClass {public uint Size,Style;public WndProc? Procedure;public int ExtraClass,ExtraWindow;public IntPtr Instance,Icon,Cursor,Background;public string? Menu,Name;public IntPtr SmallIcon;}
    [StructLayout(LayoutKind.Sequential)]private struct Message {public IntPtr Hwnd;public uint Id;public UIntPtr WParam;public IntPtr LParam;public uint Time;public Point Point;public uint Private;}
    [StructLayout(LayoutKind.Sequential)]private struct Point {public int X,Y;}
    [StructLayout(LayoutKind.Sequential)]private struct Rect {public int Left,Top,Right,Bottom;}
    [StructLayout(LayoutKind.Sequential)]private struct RawDevice {public ushort Page,Usage;public uint Flags;public IntPtr Target;}
    [StructLayout(LayoutKind.Sequential)]private struct PixelFormat {public ushort Size,Version;public uint Flags;public byte Type,ColorBits,RedBits,RedShift,GreenBits,GreenShift,BlueBits,BlueShift,AlphaBits,AlphaShift,AccumBits,AccumRedBits,AccumGreenBits,AccumBlueBits,AccumAlphaBits,DepthBits,StencilBits,AuxBuffers,LayerType,Reserved;public uint LayerMask,VisibleMask,DamageMask;}
    public DesktopWindow(){
        SetProcessDPIAware();procedure=WindowProcedure;var instance=GetModuleHandle(null);var cls=new WndClass{Size=(uint)Marshal.SizeOf<WndClass>(),Style=0x20,Procedure=procedure,Instance=instance,Icon=LoadIcon(instance,(IntPtr)32512),SmallIcon=LoadIcon(instance,(IntPtr)32512),Cursor=LoadCursor(IntPtr.Zero,(IntPtr)32512),Name="UltimateArenaNative"};if(RegisterClassEx(ref cls)==0)throw new InvalidOperationException("Could not register game window.");var rect=new Rect{Right=Width,Bottom=Height};AdjustWindowRect(ref rect,0xCF0000,false);Handle=CreateWindowEx(0,cls.Name!,"Ultimate Arena — The Foundry",0xCF0000,70,40,rect.Right-rect.Left,rect.Bottom-rect.Top,IntPtr.Zero,IntPtr.Zero,instance,IntPtr.Zero);if(Handle==IntPtr.Zero)throw new InvalidOperationException("Could not create game window.");dc=GetDC(Handle);var pfd=new PixelFormat{Size=(ushort)Marshal.SizeOf<PixelFormat>(),Version=1,Flags=0x25,Type=0,ColorBits=32,DepthBits=24,StencilBits=8};int pf=ChoosePixelFormat(dc,ref pfd);if(pf==0||!SetPixelFormat(dc,pf,ref pfd))throw new InvalidOperationException("OpenGL pixel format unavailable.");context=wglCreateContext(dc);if(context==IntPtr.Zero||!wglMakeCurrent(dc,context))throw new InvalidOperationException("OpenGL context unavailable.");var dev=new RawDevice{Page=1,Usage=2,Flags=0,Target=Handle};RegisterRawInputDevices([dev],1,(uint)Marshal.SizeOf<RawDevice>());SetVsync(true);ShowWindow(Handle,5);UpdateWindow(Handle);
    }
    public void SetVsync(bool enabled){var address=wglGetProcAddress("wglSwapIntervalEXT");if(address.ToInt64()>3)Marshal.GetDelegateForFunctionPointer<SwapInterval>(address)(enabled?1:0);}
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]private delegate int SwapInterval(int interval);
    private IntPtr WindowProcedure(IntPtr h,uint msg,IntPtr wp,IntPtr lp){
        switch(msg){case 0x10:Running=false;PostQuitMessage(0);return IntPtr.Zero;case 0x05:Width=Math.Max(1,(int)((long)lp&65535));Height=Math.Max(1,(int)(((long)lp>>16)&65535));break;case 0x1C:Active=wp!=IntPtr.Zero;if(!Active)Capture(false);break;case 0x20A:Wheel+=Math.Sign((short)(((long)wp>>16)&65535));break;case 0x102:if((long)wp>=32&&(long)wp<127)Characters.Add((char)wp);break;
        case 0xFF:if(Captured){uint size=0;uint header=(uint)(IntPtr.Size==8?24:16);GetRawInputData(lp,0x10000003,IntPtr.Zero,ref size,header);if(size>=header+20&&size<1024){var data=Marshal.AllocHGlobal((int)size);try{if(GetRawInputData(lp,0x10000003,data,ref size,header)==size&&Marshal.ReadInt32(data)==0&&(Marshal.ReadInt16(data,(int)header)&1)==0){DeltaX+=Marshal.ReadInt32(data,(int)header+12);DeltaY+=Marshal.ReadInt32(data,(int)header+16);}}finally{Marshal.FreeHGlobal(data);}}}return DefWindowProc(h,msg,wp,lp);}
        return DefWindowProc(h,msg,wp,lp);
    }
    public void Poll(){DeltaX=DeltaY=Wheel=0;Characters.Clear();while(PeekMessage(out var msg,IntPtr.Zero,0,0,1)){if(msg.Id==0x12){Running=false;break;}TranslateMessage(ref msg);DispatchMessage(ref msg);}GetCursorPos(out var p);ScreenToClient(Handle,ref p);MouseX=p.X;MouseY=p.Y;}
    public bool Key(int key)=>Active&&(GetAsyncKeyState(key)&0x8000)!=0;
    public void Capture(bool capture){capture&=Active;if(capture==Captured)return;Captured=capture;if(capture){ShowCursor(false);GetClientRect(Handle,out var r);var top=new Point{X=r.Left,Y=r.Top};var bottom=new Point{X=r.Right,Y=r.Bottom};ClientToScreen(Handle,ref top);ClientToScreen(Handle,ref bottom);var rect=new Rect{Left=top.X,Top=top.Y,Right=bottom.X,Bottom=bottom.Y};ClipCursor(ref rect);}else{ShowCursor(true);ClipCursor(IntPtr.Zero);}}
    public void ToggleFullscreen(){Capture(false);if(!fullscreen){GetWindowRect(Handle,out oldRect);SetWindowLongPtr(Handle,-16,(IntPtr)unchecked((int)0x90000000));SetWindowPos(Handle,IntPtr.Zero,0,0,GetSystemMetrics(0),GetSystemMetrics(1),0x20);}else{SetWindowLongPtr(Handle,-16,(IntPtr)0xCF0000);SetWindowPos(Handle,IntPtr.Zero,oldRect.Left,oldRect.Top,oldRect.Right-oldRect.Left,oldRect.Bottom-oldRect.Top,0x20);}fullscreen=!fullscreen;}
    public void Swap()=>SwapBuffers(dc);
    public void Dispose(){Capture(false);wglMakeCurrent(IntPtr.Zero,IntPtr.Zero);if(context!=IntPtr.Zero)wglDeleteContext(context);if(dc!=IntPtr.Zero)ReleaseDC(Handle,dc);if(Handle!=IntPtr.Zero)DestroyWindow(Handle);GC.KeepAlive(procedure);}
    [DllImport("kernel32",CharSet=CharSet.Unicode)]private static extern IntPtr GetModuleHandle(string? name);
    [DllImport("user32")]private static extern bool SetProcessDPIAware();
    [DllImport("user32",CharSet=CharSet.Unicode)]private static extern ushort RegisterClassEx(ref WndClass cls);
    [DllImport("user32",CharSet=CharSet.Unicode)]private static extern IntPtr CreateWindowEx(uint ex,string cls,string title,uint style,int x,int y,int w,int h,IntPtr parent,IntPtr menu,IntPtr instance,IntPtr parameter);
    [DllImport("user32",CharSet=CharSet.Unicode)]private static extern IntPtr DefWindowProc(IntPtr h,uint m,IntPtr w,IntPtr l);
    [DllImport("user32")]private static extern bool PeekMessage(out Message m,IntPtr h,uint min,uint max,uint remove);
    [DllImport("user32")]private static extern bool TranslateMessage(ref Message m);
    [DllImport("user32")]private static extern IntPtr DispatchMessage(ref Message m);
    [DllImport("user32")]private static extern void PostQuitMessage(int code);
    [DllImport("user32")]private static extern bool ShowWindow(IntPtr h,int cmd);
    [DllImport("user32")]private static extern bool UpdateWindow(IntPtr h);
    [DllImport("user32")]private static extern bool DestroyWindow(IntPtr h);
    [DllImport("user32")]private static extern IntPtr GetDC(IntPtr h);
    [DllImport("user32")]private static extern int ReleaseDC(IntPtr h,IntPtr dc);
    [DllImport("user32")]private static extern bool AdjustWindowRect(ref Rect r,uint style,bool menu);
    [DllImport("user32")]private static extern bool GetClientRect(IntPtr h,out Rect r);
    [DllImport("user32")]private static extern bool GetWindowRect(IntPtr h,out Rect r);
    [DllImport("user32")]private static extern bool GetCursorPos(out Point p);
    [DllImport("user32")]private static extern bool ScreenToClient(IntPtr h,ref Point p);
    [DllImport("user32")]private static extern bool ClientToScreen(IntPtr h,ref Point p);
    [DllImport("user32",EntryPoint="ClipCursor")]private static extern bool ClipCursor(ref Rect r);
    [DllImport("user32",EntryPoint="ClipCursor")]private static extern bool ClipCursor(IntPtr r);
    [DllImport("user32")]private static extern int ShowCursor(bool show);
    [DllImport("user32")]private static extern short GetAsyncKeyState(int key);
    [DllImport("user32")]private static extern bool RegisterRawInputDevices(RawDevice[] devices,uint count,uint size);
    [DllImport("user32")]private static extern uint GetRawInputData(IntPtr raw,uint cmd,IntPtr data,ref uint size,uint header);
    [DllImport("user32",EntryPoint="SetWindowLongPtrW")]private static extern IntPtr SetWindowLongPtr(IntPtr h,int index,IntPtr value);
    [DllImport("user32")]private static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int w,int height,uint flags);
    [DllImport("user32")]private static extern int GetSystemMetrics(int index);
    [DllImport("user32",CharSet=CharSet.Unicode)]private static extern IntPtr LoadIcon(IntPtr instance,IntPtr id);
    [DllImport("user32",CharSet=CharSet.Unicode)]private static extern IntPtr LoadCursor(IntPtr instance,IntPtr id);
    [DllImport("gdi32")]private static extern int ChoosePixelFormat(IntPtr dc,ref PixelFormat pfd);
    [DllImport("gdi32")]private static extern bool SetPixelFormat(IntPtr dc,int pf,ref PixelFormat pfd);
    [DllImport("gdi32")]private static extern bool SwapBuffers(IntPtr dc);
    [DllImport("opengl32.dll")]private static extern IntPtr wglCreateContext(IntPtr dc);
    [DllImport("opengl32.dll")]private static extern bool wglMakeCurrent(IntPtr dc,IntPtr ctx);
    [DllImport("opengl32.dll")]private static extern bool wglDeleteContext(IntPtr ctx);
    [DllImport("opengl32.dll",CharSet=CharSet.Ansi)]private static extern IntPtr wglGetProcAddress(string name);
}
