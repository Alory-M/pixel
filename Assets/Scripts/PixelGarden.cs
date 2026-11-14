using UnityEngine;
using PixelGravity;
using System.Collections.Generic;
public sealed class PixelGarden : MonoBehaviour {
 GravityModel model; int page=-1,unlocked; float clock,flash; Vector2 swipe; bool sound=true; AudioSource music,sfx;
 Texture2D paper,hero,round; Font font; readonly Color ink=new Color(.28f,.35f,.38f);
 readonly Color[] palette={new Color(1,.68f,.69f),new Color(.65f,.77f,.94f),new Color(.65f,.83f,.70f)};
 string[] names={"初见 · 一朵小花","午后 · 云间花园","远行 · 环绕的岛"};
 string[] tips={"让粉色星粒落向花朵，同色的一整片都会消散。","蓝天、花瓣与绿叶：观察下一颗星粒的颜色。","向左或向右穿过边缘，星粒会从另一边出现。"};
 Rect board=new Rect(41,234,338,459); float unit=338f/14; Vector2 origin; float scale;
 Dictionary<Cell,Vector2> targetFrom=new Dictionary<Cell,Vector2>(); float targetTween=1;

 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(FindObjectOfType<PixelGarden>()==null)new GameObject("Pixel Garden").AddComponent<PixelGarden>();}
 void Awake(){var cam=new GameObject("Garden Camera").AddComponent<Camera>();cam.gameObject.AddComponent<AudioListener>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.99f,.97f,.93f);Application.targetFrameRate=60;Screen.orientation=ScreenOrientation.Portrait;unlocked=PlayerPrefs.GetInt("garden.unlocked",0);font=Resources.Load<Font>("Art/Chinese");if(!font)font=Font.CreateDynamicFontFromOSFont(new[]{"Noto Sans CJK SC","Microsoft YaHei","Arial"},24);paper=Resources.Load<Texture2D>("Art/paper");hero=Resources.Load<Texture2D>("Art/garden");round=Resources.Load<Texture2D>("Art/rounded");music=gameObject.AddComponent<AudioSource>();music.clip=Resources.Load<AudioClip>("Audio/afternoon");music.loop=true;music.volume=.25f;music.Play();sfx=gameObject.AddComponent<AudioSource>();sfx.clip=Resources.Load<AudioClip>("Audio/bloom");}
 void StartLevel(int n){page=n;model=new GravityModel(n);clock=0;flash=0;targetFrom.Clear();}
 void Update(){
  if(page<0||model==null)return;
  if(Input.GetMouseButtonDown(0))swipe=Input.mousePosition;
  if(Input.GetMouseButtonUp(0)){Vector2 d=(Vector2)Input.mousePosition-swipe;Vector2 start=new Vector2((swipe.x-origin.x)/scale,(Screen.height-swipe.y-origin.y)/scale);if(board.Contains(start)&&d.magnitude>35){if(Mathf.Abs(d.x)>Mathf.Abs(d.y))model.SetDirection(d.x<0?Direction.Left:Direction.Right);else if(d.y<0)model.SetDirection(Direction.Down);}}
  if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A))model.SetDirection(Direction.Left);
  if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.D))model.SetDirection(Direction.Right);
  if(Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown(KeyCode.S))model.SetDirection(Direction.Down);
  targetTween+=Time.deltaTime/.28f;clock+=Time.deltaTime;flash=Mathf.Max(0,flash-Time.deltaTime);
  if(clock>=.48f&&!model.won&&!model.lost){clock-=.48f;var old=new Dictionary<Cell,int>(model.targets);model.Tick();PrepareFalls(old);if(model.cleared.Count>0){flash=.4f;if(sound)sfx.Play();}if(model.won){unlocked=Mathf.Max(unlocked,Mathf.Min(2,page+1));PlayerPrefs.SetInt("garden.unlocked",unlocked);PlayerPrefs.Save();}}
 }
 void Box(Rect r,Color c){GUI.color=c;if(round)GUI.DrawTexture(r,round);else GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
 void Label(Rect r,string s,int size,Color c,TextAnchor align=TextAnchor.MiddleLeft){GUI.Label(r,s,new GUIStyle{font=font,fontSize=size,normal={textColor=c},alignment=align,wordWrap=true});}
 bool Button(Rect r,string text,bool active=false){Box(new Rect(r.x,r.y+4,r.width,r.height),new Color(.82f,.82f,.77f));Box(r,active?new Color(.75f,.85f,.75f):new Color(1,.98f,.93f));Label(r,text,20,ink,TextAnchor.MiddleCenter);return GUI.Button(r,"",GUIStyle.none);}
 void OnGUI(){
  scale=Mathf.Min(Screen.width/420f,Screen.height/900f);origin=new Vector2((Screen.width-420*scale)/2,(Screen.height-900*scale)/2);GUI.color=new Color(.92f,.94f,.88f);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;
  GUI.matrix=Matrix4x4.TRS(new Vector3(origin.x,origin.y,0),Quaternion.identity,new Vector3(scale,scale,1));
  Box(new Rect(0,0,420,900),new Color(.99f,.97f,.93f));if(paper){GUI.color=new Color(1,1,1,.25f);GUI.DrawTexture(new Rect(0,0,420,900),paper,ScaleMode.StretchToFill);GUI.color=Color.white;}
  if(page<0){Home();return;}
  if(Button(new Rect(24,24,60,38),"返回")){page=-1;return;}if(Button(new Rect(331,24,65,38),sound?"♫ 开":"♫ 关")){sound=!sound;music.mute=!sound;}
  Label(new Rect(28,83,364,22),"PIXEL GRAVITY  /  CHAPTER 0"+(page+1),12,new Color(.51f,.59f,.54f));
  Label(new Rect(28,112,365,44),names[page],28,ink);
  Label(new Rect(28,159,364,40),tips[page],14,new Color(.52f,.57f,.57f));
  Box(new Rect(28,211,364,497),new Color(.86f,.88f,.82f));Box(new Rect(30,209,360,497),new Color(1,.995f,.97f));
  for(int y=0;y<GravityModel.Height;y++)for(int x=0;x<GravityModel.Width;x++)Box(new Rect(board.x+x*unit+1,board.y+y*unit+1,unit-2,unit-2),new Color(.94f,.95f,.91f));
  foreach(var pair in model.targets)Tile(pair.Key,pair.Value,1);
  foreach(var m in model.motes){Vector2 pos=new Vector2(m.cell.x,m.cell.y);bool wrap=Mathf.Abs(m.cell.x-m.previous.x)>1;if(!wrap)pos=Vector2.Lerp(new Vector2(m.previous.x,m.previous.y),pos,Mathf.Clamp01(clock/.48f));Vector2 dir=model.gravity==Direction.Down?Vector2.down:model.gravity==Direction.Left?Vector2.right:Vector2.left; // GUI y points down
   if(model.gravity==Direction.Down)dir=Vector2.up;
   for(int i=3;i>=1;i--)DrawTile(pos+dir*(i*.23f),m.color,.13f*(4-i),.62f);DrawTile(pos,m.color,1,.84f);
  }
  if(flash>0)foreach(var p in model.cleared){if(flash>.25f)Box(new Rect(board.x+p.x*unit+1,board.y+p.y*unit+1,unit-2,unit-2),new Color(1,.87f,.67f,(flash-.25f)/.15f));Vector2 center=new Vector2(board.x+(p.x+.5f)*unit,board.y+(p.y+.5f)*unit);for(int i=0;i<3;i++){float angle=i*2.094f+p.x;Vector2 v=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(1-flash/.4f)*22;Box(new Rect(center.x+v.x,center.y+v.y,4,4),new Color(1,.72f,.48f,flash/.4f));}}
  Box(new Rect(board.x,board.y+18*unit,338,2),new Color(.89f,.65f,.62f));
  Label(new Rect(37,708,346,30),"已收集 "+model.removed+" / "+model.Total+"    ·    重力 "+(model.gravity==Direction.Down?"↓ 向下":model.gravity==Direction.Left?"← 向左":"→ 向右"),14,ink,TextAnchor.MiddleCenter);
  if(Button(new Rect(38,756,105,62),"← 左",model.gravity==Direction.Left))model.SetDirection(Direction.Left);
  if(Button(new Rect(157,756,105,62),"↓ 下",model.gravity==Direction.Down))model.SetDirection(Direction.Down);
  if(Button(new Rect(276,756,105,62),"右 →",model.gravity==Direction.Right))model.SetDirection(Direction.Right);
  Label(new Rect(30,838,360,30),"滑动棋盘也能改变重力  ·  底部粉线为危险线",12,new Color(.57f,.61f,.58f),TextAnchor.MiddleCenter);
  if(model.won||model.lost)Result();
 }
 void PrepareFalls(Dictionary<Cell,int> old){
  targetFrom.Clear();var used=new HashSet<Cell>();foreach(var p in model.targets)if(old.ContainsKey(p.Key)&&old[p.Key]==p.Value)used.Add(p.Key);
  foreach(var p in model.targets){if(used.Contains(p.Key))continue;float best=float.MaxValue;Cell from=p.Key;bool found=false;foreach(var q in old){if(q.Value!=p.Value||used.Contains(q.Key)||model.cleared.Contains(q.Key))continue;float distance=(q.Key.x-p.Key.x)*(q.Key.x-p.Key.x)+(q.Key.y-p.Key.y)*(q.Key.y-p.Key.y);if(distance<best){best=distance;from=q.Key;found=true;}}if(found){used.Add(from);targetFrom[p.Key]=new Vector2(from.x,from.y);}}
  targetTween=0;
 }
 void Tile(Cell c,int color,float alpha){Vector2 end=new Vector2(c.x,c.y),start;if(targetFrom.TryGetValue(c,out start))end=Vector2.Lerp(start,end,Mathf.SmoothStep(0,1,Mathf.Clamp01(targetTween)));DrawTile(end,color,alpha,.9f);}
 void DrawTile(Vector2 c,int index,float alpha,float size){Color col=palette[index];col.a=alpha;Rect r=new Rect(board.x+(c.x+(1-size)/2)*unit,board.y+(c.y+(1-size)/2)*unit,unit*size,unit*size);Box(new Rect(r.x,r.y+2,r.width,r.height),new Color(col.r*.9f,col.g*.9f,col.b*.9f,alpha));Box(r,col);Box(new Rect(r.x+4,r.y+3,r.width*.43f,2),new Color(1,1,1,.55f*alpha));}
 void Home(){
  Label(new Rect(30,41,360,26),"A LITTLE GARDEN OF GRAVITY",12,new Color(.49f,.60f,.52f),TextAnchor.MiddleCenter);
  Label(new Rect(20,82,380,63),"像素重力场",42,ink,TextAnchor.MiddleCenter);Label(new Rect(20,147,380,32),"让小小的引力，开出温柔的花。",16,new Color(.55f,.60f,.55f),TextAnchor.MiddleCenter);
  if(hero)GUI.DrawTexture(new Rect(37,202+Mathf.Sin(Time.realtimeSinceStartup*1.4f)*4,346,241),hero,ScaleMode.ScaleToFit);
  for(int i=0;i<3;i++){Rect r=new Rect(32,479+i*101,356,81);Box(new Rect(r.x,r.y+4,r.width,r.height),new Color(.88f,.87f,.81f));Box(r,i<=unlocked?Color.white:new Color(.93f,.93f,.89f));Box(new Rect(48,r.y+18,43,43),palette[i]);Label(new Rect(48,r.y+18,43,43),"0"+(i+1),18,Color.white,TextAnchor.MiddleCenter);Label(new Rect(108,r.y+8,260,37),names[i],19,ink);Label(new Rect(108,r.y+42,255,25),i>unlocked?"完成上一关后开启":new[]{"基础重力  /  整片消除","多色匹配  /  失去支撑","左右环绕  /  综合挑战"}[i],12,new Color(.57f,.62f,.59f));if(i<=unlocked&&GUI.Button(r,"",GUIStyle.none))StartLevel(i);}
  Label(new Rect(25,818,370,40),"三段小旅程 · 一点点耐心 · 一整片晴天",13,new Color(.56f,.62f,.56f),TextAnchor.MiddleCenter);
 }
 void Result(){Box(new Rect(0,0,420,900),new Color(.23f,.32f,.29f,.55f));Box(new Rect(38,294,344,296),new Color(1,.98f,.93f));Label(new Rect(58,320,304,61),model.won?"花园，晴了。":"再试一次吧",32,ink,TextAnchor.MiddleCenter);Label(new Rect(63,391,294,61),model.won?(page==2?"三座小花园都已点亮。\n愿你今天也有好心情。":"收集了这一片温柔，\n去看看下一座花园。深呼吸，再出发。"):"星粒越过了底部危险线。\n试试及时转向，让同色相遇。",16,ink,TextAnchor.MiddleCenter);if(Button(new Rect(68,479,284,49),model.won?(page<2?"下一座花园 →":"回到花园"):"重新开始")){if(model.won&&page==2)page=-1;else StartLevel(model.won?page+1:page);}if(Button(new Rect(122,544,176,30),"返回选关"))page=-1;}
}
