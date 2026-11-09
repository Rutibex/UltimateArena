using System.Runtime.InteropServices;
using UltimateArena.Native;
using UltimateArena.Network;
namespace UltimateArena;
internal static class Program {
    public static int Main(string[] args){try{
        if(args.Any(a=>a is "--self-test" or "--server" or "--render-test"))PrepareConsole();
        if(args.Contains("--self-test"))return Tests.SelfTests.Run();
        if(args.Contains("--render-test")){int index=Array.IndexOf(args,"--render-test");Offscreen.Render(index+1<args.Length?args[index+1]:"renderer-check.ppm");return 0;}
        if(args.Contains("--server")){int Value(string key,int fallback){int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?int.Parse(args[i+1]):fallback;}using var host=new MatchHost(Value("--port",27960),Math.Clamp(Value("--bots",5),0,10));host.Start();Console.WriteLine($"ULTIMATE ARENA / UDP :{host.Port} / 60 HZ / CTRL+C TO STOP");using var stop=new ManualResetEventSlim();Console.CancelKeyPress+=(_,e)=>{e.Cancel=true;stop.Set();};while(!stop.Wait(250)){if(host.Error.Length>0)throw new InvalidOperationException(host.Error);}return 0;}
        if(!OperatingSystem.IsWindows()){Console.WriteLine("The native desktop client targets Windows x64. --server and --self-test run on other .NET 8 platforms.");return 1;}
        using var game=new DesktopGame();game.Run();return 0;
    }catch(Exception ex){string log=Path.Combine(Path.GetTempPath(),"UltimateArena-crash.txt");try{File.WriteAllText(log,ex.ToString());}catch(IOException){}if(OperatingSystem.IsWindows())MessageBox(IntPtr.Zero,"Ultimate Arena could not start.\n\n"+ex.Message+"\n\nDetails: "+log,"Ultimate Arena",0x10);else Console.Error.WriteLine(ex);return 1;}}
    private static void PrepareConsole(){if(!OperatingSystem.IsWindows())return;if(!AttachConsole(uint.MaxValue))AllocConsole();Console.SetOut(new StreamWriter(Console.OpenStandardOutput()){AutoFlush=true});Console.SetError(new StreamWriter(Console.OpenStandardError()){AutoFlush=true});}
    [DllImport("kernel32")]private static extern bool AttachConsole(uint process);
    [DllImport("kernel32")]private static extern bool AllocConsole();
    [DllImport("user32",CharSet=CharSet.Unicode)]private static extern int MessageBox(IntPtr h,string text,string title,uint type);
}
