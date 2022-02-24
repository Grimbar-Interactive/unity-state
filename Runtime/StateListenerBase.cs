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

        public void OnStateChanged(TState state)
        {
            if (activeStates.Contains(state))
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
