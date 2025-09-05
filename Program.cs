using Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ava;

class Program
{
   public static void Restart()
    {
        string? currentExe = Environment.ProcessPath;
        ProcessStartInfo startInfo = new()
        {
            FileName = currentExe,
            Arguments = Environment.CommandLine.Replace($"\"{currentExe}\"", "").Trim(),
            UseShellExecute = true
        };
        Process.Start(startInfo);
        Environment.Exit(0);
    }
    
    [ModuleInitializer]
    internal static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();
        string tempPath = Path.Combine(Path.GetTempPath(), assembly.GetName().Name!);
        Directory.CreateDirectory(tempPath);
        Console.WriteLine($"tempPath: {tempPath}");
        var libs = assembly.GetManifestResourceNames();
        foreach (var lib in libs){
            using var stream = assembly.GetManifestResourceStream(lib);
            if (stream != null)
            {
                var fn = string.Join('.', lib.Split('.').Skip(3));
                var extra = Path.Combine(tempPath, fn);
                if(fn.Length > 0 && !File.Exists(extra)){
                    using var file = File.Create(extra);
                    stream!.CopyTo(file);
                    file.Close();
                }
            }
        }

        if(Environment.GetEnvironmentVariable("DYLD_LIBRARY_PATH") != tempPath)
        {
            Environment.SetEnvironmentVariable("DYLD_LIBRARY_PATH", tempPath);
            Restart();
        }
    }

    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .WithInterFont()
                .StartWithClassicDesktopLifetime(args);
        }finally
        {
            var assembly = Assembly.GetEntryAssembly();
            string tempPath = Path.Combine(Path.GetTempPath(), assembly.GetName().Name!);
            Directory.Delete(tempPath, true);
        }
    }
}
