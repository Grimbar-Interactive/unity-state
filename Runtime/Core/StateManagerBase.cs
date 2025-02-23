using System;
using System.Collections.Generic;
using GI.UnityToolkit.Utilities;
using GI.UnityToolkit.Variables;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using GI.UnityToolkit.Attributes;
#endif

namespace GI.UnityToolkit.State
{
    public abstract class StateManagerBase<TState> : DataObject where TState : StateBase
    {
        [SerializeField, Space(10)] private List<TState> states = new List<TState>();
        
#if ODIN_INSPECTOR
        [ValueDropdown("states"), OnValueChanged(nameof(OnDefaultStateChanged))]
#else
        private bool HasStates => states.Count > 0;
        
        [ShowIf("HasStates"), Dropdown("states"), OnValueChanged(nameof(OnDefaultStateChanged))]
#endif
        [SerializeField, Space(10)] private TState defaultState = null;
        
        [UsedImplicitly]
        public List<TState> States => states;
        
        [UsedImplicitly]
        public TState DefaultState => defaultState;
        
#if ODIN_INSPECTOR
        [ValueDropdown("states"), ShowInInspector, LabelText("Current State"), DisableInEditorMode, OnValueChanged("OnCurrentStateChanged")]
#else
        [UsedImplicitly]
        private bool IsEditor => Application.isPlaying == false;
        
        [field: SerializeField, ShowIf("HasStates"), Dropdown("states"), Label("Current State"), DisableIf("IsEditor"), OnValueChanged("OnCurrentStateChanged")]
#endif
        public TState CurrentState { get; protected set; }
        
#if ODIN_INSPECTOR
        [ShowInInspector, DisplayAsString, Title("Runtime State")]
#else
        [UsedImplicitly]
        private bool DisableEditorField => true;
        
        [field: SerializeField, ShowIf("HasStates"), Dropdown("states"), DisableIf(nameof(DisableEditorField)), Header("Runtime State")]
#endif
        public TState PreviousState { get; protected set; }
        
#if ODIN_INSPECTOR
        [NonSerialized, ShowInInspector, ListDrawerSettings(IsReadOnly = true), Title("Runtime Listeners"), PropertyOrder(1)]
#else
        [NonSerialized]
#endif
        private readonly List<IStateListener<TState>> _listeners = new List<IStateListener<TState>>();
        
        private TState _lastSentState = null;
        
#if ODIN_INSPECTOR
        [Title("Events"), PropertyOrder(4)]
#else
        [Header("Events")]
#endif
        [SerializeField] protected UnityEvent OnChangedEvent = default;
        
        public void AddOnChangedListener(UnityAction listener)
        {
            OnChangedEvent.AddListener(listener);
        }

        public void RemoveOnChangeListener(UnityAction listener)
        {
            OnChangedEvent.RemoveListener(listener);
        }
        
        protected override void OnBegin()
        {
            base.OnBegin();
            PreviousState = null;
            CurrentState = _lastSentState = DefaultState;
            OnStateChanged();
        }

        protected override void OnEnd()
        {
            base.OnEnd();
            _listeners.Clear();
        }

        [UsedImplicitly]
        public void SetState(TState state)
        {
            if (!states.Contains(state) || state == CurrentState) return;
            PreviousState = CurrentState;
            CurrentState = state;
            _lastSentState = CurrentState;
            OnStateChanged();
        }

        [UsedImplicitly]
        public void SetToPreviousState()
        {
            SetState(PreviousState);
        }

        [UsedImplicitly]
        public void Default()
        {
            SetState(DefaultState);
        }

        /// <summary>
        /// Updates the current state to the state forward (+) or backwards (-) on the state list.
        /// </summary>
        [UsedImplicitly]
        public void ShiftState(int amount)
        {
            SetState(states[(states.IndexOf(CurrentState) + amount).Mod(states.Count)]);
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
            OnChangedEvent?.Invoke();
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
