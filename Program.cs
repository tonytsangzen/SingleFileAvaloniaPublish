using Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ava;

class Program
{
    static string GetEnvironmentVariableName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "PATH";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "LD_LIBRARY_PATH";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "DYLD_LIBRARY_PATH";
        else
            return "UNKNOW";
    }

    static string GetFileName(string src){
        var temp = src.Split('.');
        return string.Join('.', temp.Skip(temp.Length - 2));
    }

    static void Restart()
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
        var files = assembly.GetManifestResourceNames();
        foreach (var f in files){
            using var stream = assembly.GetManifestResourceStream(f);
            if (stream != null)
            {
                var fn = GetFileName(f);
                var extra = Path.Combine(tempPath, fn);
                if(fn.Length > 0 && !File.Exists(extra)){
                    using var file = File.Create(extra);
                    stream!.CopyTo(file);
                    file.Close();
                }
            }
        }

        var env = GetEnvironmentVariableName();
        if (Environment.GetEnvironmentVariable(env) != tempPath)
        {
            Environment.SetEnvironmentVariable(env, tempPath);
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
            string tempPath = Path.Combine(Path.GetTempPath(), assembly!.GetName().Name!);
            Directory.Delete(tempPath, true);
        }
    }
}
