using Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ava;

class Program
{
    static readonly List<string> extraLibs = [];

    [ModuleInitializer]
    internal static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var libs = assembly.GetManifestResourceNames();
        foreach (var lib in libs){
            using var stream = assembly.GetManifestResourceStream(lib);
            if (stream != null)
            {
                var extra = string.Join('.', lib.Split('.').Skip(3));
                if(extra.Length > 0 && !File.Exists(extra)){
                    using var file = File.Create(extra);
                    stream!.CopyTo(file);
                    file.Close();
                    extraLibs.Add(extra);
                }
            }
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
            foreach (var lib in extraLibs)
            {
                File.Delete(lib);
            }
        }
    }
}
