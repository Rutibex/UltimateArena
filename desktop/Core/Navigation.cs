using System.Numerics;
namespace UltimateArena.Core;
public static class Navigation {
    private sealed class Node {public Vector3 Position;public int Key;public List<Node> Links=[];}
    private static readonly Dictionary<int,Node> Nodes=[];
    static Navigation(){
        for(int ix=0;ix<45;ix++)for(int iz=0;iz<45;iz++){float x=ix-22,z=iz-22,y=Arena.Surface(x,z);if(y>3.01f)continue;Nodes[ix*45+iz]=new(){Position=new(x,y,z),Key=ix*45+iz};}
        foreach(var n in Nodes.Values)foreach(int off in new[]{-45,45,-1,1})if(Nodes.TryGetValue(n.Key+off,out var m)&&MathF.Abs(n.Position.X-m.Position.X)+MathF.Abs(n.Position.Z-m.Position.Z)<1.1f&&MathF.Abs(n.Position.Y-m.Position.Y)<=.42f)n.Links.Add(m);
    }
    private static Node Nearest(Vector3 p)=>Nodes.Values.MinBy(n=>Vector3.DistanceSquared(p,n.Position)+3*(p.Y-n.Position.Y)*(p.Y-n.Position.Y))!;
    public static List<Vector3> Find(Vector3 a,Vector3 b){var start=Nearest(a);var end=Nearest(b);var q=new Queue<Node>();q.Enqueue(start);var prev=new Dictionary<int,Node?>{{start.Key,null}};while(q.Count>0){var n=q.Dequeue();if(n==end)break;foreach(var m in n.Links)if(prev.TryAdd(m.Key,n))q.Enqueue(m);}if(!prev.ContainsKey(end.Key))return [];var path=new List<Vector3>();for(Node? n=end;n!=null&&n!=start;n=prev[n.Key])path.Add(n.Position);path.Reverse();return path;}
}
