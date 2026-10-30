from pathlib import Path
import shutil
import subprocess
import sys

root = Path(__file__).resolve().parents[2]
project = Path(__file__).resolve().parent
sdk = root / ".port-tools" / "dotnet" / "sdk" / "8.0.427"
csc = sdk / "Roslyn" / "bincore" / "csc.dll"
managed = root / "Probably Stolen_Data" / "Managed"
out_dir = project / "build"
out_dir.mkdir(exist_ok=True)
out = out_dir / "NextOpeningVisitors.dll"

args = [
    str(root / ".port-tools" / "dotnet" / "dotnet.exe"),
    str(csc),
    "-nologo",
    "-target:library",
    "-platform:anycpu",
    "-langversion:latest",
    "-nullable:enable",
    "-optimize+",
    "-deterministic+",
    f"-out:{out}",
    f"-pdb:{out_dir / 'NextOpeningVisitors.pdb'}",
    "-debug:portable",
]
args.extend(f"-reference:{path}" for path in sorted(managed.glob("*.dll")))
args.extend(str(path) for path in sorted((project / "src").glob("*.cs")))
args.append(str(project / "Properties" / "AssemblyInfo.cs"))
subprocess.run(args, check=True, cwd=root)

install = root / "Mods" / "NextOpeningVisitors"
install.mkdir(parents=True, exist_ok=True)
shutil.copy2(out, install / out.name)
shutil.copy2(project / "manifest.xml", install / "manifest.xml")
print(f"Built and installed: {install}")
