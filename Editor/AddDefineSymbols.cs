using System.Linq;
using UnityEditor;

namespace GI.UnityToolkit.State.Editor
{
    /// <summary>
    /// Adds the given define symbols to PlayerSettings define symbols.
    /// Just add your own define symbols to the Symbols property at the below.
    /// </summary>
    [InitializeOnLoad]
    public class AddDefineSymbols : UnityEditor.Editor
    {
        /// <summary>
        /// Symbols that will be added to the editor
        /// </summary>
        private static readonly string[] Symbols =
        {
            "GI_STATE"
        };

        /// <summary>
        /// Add define symbols as soon as Unity gets done compiling.
        /// </summary>
        static AddDefineSymbols()
        {
            var definesString =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var allDefines = definesString.Split(';').ToList();
            allDefines.AddRange(Symbols.Except(allDefines));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
        }
    }
}