using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;

namespace GI.UnityToolkit.State.Editor
{
    public class StateEventDrawer : OdinValueDrawer<StateListener.StateEvent>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var value = ValueEntry.SmartValue;

            EditorGUILayout.Space(2);

            if (value.GetPersistentEventCount() == 0)
            {
                // Display button only
                if (GUILayout.Button(label)) UnityEventTools.AddPersistentListener(value);
            }
            else
            {
                // Display default UI
                CallNextDrawer(label);
            }

            ValueEntry.SmartValue = value;
        }
    }
}