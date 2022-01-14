using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GI.UnityToolkit.State
{
    public class StateListener : MonoBehaviour, IStateListener
    {
        [SerializeField] private StateManager manager = null;
        [SerializeField] private List<State> activeStates = null;
        [SerializeField] private StateEvent response = null;
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

        public void OnStateChanged(State state)
        {
            response.Invoke(state);

            if (activeStates.Contains(state))
            {
                activeResponse?.Invoke();
            }
            else
            {
                inactiveResponse?.Invoke();
            }
        }

        [System.Serializable]
        public class StateEvent : UnityEvent<State> { }
    }
}