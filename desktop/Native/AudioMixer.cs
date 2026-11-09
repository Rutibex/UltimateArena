using System.Runtime.InteropServices;
namespace UltimateArena.Native;
internal sealed class AudioMixer:IDisposable {
    [StructLayout(LayoutKind.Sequential,Pack=2)]private struct Format {public ushort Tag,Channels;public uint Rate,Average;public ushort Align,Bits,Extra;}
    [StructLayout(LayoutKind.Sequential)]private struct Header {public IntPtr Data;public uint Length,Recorded;public IntPtr User;public uint Flags,Loops;public IntPtr Next,Reserved;}
    private sealed class Voice {public double Phase,At,Duration,Frequency,Volume,Pan;public bool Noise;}
    private sealed class Block {public short[] Data=new short[1024];public GCHandle Pin;public IntPtr Header;}
    private readonly List<Voice> voices=[];private readonly List<Block> blocks=[];private readonly object gate=new();private readonly Thread? worker;private volatile bool running;private IntPtr device;private uint random=7101;
    public float Volume=.35f;public bool Available=>device!=IntPtr.Zero;
    public AudioMixer(){var format=new Format{Tag=1,Channels=2,Rate=44100,Average=176400,Align=4,Bits=16};if(waveOutOpen(out device,0xFFFFFFFF,ref format,IntPtr.Zero,IntPtr.Zero,0)!=0){device=IntPtr.Zero;return;}try{for(int i=0;i<4;i++){var b=new Block();b.Pin=GCHandle.Alloc(b.Data,GCHandleType.Pinned);b.Header=Marshal.AllocHGlobal(Marshal.SizeOf<Header>());var h=new Header{Data=b.Pin.AddrOfPinnedObject(),Length=(uint)(b.Data.Length*2)};Marshal.StructureToPtr(h,b.Header,false);blocks.Add(b);if(waveOutPrepareHeader(device,b.Header,(uint)Marshal.SizeOf<Header>())!=0)throw new InvalidOperationException("Audio buffer unavailable.");}running=true;worker=new Thread(Mix){IsBackground=true,Name="Arena audio mixer"};worker.Start();}catch{Dispose();}}
    public void Play(float frequency,float duration,float gain,bool noise=true,float pan=0){if(!Available||Volume<=0)return;lock(gate){if(voices.Count>=32)voices.RemoveAt(0);voices.Add(new(){Frequency=frequency,Duration=duration,Volume=gain,Noise=noise,Pan=Math.Clamp(pan,-1,1)});}}
    private void Mix(){while(running){foreach(var b in blocks){var h=Marshal.PtrToStructure<Header>(b.Header);if((h.Flags&0x10)!=0&&(h.Flags&1)==0)continue;lock(gate){for(int sample=0;sample<512;sample++){double l=0,r=0;foreach(var v in voices){if(v.At>=v.Duration)continue;double frac=v.At/v.Duration,envelope=Math.Exp(-frac*6)*(1-frac);v.Phase+=Math.Tau*v.Frequency*(1-frac*.8)/44100;random=unchecked(random*1664525+1013904223);double noise=random/2147483648.0-1;double s=(Math.Sin(v.Phase)*(v.Noise?.55:1)+(v.Noise?noise*.45:0))*envelope*v.Volume*Volume;l+=s*(1-v.Pan*.6);r+=s*(1+v.Pan*.6);v.At+=1/44100.0;}b.Data[sample*2]=(short)(Math.Clamp(l,-1,1)*30000);b.Data[sample*2+1]=(short)(Math.Clamp(r,-1,1)*30000);}voices.RemoveAll(v=>v.At>=v.Duration);}waveOutWrite(device,b.Header,(uint)Marshal.SizeOf<Header>());}Thread.Sleep(3);}}
    public void Dispose(){running=false;worker?.Join(1000);if(device!=IntPtr.Zero){waveOutReset(device);foreach(var b in blocks){waveOutUnprepareHeader(device,b.Header,(uint)Marshal.SizeOf<Header>());Marshal.FreeHGlobal(b.Header);if(b.Pin.IsAllocated)b.Pin.Free();}blocks.Clear();waveOutClose(device);device=IntPtr.Zero;}}
    [DllImport("winmm.dll")]private static extern uint waveOutOpen(out IntPtr handle,uint id,ref Format format,IntPtr callback,IntPtr instance,uint flags);
    [DllImport("winmm.dll")]private static extern uint waveOutPrepareHeader(IntPtr h,IntPtr header,uint size);
    [DllImport("winmm.dll")]private static extern uint waveOutUnprepareHeader(IntPtr h,IntPtr header,uint size);
    [DllImport("winmm.dll")]private static extern uint waveOutWrite(IntPtr h,IntPtr header,uint size);
    [DllImport("winmm.dll")]private static extern uint waveOutReset(IntPtr h);
    [DllImport("winmm.dll")]private static extern uint waveOutClose(IntPtr h);
}
