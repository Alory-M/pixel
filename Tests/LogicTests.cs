using System;
using System.Linq;
using System.Collections.Generic;
using PixelGravity;
class LogicTests {
 static int checks;
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);checks++;Console.WriteLine("PASS "+message);}
 static string Key(GravityModel b){return ((int)b.gravity)+":"+string.Join(";",b.targets.OrderBy(p=>p.Key.y*14+p.Key.x).Select(p=>p.Key.x+","+p.Key.y+","+p.Value))+"/"+string.Join(";",b.motes.Select(m=>m.cell.x+","+m.cell.y+","+m.color));}
 class Node {public GravityModel b;public string path;public Node(GravityModel b,string p){this.b=b;path=p;}}
 static string Solve(int level){var beam=new List<Node>{new Node(new GravityModel(level),"")};for(int tick=0;tick<180;tick++){var next=new List<Node>();var seen=new HashSet<string>();foreach(var n in beam)foreach(Direction d in Enum.GetValues(typeof(Direction))){var b=n.b.Copy();b.SetDirection(d);b.Tick();string path=n.path+(int)d;if(b.won&&(level!=2||b.wraps>0))return path;if(!b.lost&&seen.Add(Key(b)))next.Add(new Node(b,path));}beam=next.OrderByDescending(n=>n.b.removed*100-n.b.motes.Count*2-n.b.turns*.02).Take(350).ToList();if(beam.Count==0)break;}return null;}
 static void Main(){
  var b=new GravityModel(0);b.gravity=Direction.Left;Check(b.Advance(new Cell(0,4)).Equals(new Cell(13,4)),"left wraps on same row");b.gravity=Direction.Right;Check(b.Advance(new Cell(13,4)).Equals(new Cell(0,4)),"right wraps on same row");
  b.targets.Clear();b.targets[new Cell(2,4)]=0;b.targets[new Cell(3,4)]=0;b.targets[new Cell(4,5)]=0;b.targets[new Cell(2,5)]=1;Check(b.Connected(new Cell(2,4),true).Count==2,"four-neighbor flood fill excludes diagonal and other color");
  b=new GravityModel(0);b.motes.Add(new Mote(new Cell(6,9),0));b.Tick();Check(b.won&&b.targets.Count==0,"same-color impact clears entire flower and wins");
  b=new GravityModel(0);int total=b.targets.Count;b.motes.Add(new Mote(new Cell(6,9),1));b.Tick();Check(b.targets.Count==total,"different-color impact preserves artwork");
  b=new GravityModel(0);b.motes.Add(new Mote(new Cell(0,17),0));b.Tick();Check(b.lost,"bottom danger line causes failure");
  b=new GravityModel(1);b.targets.Clear();b.targets[new Cell(3,12)]=0;b.targets[new Cell(3,13)]=0;b.targets[new Cell(8,9)]=1;b.Settle();Check(b.targets.ContainsKey(new Cell(8,17)),"detached component falls to last safe row");
  b=new GravityModel(0);b.targets.Clear();b.targets[new Cell(3,12)]=1;b.targets[new Cell(3,13)]=1;b.targets[new Cell(3,10)]=1;b.Settle();Check(b.targets.Count==0&&b.removed==3,"falling same-color components chain and clear");
  for(int l=0;l<3;l++){var path=Solve(l);Check(path!=null,"level "+(l+1)+" has a winning control sequence");System.IO.Directory.CreateDirectory("Tests/Results");System.IO.File.WriteAllText("Tests/Results/level"+(l+1)+".txt",path??"FAILED");Console.WriteLine("SOLUTION "+path);}
  Console.WriteLine(checks+" checks passed");
 }
}
