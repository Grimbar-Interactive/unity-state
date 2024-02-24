using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;
using UnityEngine;
// ReSharper disable StringLiteralTypo

namespace GI.UnityToolkit.State.Editor
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [InitializeOnLoad]
    public class DependencyResolver : AssetPostprocessor
    {
        private static readonly (string, string)[] Dependencies =
        {
            ("com.grimbarinteractive.unityvariables", "https://github.com/Grimbar-Interactive/unity-variables.git"),
            #if !ODIN_INSPECTOR
            ("com.dbrizov.naughtyattributes", "https://github.com/dbrizov/NaughtyAttributes.git#upm")
            #endif
        };

        static DependencyResolver()
        {
            CompilationPipeline.compilationStarted += OnCompilationStarted;
        }

        private static void OnCompilationStarted(object obj) => InstallDependencies();
        private void OnPreprocessAsset() => InstallDependencies();

        [InitializeOnLoadMethod]
        public static async void InstallDependencies()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(assembly);
            if (packageInfo == null)
            {
                Debug.Log("[DependencyResolver] Skipping dependency resolution: This package is not installed as a UPM package. ");
                return;
            }

            var value = Client.List(false, true);
 
            while (!value.IsCompleted)
                await Task.Delay(100);

            foreach (var (packageName, url) in Dependencies)
            {
                if (value.Result.Any(item => item.name == packageName)) return;
                
                Debug.LogWarning($"[DependencyResolver] The dependency \"{packageName}\" is not installed! Installing from \"{url}\"...");
                Client.Add(url);
            }
            
            
        }
    }
}