using System.Collections.Generic;
using GI.UnityToolkit.Variables;
using JetBrains.Annotations;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace GI.UnityToolkit.State
{
    public abstract class StateManagerBase<TState> : DataObject where TState : StateBase
    {
        [SerializeField, Space(10)] private List<TState> states = new List<TState>();
        
#if ODIN_INSPECTOR
        [ValueDropdown("states"), OnValueChanged("OnDefaultStateChanged")]
#endif
        [SerializeField] private TState defaultState = null;
        
        public List<TState> States => states;
        public TState DefaultState => defaultState;
        
#if ODIN_INSPECTOR
        [ShowInInspector, DisplayAsString]
#endif
        public TState PreviousState { get; protected set; }
        
#if ODIN_INSPECTOR
        [ValueDropdown("states"), ShowInInspector, LabelText("Current State"), DisableInEditorMode, OnValueChanged("OnCurrentStateChanged")]
#endif
        public TState CurrentState { get; protected set; }

        private readonly List<IStateListener<TState>> _listeners = new List<IStateListener<TState>>();

        private TState _lastSentState = null;
        
        protected override void OnBegin()
        {
            base.OnBegin();
            CurrentState = PreviousState = _lastSentState = DefaultState;
            OnStateChanged();
        }

        public void SetState(TState state)
        {
            if (!states.Contains(state) || state == CurrentState) return;
            PreviousState = CurrentState;
            CurrentState = state;
            _lastSentState = CurrentState;
            OnStateChanged();
        }

        public void Default()
        {
            SetState(DefaultState);
        }

        public void RegisterListener(IStateListener<TState> listener)
        {
            if (listener == null) return;
            _listeners.Add(listener);
        }

        public void UnregisterListener(IStateListener<TState> listener)
        {
            _listeners.Remove(listener);
        }

        private void OnStateChanged()
        {
            for (var i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i].OnStateChanged(PreviousState, CurrentState);
            }
        }

        [UsedImplicitly]
        private void OnDefaultStateChanged()
        {
            if (Application.isPlaying) return;
            CurrentState = PreviousState = _lastSentState = defaultState;
        }

        [UsedImplicitly]
        private void OnCurrentStateChanged()
        {
            PreviousState = _lastSentState;
            _lastSentState = CurrentState;
            OnStateChanged();
        }
        
        private void OnValidate()
        {
            if (states.Count == 0)
            {
                defaultState = CurrentState = PreviousState = _lastSentState = null;
                return;
            }
            
            if (defaultState == null)
            {
                if (states.Count == 0 || states[0] == null) return;
                defaultState = states[0];
            }
            
            if (!Application.isPlaying) CurrentState = PreviousState = _lastSentState = defaultState;
        }
    }
}
