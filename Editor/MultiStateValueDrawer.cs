using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GI.UnityToolkit.State.Editor
{
    [CustomPropertyDrawer(typeof(MultiStateValue<State>))]
    public class MultiStateValueDrawer : MultiStateValueDrawer<State> {}

    /// <summary>
    /// Generic property drawer for MultiStateValue properties.
    /// Note that you'll need to add your own drawer inheriting from this if you implement a custom, inherited StateBase type.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public class MultiStateValueDrawer<TState> : PropertyDrawer where TState : StateBase
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects) return;

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var multiStateValue = (MultiStateValue<TState>)PropertyDrawerUtilities.GetTargetObjectOfProperty(property);

            string dropdownString;
            if (multiStateValue.ActiveStates.Count == 0)
            {
                dropdownString = "None";
            }
            else if (multiStateValue.AreAllActive())
            {
                dropdownString = "All";
            }
            else
            {
                dropdownString = multiStateValue.ToString();
            }

            if (EditorGUI.DropdownButton(position, new GUIContent(dropdownString), FocusType.Keyboard))
            {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("None"), multiStateValue.ActiveStates.Count == 0,
                    _ =>
                    {
                        multiStateValue.SetAllInactive();
                        FinalizeChanges();
                    }, null);

                foreach (var state in multiStateValue.AvailableStates)
                {
                    menu.AddItem(new GUIContent(state.name), multiStateValue.IsActive(state),
                        _ =>
                        {
                            multiStateValue.Toggle(state);
                            FinalizeChanges();
                        }, null);
                }

                menu.AddItem(new GUIContent("All"),
                    multiStateValue.ActiveStates.Count == multiStateValue.AvailableStates.Count,
                    _ =>
                    {
                        if (multiStateValue.AreAllActive())
                        {
                            multiStateValue.SetAllInactive();
                        }
                        else
                        {
                            multiStateValue.SetAllActive();
                        }

                        FinalizeChanges();
                    }, null);

                menu.ShowAsContext();
            }

            void FinalizeChanges()
            {
                property.serializedObject.ApplyModifiedProperties();
                PropertyDrawerUtilities.CallOnValueChangedCallbacks(property);
            }
        }
    }
}