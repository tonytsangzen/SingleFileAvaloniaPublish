using Avalonia;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ava;

class Program
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var libs = assembly.GetManifestResourceNames();
        foreach (var lib in libs){
            using var stream = assembly.GetManifestResourceStream(lib);
            if (stream != null)
            {
                var local = string.Join('.', lib.Split('.').Skip(3));
                if(local.Length > 0){
                    using var file = File.Create(local);
                    stream!.CopyTo(file);
                    file.Close();
                }
            }
        }
    }

    [STAThread]
    public static void Main(string[] args)
    {
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .WithInterFont()
            .StartWithClassicDesktopLifetime(args);
    }
}
