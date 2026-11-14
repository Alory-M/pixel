from PIL import Image,ImageDraw,ImageFilter
import random, math, wave, struct
from pathlib import Path
random.seed(42)
root=Path(__file__).resolve().parents[1]/'Assets/Resources'
a=root/'Art'; a.mkdir(parents=True,exist_ok=True)
s=3
im=Image.new('RGBA',(1040,720),(251,247,236,255));d=ImageDraw.Draw(im)
def line(points,fill,width=3):
 d.line(points,fill=fill,width=width,joint='curve')
def ellipse(box,fill,outline=None,width=3):d.ellipse(box,fill=fill,outline=outline,width=width)
# A softly inked floating island, two small anime-inspired garden spirits and flowers.
ink='#657b72'; pink='#f3b6bc'; green='#b7d2b7'; blue='#bdcfea'
ellipse((170,601,875,650),'#e9e7d9')
d.polygon([(217,458),(819,458),(730,578),(540,629),(320,552)],fill='#e3d7bf')
line([(217,458),(320,552),(540,629),(730,578),(819,458)],'#c8b9a0',4)
ellipse((205,386,827,516),green,ink,4)
for x in range(260,790,42):
 y=449+random.randrange(-19,16);line([(x,y+12),(x-5,y),(x-13,y-4)],'#8fac93',2)
# clouds
for x,y in [(173,173),(781,123),(838,307)]:
 for dx,dy,r in [(0,0,35),(34,-20,43),(75,2,34)]:ellipse((x+dx-r,y+dy-r,x+dx+r,y+dy+r),'#ffffff')
# flowers
for x,y,c in [(327,357,pink),(697,351,blue),(750,405,pink),(264,418,'#f1d68f')]:
 line([(x,y+64),(x,y-8)],ink,4);ellipse((x-30,y+25,x,y+40),green,ink,2)
 for k in range(5):
  dx=math.cos(k*math.tau/5)*25;dy=math.sin(k*math.tau/5)*25
  ellipse((x+dx-20,y+dy-20,x+dx+20,y+dy+20),c,ink,2)
 ellipse((x-13,y-13,x+13,y+13),'#f5e4a6',ink,2)
# a rabbit gardener
ellipse((430,224,469,357),'#fffaf0',ink,4);ellipse((491,215,531,352),'#fffaf0',ink,4)
ellipse((440,239,458,326),pink);ellipse((501,230,520,321),pink)
ellipse((409,321,553,464),'#fffaf0',ink,4)
ellipse((427,429,467,478),'#fffaf0',ink,3);ellipse((500,429,539,478),'#fffaf0',ink,3)
ellipse((442,375,451,388),ink);ellipse((509,375,518,388),ink)
ellipse((424,389,451,402),pink);ellipse((512,389,539,402),pink)
line([(476,396),(482,402),(488,396)],ink,3)
# blue companion
ellipse((572,382,646,466),blue,ink,3);ellipse((582,408,588,415),ink);ellipse((626,408,632,415),ink)
line([(601,425),(607,429),(613,425)],ink,2)
for x,y in [(381,223),(653,251),(238,286),(734,222)]:
 line([(x-9,y),(x+9,y)],'#d7b97d',3);line([(x,y-9),(x,y+9)],'#d7b97d',3)
# paper grain
for _ in range(1400):
 x=random.randrange(1040);y=random.randrange(720);d.point((x,y), fill=(239,233,221,255))
im.save(a/'garden.png')
p=Image.new('RGB',(256,256));px=p.load()
for y in range(256):
 for x in range(256):
  v=random.randrange(-4,5);px[x,y]=(max(0,min(255,248+v)),max(0,min(255,246+v)),max(0,min(255,237+v)))
p.save(a/'paper.png')
r=Image.new('RGBA',(96,96));ImageDraw.Draw(r).rounded_rectangle((1,1,94,94),radius=15,fill='white');r.save(a/'rounded.png')
# Original 32-second pentatonic music, sample-accurate loop with no external recordings.
audio=root/'Audio';audio.mkdir(exist_ok=True)
rate=22050;duration=32;n=rate*duration;buf=[0.0]*n
notes=[72,76,79,81,79,76,74,67,72,74,76,79,76,74,72,67,69,72,76,79,76,72,69,67,65,69,72,76,74,72,69,67]
def note(start,length,midi,amp):
 f=440*2**((midi-69)/12)
 for j in range(int(length*rate)):
  idx=(int(start*rate)+j)%n;t=j/rate
  env=min(1,t/.025)*math.exp(-t*3/length)*min(1,(length-t)/.15)
  v=(math.sin(math.tau*f*t)+.22*math.sin(math.tau*f*2*t))*env*amp
  buf[idx]+=v
for i,m in enumerate(notes):note(i,1.7,m,.115)
for bar,chord in enumerate([(48,55,60),(45,52,57),(41,48,53),(43,50,55)]*2):
 for j,m in enumerate(chord):note(bar*4+j*.18,3.8,m,.06)
def save(path,data):
 with wave.open(str(path),'wb') as w:
  w.setparams((1,2,rate,0,'NONE','not compressed'));w.writeframes(b''.join(struct.pack('<h',int(max(-.95,min(.95,v))*32767)) for v in data))
save(audio/'afternoon.wav',buf)
buf=[0.0]*int(rate*.8);n=len(buf)
for i,m in enumerate([79,84,88]):note(i*.08,.48,m,.14)
save(audio/'bloom.wav',buf)
print('Generated 3 illustrations/textures and 2 original audio assets.')
