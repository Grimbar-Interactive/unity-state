using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace GI.UnityToolkit.State
{
    public abstract class StateListenerBase<TState, TManager> : MonoBehaviour, IStateListener<TState>
        where TState: StateBase
        where TManager: StateManagerBase<TState>
    {
        [Title("Settings")]
        [SerializeField] private TManager manager = null;
        [SerializeField, Space(4)] private List<TState> activeStates = null;
        
        [Title("Events")]
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
