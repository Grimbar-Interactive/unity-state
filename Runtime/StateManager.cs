using System.Collections.Generic;
using UnityEngine;
using GI.UnityToolkit.Variables;

namespace GI.UnityToolkit.State
{
    [CreateAssetMenu(menuName = "Data Objects/State/State Manager")]
    public class StateManager : DataObject
    {
        [SerializeField] private List<State> states = new List<State>();
        [SerializeField] private State defaultState = null;
        
        public List<State> States => states;
        public State DefaultState => defaultState;
        public State CurrentState { get; protected set; }
        public State PreviousState { get; protected set; }

        private readonly List<IStateListener> _listeners = new List<IStateListener>();

        protected override void OnBegin()
        {
            base.OnBegin();
            CurrentState = DefaultState;
            OnStateChanged();
        }

        public void SetState(State state)
        {
            if (!states.Contains(state) || state == CurrentState)
            {
                Debug.Log($"returning: {state}, {CurrentState}");
                return;
            }

            PreviousState = CurrentState;
            CurrentState = state;
            
            OnStateChanged();
        }

        public void Default()
        {
            SetState(DefaultState);
        }

        public void RegisterListener(IStateListener listener)
        {
            if (listener == null) return;
            _listeners.Add(listener);
        }

        public void UnregisterListener(IStateListener listener)
        {
            _listeners.Remove(listener);
        }

        private void OnStateChanged()
        {
            for (var i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i].OnStateChanged(CurrentState);
            }
        }
    }
}