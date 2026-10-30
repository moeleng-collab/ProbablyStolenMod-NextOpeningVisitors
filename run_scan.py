from pathlib import Path
import subprocess
import sys

root = Path(__file__).resolve().parents[2]
project = Path(__file__).resolve().parent
tests = project / "tests"
dotnet = root / ".port-tools" / "dotnet" / "dotnet.exe"
csc = root / ".port-tools" / "dotnet" / "sdk" / "8.0.427" / "Roslyn" / "bincore" / "csc.dll"
refs = root / ".port-tools" / "dotnet" / "packs" / "Microsoft.NETCore.App.Ref" / "8.0.33" / "ref" / "net8.0"
output = tests / "ScanMod.dll"
args = [str(dotnet), str(csc), "-nologo", "-target:exe", f"-out:{output}", f"-reference:{root / 'Probably Stolen_Data' / 'Managed' / 'Assembly-CSharp.dll'}"]
args.extend(f"-reference:{path}" for path in refs.glob("*.dll"))
args.append(str(tests / "ScanMod.cs"))
subprocess.run(args, check=True, cwd=root)
result = subprocess.run([str(dotnet), str(output), str(root), str(project / "build" / "NextOpeningVisitors.dll")], cwd=tests)
sys.exit(result.returncode)
