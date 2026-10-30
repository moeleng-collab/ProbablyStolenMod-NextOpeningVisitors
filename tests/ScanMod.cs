using System;
using System.IO;
using System.Runtime.Loader;

internal static class ScanMod
{
    private static int Main(string[] args)
    {
        string managed = Path.GetFullPath(Path.Combine(args[0], "Probably Stolen_Data", "Managed"));
        AssemblyLoadContext.Default.Resolving += (_, name) =>
        {
            string path = Path.Combine(managed, name.Name + ".dll");
            return File.Exists(path) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
        };
        return Run(args[1]);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static int Run(string dllPath)
    {
        string[] gameReferences = { "mscorlib", "netstandard", "System", "System.Core", "System.Numerics", "Assembly-CSharp", "0Harmony", "UnityEngine", "UnityEngine.CoreModule", "UnityEngine.UI", "Unity.TextMeshPro", "Newtonsoft.Json" };
        ModScanResult result = ModScanner.Scan(new[] { Path.GetFullPath(dllPath) }, gameReferences);
        Console.WriteLine("Error: " + (result.Error ?? "<none>"));
        Console.WriteLine("Refused: " + result.Refused);
        Console.WriteLine("MethodsScanned: " + result.MethodsScanned);
        Console.WriteLine("Capabilities: " + (result.Capabilities.Count == 0 ? "<none>" : string.Join(",", result.Capabilities)));
        Console.WriteLine("HardFlags: " + result.HardFlags.Count);
        Console.WriteLine("Findings: " + result.Findings.Count);
        Console.WriteLine(result.FormatFindings(100));
        return result.Refused ? 2 : 0;
    }
}
