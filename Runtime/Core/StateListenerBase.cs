using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace GI.UnityToolkit.State
{
    public abstract class StateListenerBase<TState, TManager> : MonoBehaviour, IStateListener<TState>
        where TState: StateBase
        where TManager: StateManagerBase<TState>
    {
#if ODIN_INSPECTOR
        [Title("Settings")]
#else
        [Header("Settings")]
#endif
        [SerializeField] private TManager manager = null;
        
#if ODIN_INSPECTOR
        private bool StateManagerIsNull => manager == null;
        private List<TState> StateOptions => manager != null ? manager.States : new List<TState>();

        [FormerlySerializedAs("activeStates")] [SerializeField, HideIf(nameof(StateManagerIsNull)), ValueDropdown(nameof(StateOptions)), Space(4)]
#else
        [FormerlySerializedAs("activeStates")] [SerializeField, Space(4)]
#endif
        private List<TState> statesListenedFor = null;
        
#if ODIN_INSPECTOR
        [Title("Events"), HideIfGroup("Events", Condition = nameof(StateManagerIsNull))]
#else
        [Header("Events")]
#endif
        [SerializeField, Tooltip("Triggers when a listened state is entered from a non-listened state.")]
        private UnityEvent enteringResponse = null;
#if ODIN_INSPECTOR
        [HideIfGroup("Events", Condition = nameof(StateManagerIsNull))]
#endif
        [SerializeField, Tooltip("Triggers when a non-listened state is entered from a listened state.")]
        private UnityEvent leavingResponse = null;

        private void OnEnable()
        {
            if (!manager) return;
            manager.RegisterListener(this);
        }

        private void OnDisable()
        {
            if (!manager) return;
            manager.UnregisterListener(this);
        }

        public void OnStateChanged(TState previousState, TState newState)
        {
            var wasActivePreviously = statesListenedFor.Contains(previousState);
            var isActiveState = statesListenedFor.Contains(newState);

            switch (wasActivePreviously)
            {
                case false when isActiveState:
                    enteringResponse?.Invoke();
                    break;
                case true when !isActiveState:
                    leavingResponse?.Invoke();
                    break;
            }
        }
    }
}
