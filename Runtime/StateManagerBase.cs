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
        
        [ValueDropdown("states"), ShowInInspector, LabelText("Current State"), DisableInEditorMode, OnValueChanged("OnStateChanged")]
        public TState CurrentState { get; protected set; }

        private readonly List<IStateListener<TState>> _listeners = new List<IStateListener<TState>>();

        protected override void OnBegin()
        {
            base.OnBegin();
            CurrentState = DefaultState;
            OnStateChanged();
        }

        public void SetState(TState state)
        {
            if (!states.Contains(state) || state == CurrentState)
            {
                Debug.Log($"returning: {state}, {CurrentState}");
                return;
            }

            CurrentState = state;
            
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
                _listeners[i].OnStateChanged(CurrentState);
            }
        }

        [UsedImplicitly]
        private void OnDefaultStateChanged()
        {
            if (Application.isPlaying) return;
            CurrentState = defaultState;
        }

        [UsedImplicitly]
        private void OnCurrentStateChanged()
        {
            OnStateChanged();
        }
        
        private void OnValidate()
        {
            if (states.Count == 0)
            {
                defaultState = CurrentState = null;
                return;
            }
            
            if (defaultState == null)
            {
                if (states.Count == 0 || states[0] == null) return;
                defaultState = states[0];
            }
            
            if (!Application.isPlaying) CurrentState = defaultState;
        }
    }
}
