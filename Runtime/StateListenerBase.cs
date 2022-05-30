using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
        [SerializeField, Space(4)] private List<TState> activeStates = null;
        
#if ODIN_INSPECTOR
        [Title("Events")]
#else
        [Header("Events")]
#endif
        [SerializeField] private UnityEvent activeResponse = null;
        [SerializeField] private UnityEvent inactiveResponse = null;
        [SerializeField] private UnityEvent enteringResponse = null;
        [SerializeField] private UnityEvent leavingResponse = null;

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
            var wasActivePreviously = activeStates.Contains(previousState);
            var isActiveState = activeStates.Contains(newState);

            switch (wasActivePreviously)
            {
                case false when isActiveState:
                    enteringResponse?.Invoke();
                    break;
                case true when !isActiveState:
                    leavingResponse?.Invoke();
                    break;
            }
            
            if (isActiveState)
            {
                activeResponse?.Invoke();
            }
            else
            {
                inactiveResponse?.Invoke();
            }
        }
    }
}
