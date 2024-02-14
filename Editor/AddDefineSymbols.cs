using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Compilation;

namespace GI.UnityToolkit.State.Editor
{
    /// <summary>
    /// Adds the given define symbols to PlayerSettings define symbols.
    /// Just add your own define symbols to the Symbols property at the below.
    /// </summary>
    [InitializeOnLoad]
    public class AddDefineSymbols : AssetPostprocessor, IActiveBuildTargetChanged
    {
        /// <summary>
        /// Symbols that will be added to the editor
        /// </summary>
        private static readonly string[] Symbols =
        {
            "GI_STATE"
        };
        
        public int callbackOrder => 0;

        static AddDefineSymbols()
        {
            CompilationPipeline.compilationStarted += ProcessBatchModeCompileFinish;
        }

        private static void ProcessBatchModeCompileFinish(object obj) => AddDefinesAsNeeded();
        private void OnPreprocessAsset() => AddDefinesAsNeeded();
        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget) => AddDefinesAsNeeded();

        private static void AddDefinesAsNeeded()
        {
            var definesString = PlayerSettings.GetScriptingDefineSymbols(CurrentNamedBuildTarget);
            var allDefines = definesString.Split(';').ToList();
            allDefines.AddRange(Symbols.Except(allDefines));
            PlayerSettings.SetScriptingDefineSymbols(CurrentNamedBuildTarget, allDefines.ToArray());
        }

        private static NamedBuildTarget CurrentNamedBuildTarget
        {
            get
            {
#if UNITY_SERVER
                return NamedBuildTarget.Server;
#else
                var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
                var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
                return namedBuildTarget;
#endif
            }
        }
    }
}