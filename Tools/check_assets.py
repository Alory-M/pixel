from pathlib import Path
import wave,json
from PIL import Image
root=Path(__file__).resolve().parents[1]
for name in ['garden','paper','rounded']:
 p=root/f'Assets/Resources/Art/{name}.png'
 with Image.open(p) as im:
  im.verify()
 print('PASS image',name)
for name,seconds in [('afternoon',32),('bloom',.8)]:
 with wave.open(str(root/f'Assets/Resources/Audio/{name}.wav')) as f:
  assert f.getnchannels()==1 and f.getsampwidth()==2
  assert abs(f.getnframes()/f.getframerate()-seconds)<.001
 print('PASS audio',name,seconds,'seconds')
assert (root/'Assets/Resources/Art/Chinese.otf').stat().st_size>1000000
assert 'SIL OPEN FONT LICENSE' in (root/'Assets/Resources/Art/FONT-LICENSE.txt').read_text()
json.loads((root/'Packages/manifest.json').read_text())
assert 'b41ff65320eb4b8aaca8e77203d160b1' in (root/'Assets/Scenes/Garden.unity').read_text()
assert 'b41ff65320eb4b8aaca8e77203d160b1' in (root/'Assets/Scripts/PixelGarden.cs.meta').read_text()
print('PASS font, license, manifest, scene script reference')
