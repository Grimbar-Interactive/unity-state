using System.Collections.Generic;
using GI.UnityToolkit.Variables;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GI.UnityToolkit.State
{
    public abstract class StateManagerBase<TState> : DataObject where TState : StateBase
    {
        [SerializeField, Space(10)] private List<TState> states = new List<TState>();
        
        [ValueDropdown("states"), OnValueChanged("OnDefaultStateChanged")]
        [SerializeField] private TState defaultState = null;
        
        public List<TState> States => states;
        public TState DefaultState => defaultState;
        
        [ShowInInspector, DisplayAsString] public TState PreviousState { get; protected set; }
        
        [ValueDropdown("states"), ShowInInspector, LabelText("Current State"), DisableInEditorMode, OnValueChanged("OnCurrentStateChanged")]
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
