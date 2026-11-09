using System.Text.Json;
namespace UltimateArena;
internal sealed class Settings {
    public string Name {get;set;}="ROBINSON";public string Address {get;set;}="127.0.0.1:27960";public float Sensitivity {get;set;}=1;public float Volume {get;set;}=.35f;public bool Vsync {get;set;}=true;public int Bots {get;set;}=5;
    private static string Folder=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"UltimateArena");
    public static Settings Load(){try{var s=JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(Folder,"settings.json")))??new();s.Sensitivity=Math.Clamp(s.Sensitivity,.2f,3);s.Volume=Math.Clamp(s.Volume,0,1);s.Bots=Math.Clamp(s.Bots,0,10);s.Name=(s.Name??"PILOT")[..Math.Min(16,(s.Name??"PILOT").Length)];s.Address??="127.0.0.1:27960";return s;}catch(Exception ex)when(ex is IOException or JsonException or UnauthorizedAccessException){return new();}}
    public void Save(){try{Directory.CreateDirectory(Folder);File.WriteAllText(Path.Combine(Folder,"settings.json"),JsonSerializer.Serialize(this,new JsonSerializerOptions{WriteIndented=true}));}catch(Exception ex)when(ex is IOException or UnauthorizedAccessException){}}
}
