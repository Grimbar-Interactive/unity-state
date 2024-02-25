#if !UNITY_2019
using System;
using System.Collections.Generic;
using System.Linq;
using GI.UnityToolkit.Variables;
using JetBrains.Annotations;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using GI.UnityToolkit.Attributes;
#endif

namespace GI.UnityToolkit.State
{
    public abstract class MultiStateManagerBase<TState> : DataObject where TState : StateBase
    {
#if ODIN_INSPECTOR
        [field: SerializeField, Space(10), OnValueChanged(nameof(OnAvailableStatesChanged))]
#else
        [field: SerializeField, Space(10), OnValueChanged(nameof(OnAvailableStatesChanged))]
#endif
        public List<TState> States { get; private set; }


#if ODIN_INSPECTOR
        [field: SerializeField, Space(10), OnValueChanged(nameof(OnDefaultActiveStatesChanged))]
#else
        [field: SerializeField, Space(10), OnValueChanged(nameof(OnDefaultActiveStatesChanged))]
#endif
        public MultiStateValue<TState> DefaultActiveStates { get; private set; }

#if ODIN_INSPECTOR
        [field: SerializeField, LabelText("Current Active States"), DisableInEditorMode,
         OnValueChanged(nameof(OnCurrentActiveStatesChanged)), Title("Runtime State")]
#else
        [UsedImplicitly] private bool IsEditor => Application.isPlaying == false;

        [field: SerializeField, Label("Current Active States"), DisableIf(nameof(IsEditor)),
                OnValueChanged(nameof(OnCurrentActiveStatesChanged)), Header("Runtime States")]
#endif
        public MultiStateValue<TState> CurrentActiveStates { get; protected set; }

#if ODIN_INSPECTOR
        [field: SerializeField, ReadOnly]
#else
        [UsedImplicitly] private bool DisableEditorField => true;

        [field: SerializeField, DisableIf(nameof(DisableEditorField))]
#endif
        public MultiStateValue<TState> PreviousActiveStates { get; protected set; }

#if ODIN_INSPECTOR
        [NonSerialized, ShowInInspector, ListDrawerSettings(IsReadOnly = true), Title("Runtime Listeners"), PropertyOrder(1)]
#else
        [NonSerialized]
#endif
        private readonly List<IMultiStateListener<TState>> _listeners = new List<IMultiStateListener<TState>>();

        private MultiStateValue<TState> _lastSentStates;

        protected override void OnBegin()
        {
            base.OnBegin();
            ResetStateListsToMatchDefault();
            PreviousActiveStates.SetAllInactive();
            OnStatesChanged();
        }

        protected override void OnEnd()
        {
            base.OnEnd();
            _listeners.Clear();
        }

        [UsedImplicitly]
        public void Set(IEnumerable<TState> newStates) => ApplyChange(CurrentActiveStates.Set, States);

        [UsedImplicitly]
        public void SetStateAsActive(TState state) => ApplyChange(CurrentActiveStates.SetActive, new[] { state });

        [UsedImplicitly]
        public void UnsetStateAsActive(TState state) => ApplyChange(CurrentActiveStates.SetInactive, new[] { state });
        
        [UsedImplicitly]
        public void ToggleStateActive(TState state)
        {
            Func<IEnumerable<TState>, bool> method;
            if (CurrentActiveStates.IsActive(state))
            {
                method = CurrentActiveStates.SetInactive;
            }
            else
            {
                method = CurrentActiveStates.SetActive;
            }
            ApplyChange(method, new[] { state });
        }

        [UsedImplicitly]
        public void ToggleAllActive() => ApplyChange(CurrentActiveStates.Set,
            CurrentActiveStates.AreAllActive() ? new List<TState>() : States.ToList());

        [UsedImplicitly]
        public void SetToPreviousActiveStates() => ApplyChange(CurrentActiveStates.Set, PreviousActiveStates.ActiveStates);

        [UsedImplicitly]
        public void Default() => ApplyChange(CurrentActiveStates.Set, DefaultActiveStates.ActiveStates);

        public void SetAllActive()
        {
            CurrentActiveStates.SetAllActive();
        }

        public void SetAllInactive()
        {
            CurrentActiveStates.SetAllInactive();
        }

        public bool AreAllActive() => CurrentActiveStates.AreAllActive();
        public bool IsActive(TState state) => CurrentActiveStates.IsActive(state);

        private void ApplyChange(Func<IEnumerable<TState>, bool> action, IEnumerable<TState> statesToApply)
        {
            var currentStates = new List<TState>(CurrentActiveStates.ActiveStates);

            var changed = action?.Invoke(statesToApply);
            if (changed == false) return;

            PreviousActiveStates.Set(currentStates);
            _lastSentStates.Set(CurrentActiveStates);
            OnStatesChanged();
        }

        public void RegisterListener(IMultiStateListener<TState> listener)
        {
            if (listener == null) return;
            _listeners.Add(listener);
        }

        public void UnregisterListener(IMultiStateListener<TState> listener)
        {
            _listeners.Remove(listener);
        }

        private void OnStatesChanged()
        {
            for (var i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i].OnStateChanged(PreviousActiveStates, CurrentActiveStates);
            }
        }

        private void OnAvailableStatesChanged()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[MultiStateManagerBase] Changing available states during runtime is not explicitly supported! Weird behaviour may occur, setting the current active states to default to play it safe.");
            }
            var defaultStates = DefaultActiveStates.ActiveStates.Intersect(States);
            DefaultActiveStates = new MultiStateValue<TState>(States, defaultStates);
            ResetStateListsToMatchDefault();
        }

        [UsedImplicitly]
        private void OnDefaultActiveStatesChanged()
        {
            if (Application.isPlaying) return;
            ResetStateListsToMatchDefault();
        }

        [UsedImplicitly]
        private void OnCurrentActiveStatesChanged()
        {
            PreviousActiveStates.Set(_lastSentStates);
            _lastSentStates.Set(CurrentActiveStates);
            OnStatesChanged();
        }

        private void OnValidate()
        {
            if (DefaultActiveStates == null) DefaultActiveStates = new MultiStateValue<TState>(States);
            if (CurrentActiveStates == null) CurrentActiveStates = new MultiStateValue<TState>(States, DefaultActiveStates);
            if (PreviousActiveStates == null) PreviousActiveStates = new MultiStateValue<TState>(States, DefaultActiveStates);
            if (_lastSentStates == null) _lastSentStates = new MultiStateValue<TState>(States, DefaultActiveStates);

            if (Application.isPlaying) return;
            ResetStateListsToMatchDefault();
        }

        private void ResetStateListsToMatchDefault()
        {
            if (DefaultActiveStates == null) DefaultActiveStates = new MultiStateValue<TState>(States);
            CurrentActiveStates = new MultiStateValue<TState>(States, DefaultActiveStates);
            PreviousActiveStates = new MultiStateValue<TState>(States, DefaultActiveStates);
            _lastSentStates = new MultiStateValue<TState>(States, DefaultActiveStates);
        }
    }
}
#endif