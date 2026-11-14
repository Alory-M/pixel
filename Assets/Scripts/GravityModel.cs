using System;
using System.Collections.Generic;
namespace PixelGravity {
 public enum Direction { Down, Left, Right }
 public struct Cell : IEquatable<Cell> {
  public int x,y; public Cell(int x,int y){this.x=x;this.y=y;}
  public bool Equals(Cell b){return x==b.x&&y==b.y;} public override bool Equals(object o){return o is Cell && Equals((Cell)o);} public override int GetHashCode(){return x*397^y;}
 }
 public sealed class Mote { public Cell cell,previous; public int color; public Mote(Cell p,int c){cell=previous=p;color=c;} }
 public sealed class GravityModel {
  public const int Width=14, Height=19;
  public readonly Dictionary<Cell,int> targets=new Dictionary<Cell,int>();
  public readonly List<Mote> motes=new List<Mote>();
  public readonly List<Cell> cleared=new List<Cell>();
  public Direction gravity=Direction.Down;
  public bool won,lost; public int removed,wraps,turns,level; int ticks,spawnIndex;
  public int Total{get;private set;}
  static readonly Cell[] Neighbors={new Cell(1,0),new Cell(-1,0),new Cell(0,1),new Cell(0,-1)};
  public GravityModel(int level){this.level=level; Load(level);Total=targets.Count;}
  void Load(int n){
   // Coordinates grow downward. Empty top rows leave room for steering.
   string[] pattern=n==0?new[]{"....00....","...0000...","..000000..","...0000...","....00...."}:n==1?new[]{"...11...","..1111..","...00...","..0000..",".222222.","..2222.."}:new[]{"11......22","111....222",".11....22.","...0000...","..000000..","...0000..."};
   int offset=(Width-pattern[0].Length)/2;
   for(int y=0;y<pattern.Length;y++)for(int x=0;x<pattern[y].Length;x++)if(pattern[y][x]!='.')targets[new Cell(x+offset,y+10)]=pattern[y][x]-'0';
  }
  public void SetDirection(Direction d){if(won||lost)return;if(gravity!=d)turns++;gravity=d;}
  public Cell Advance(Cell c){if(gravity==Direction.Down)return new Cell(c.x,c.y+1);return new Cell((c.x+(gravity==Direction.Left?Width-1:1))%Width,c.y);}
  public List<Cell> Connected(Cell start,bool sameColor){
   var result=new List<Cell>();if(!targets.ContainsKey(start))return result;
   var seen=new HashSet<Cell>();var q=new Queue<Cell>();q.Enqueue(start);seen.Add(start);int color=targets[start];
   while(q.Count>0){Cell c=q.Dequeue();result.Add(c);foreach(var d in Neighbors){Cell p=new Cell(c.x+d.x,c.y+d.y);if(targets.ContainsKey(p)&&(!sameColor||targets[p]==color)&&seen.Add(p))q.Enqueue(p);}}
   return result;
  }
  public void Tick(){
   if(won||lost)return;cleared.Clear();ticks++;
   // Fixed cadence, deterministic colors chosen from the remaining artwork.
   if(ticks==1||ticks%11==0)Spawn();
   for(int i=motes.Count-1;i>=0;i--){Mote m=motes[i];m.previous=m.cell;Cell next=Advance(m.cell);
    if(next.y>=Height-1){lost=true;continue;}
    int color;if(targets.TryGetValue(next,out color)){
     if(color==m.color){var group=Connected(next,true);foreach(var c in group){targets.Remove(c);cleared.Add(c);}removed+=group.Count;motes.RemoveAt(i);Settle();}
     // Mismatches remain mobile and wait at the surface until redirected.
    }else if(!Occupied(next,m)){if(Math.Abs(next.x-m.cell.x)>1)wraps++;m.cell=next;}
   }
   won=targets.Count==0;if(won)lost=false;
  }
  public GravityModel Copy(){var b=new GravityModel(level);b.targets.Clear();foreach(var p in targets)b.targets[p.Key]=p.Value;b.motes.Clear();foreach(var m in motes)b.motes.Add(new Mote(m.cell,m.color));b.gravity=gravity;b.won=won;b.lost=lost;b.removed=removed;b.wraps=wraps;b.turns=turns;b.ticks=ticks;b.spawnIndex=spawnIndex;b.Total=Total;return b;}
  bool Occupied(Cell c,Mote except){foreach(var m in motes)if(m!=except&&m.cell.Equals(c))return true;return false;}
  void Spawn(){
   var colors=new List<int>();foreach(int c in targets.Values)if(!colors.Contains(c))colors.Add(c);colors.Sort();if(colors.Count==0)return;
   int color=colors[spawnIndex%colors.Count];int[] columns={6,7,4,9,2,11};int column=level==1&&spawnIndex==0?1:level==2&&spawnIndex==0?13:columns[spawnIndex%columns.Length];Cell p=new Cell(column,1);spawnIndex++;
   if(targets.ContainsKey(p)||Occupied(p,null)){lost=true;return;}motes.Add(new Mote(p,color));
  }
  public void Settle(){
   // Keep the largest connected body anchored; detached bodies move as rigid groups.
   var seen=new HashSet<Cell>();var groups=new List<List<Cell>>();List<Cell> anchor=null;
   foreach(var c in new List<Cell>(targets.Keys)){if(seen.Contains(c))continue;var g=Connected(c,false);foreach(var p in g)seen.Add(p);groups.Add(g);if(anchor==null||g.Count>anchor.Count)anchor=g;}
   foreach(var g in groups){if(g==anchor)continue;var own=new HashSet<Cell>(g);bool blocked=false;int steps=0;
    while(!blocked&&steps<Width+Height){var moved=new List<Cell>();foreach(var p in g){Cell q=Advance(p);
      if(targets.ContainsKey(q)&&!own.Contains(q)&&targets[q]==targets[p]){var chain=Connected(p,true);foreach(var hit in chain){targets.Remove(hit);cleared.Add(hit);}removed+=chain.Count;Settle();return;}
      if(q.y>=Height-1||(targets.ContainsKey(q)&&!own.Contains(q))||Occupied(q,null)){blocked=true;break;}moved.Add(q);}if(blocked)break;
     var colors=new List<int>();foreach(var p in g){colors.Add(targets[p]);targets.Remove(p);}for(int j=0;j<g.Count;j++)targets[moved[j]]=colors[j];g.Clear();g.AddRange(moved);own=new HashSet<Cell>(g);steps++;
    }
   }
  }
 }
}
