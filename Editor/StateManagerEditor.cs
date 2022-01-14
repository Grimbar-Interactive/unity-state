using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GI.UnityToolkit.State.Editor
{
    [CustomEditor(typeof(StateManager))]
    public class StateManagerEditor : UnityEditor.Editor
    {
        private GUIStyle _style;
        private SerializedProperty _defaultState;
        private ReorderableList _statesList;

        private void Awake()
        {
            _style = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState()
                {
                    textColor = Color.yellow
                }
            };
        }

        private void OnEnable()
        {
            _defaultState = serializedObject.FindProperty("defaultState");
            
            _statesList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("states"),
                true,
                true,
                true,
                true
            )
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                        "Active States");
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    var element = _statesList.serializedProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;
                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                        element,
                        GUIContent.none
                    );
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _statesList.DoLayoutList();

            var manager = (StateManager) target;
            if (manager.States == null || manager.States.Count == 0)
            {
                _defaultState.objectReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(_defaultState);
            if (manager.DefaultState != null && !manager.States.Contains(manager.DefaultState))
            {
                GUILayout.Space(5);
                GUILayout.Label("Warning: Default state is not in states list!", _style);
            }
            
            GUILayout.Space(20);
            
            var currentStateName = manager.CurrentState == null ? manager.DefaultState != null ? manager.DefaultState.Name : "<None>" : manager.CurrentState.Name;
            if (GUILayout.Button($"Current State: {currentStateName}"))
            {
                var stateDropdown = new GenericMenu();
                foreach (var state in manager.States)
                {
                    if (state == null) continue;
                    stateDropdown.AddItem(new GUIContent(state.Name), manager.CurrentState == state,
                        delegate { manager.SetState(state); }, state);
                }

                stateDropdown.ShowAsContext();
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}