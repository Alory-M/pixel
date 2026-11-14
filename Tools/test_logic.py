"""Run independent C# gameplay tests with .NET 8, without changing Unity files."""
import os, shutil, subprocess, json
from pathlib import Path
root=Path(__file__).resolve().parents[1];os.chdir(root)
dotnet=shutil.which('dotnet') or str(root/'Tools/cache/dotnet/dotnet')
info=subprocess.check_output([dotnet,'--list-sdks'],text=True).splitlines()
version,location=next((line.split(' ',1) for line in reversed(info) if line.startswith('8.')))
sdk=Path(location.strip('[]'))/version
base=sdk.parent.parent
refs=sorted((base/'packs/Microsoft.NETCore.App.Ref').glob('8.*/ref/net8.0'))[-1]
cache=root/'Tools/cache';cache.mkdir(parents=True,exist_ok=True)
args=[dotnet,str(sdk/'Roslyn/bincore/csc.dll'),'-nologo','-out:'+str(cache/'LogicTests.dll'),'-target:exe']+['-r:'+str(p) for p in refs.glob('*.dll')]+['Assets/Scripts/GravityModel.cs','Tests/LogicTests.cs']
subprocess.run(args,check=True)
(cache/'LogicTests.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net8.0','framework':{'name':'Microsoft.NETCore.App','version':'8.0.0'}}}))
raise SystemExit(subprocess.run([dotnet,str(cache/'LogicTests.dll')]).returncode)
