from pathlib import Path
import subprocess

root = Path(__file__).resolve().parents[2]
project = Path(__file__).resolve().parent
tests = project / "tests"
dotnet = root / ".port-tools" / "dotnet" / "dotnet.exe"
csc = root / ".port-tools" / "dotnet" / "sdk" / "8.0.427" / "Roslyn" / "bincore" / "csc.dll"
refs = root / ".port-tools" / "dotnet" / "packs" / "Microsoft.NETCore.App.Ref" / "8.0.33" / "ref" / "net8.0"
output = tests / "RulesTests.dll"
args = [str(dotnet), str(csc), "-nologo", "-target:exe", "-langversion:latest", "-nullable:enable", f"-out:{output}", f"-reference:{root / 'Probably Stolen_Data' / 'Managed' / 'Assembly-CSharp.dll'}"]
args.extend(f"-reference:{path}" for path in refs.glob("*.dll"))
args.extend([
    str(project / "src" / "VisitorRules.cs"),
    str(project / "src" / "CooldownResetRules.cs"),
    str(project / "src" / "ModLocalization.cs"),
    str(project / "src" / "PhoneDialogueTextRules.cs"),
    str(project / "src" / "CrimeForecastModels.cs"),
    str(project / "src" / "CrimeForecastRules.cs"),
    str(project / "src" / "AutoEvidenceCallState.cs"),
    str(project / "src" / "CrimeForecastLocalization.cs"),
    str(tests / "RulesTests.cs"),
])
subprocess.run(args, check=True, cwd=root)
subprocess.run([str(dotnet), str(output)], check=True, cwd=tests)
