using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace GI.UnityToolkit.State
{
    public abstract class StateTransitionListenerBase<TState, TManager> : MonoBehaviour, IStateListener<TState>
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
        
        [SerializeField, HideIf(nameof(StateManagerIsNull)), ValueDropdown(nameof(StateOptions)), Space(4)]
#else
        [SerializeField, Space(4)] 
#endif
        private List<TState> from = null;
        
#if ODIN_INSPECTOR
        [SerializeField, HideIf(nameof(StateManagerIsNull)), ValueDropdown(nameof(StateOptions)), Space(4)]
#else
        [SerializeField, Space(4)] 
#endif
        private List<TState> to = null;
        
#if ODIN_INSPECTOR
        [Title("Events"), HideIf(nameof(StateManagerIsNull))]
#else
        [Header("Events")]
#endif
        [SerializeField, Tooltip("Triggers when a transition occurs from a state in the 'from' list to a state in the 'to' list.")]
        private UnityEvent response = null;

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
            if (from.Contains(previousState) && to.Contains(newState))
            {
                response?.Invoke();
            }
        }
    }
}