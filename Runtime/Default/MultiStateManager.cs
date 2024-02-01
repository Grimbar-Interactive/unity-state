using UnityEngine;

#if UNITY_2019
using GI.UnityToolkit.Variables;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Linq;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif
#endif

namespace GI.UnityToolkit.State
{
    [CreateAssetMenu(menuName = "State/Multi-State Manager")]
#if !UNITY_2019
    public class MultiStateManager : MultiStateManagerBase<State> {}
#else
    public class MultiStateManager : DataObject
    {
#if ODIN_INSPECTOR
        [SerializeField, Space(10), OnValueChanged(nameof(OnAvailableStatesChanged))]
#else
        [SerializeField, Space(10), OnValueChanged(nameof(OnAvailableStatesChanged))]
#endif
        private List<State> states;
        public List<State> States => states;

#if ODIN_INSPECTOR
        [SerializeField, Space(10), OnValueChanged(nameof(OnDefaultActiveStatesChanged))]
#else
        [SerializeField, Space(10), OnValueChanged(nameof(OnDefaultActiveStatesChanged))]
#endif
        private MultiStateValue defaultActiveStates;
        public MultiStateValue DefaultActiveStates => defaultActiveStates;

#if ODIN_INSPECTOR
        [SerializeField, LabelText("Current Active States"), DisableInEditorMode,
         OnValueChanged(nameof(OnCurrentActiveStatesChanged)), Title("Runtime State")]
#else
        [UsedImplicitly] private bool IsEditor => Application.isPlaying == false;

        [SerializeField, Label("Current Active States"), DisableIf(nameof(IsEditor)),
         OnValueChanged(nameof(OnCurrentActiveStatesChanged)), Header("Runtime States")]
#endif
        private MultiStateValue currentActiveStates;
        public MultiStateValue CurrentActiveStates => currentActiveStates;

#if ODIN_INSPECTOR
        [SerializeField, ReadOnly]
#else
        [UsedImplicitly] private bool DisableEditorField => true;

        [SerializeField, DisableIf(nameof(DisableEditorField))]
#endif
        private MultiStateValue previousActiveStates;
        public MultiStateValue PreviousActiveStates => previousActiveStates;

#if ODIN_INSPECTOR
        [NonSerialized, ShowInInspector, ListDrawerSettings(IsReadOnly = true), Title("Runtime Listeners"), PropertyOrder(1)]
#else
        [NonSerialized]
#endif
        private readonly List<IMultiStateListener> _listeners = new List<IMultiStateListener>();

        private MultiStateValue _lastSentStates;

        protected override void OnBegin()
        {
            base.OnBegin();
            ResetStateListsToMatchDefault();
            previousActiveStates.SetAllInactive();
            OnStatesChanged();
        }

        protected override void OnEnd()
        {
            base.OnEnd();
            _listeners.Clear();
        }

        [UsedImplicitly]
        public void Set(IEnumerable<State> newStates) => ApplyChange(currentActiveStates.Set, states);

        [UsedImplicitly]
        public void SetStateAsActive(State state) => ApplyChange(currentActiveStates.SetActive, new[] { state });

        [UsedImplicitly]
        public void UnsetStateAsActive(State state) => ApplyChange(currentActiveStates.SetInactive, new[] { state });
        
        [UsedImplicitly]
        public void ToggleStateActive(State state)
        {
            Func<IEnumerable<State>, bool> method;
            if (currentActiveStates.IsActive(state))
            {
                method = currentActiveStates.SetInactive;
            }
            else
            {
                method = currentActiveStates.SetActive;
            }
            ApplyChange(method, new[] { state });
        }

        [UsedImplicitly]
        public void ToggleAllActive() => ApplyChange(currentActiveStates.Set,
            currentActiveStates.AreAllActive() ? new List<State>() : states.ToList());

        [UsedImplicitly]
        public void SetToPreviousActiveStates() => ApplyChange(currentActiveStates.Set, previousActiveStates.ActiveStates);

        [UsedImplicitly]
        public void Default() => ApplyChange(currentActiveStates.Set, defaultActiveStates.ActiveStates);

        public void SetAllActive()
        {
            currentActiveStates.SetAllActive();
        }

        public void SetAllInactive()
        {
            currentActiveStates.SetAllInactive();
        }

        public bool AreAllActive() => currentActiveStates.AreAllActive();
        public bool IsActive(State state) => currentActiveStates.IsActive(state);

        private void ApplyChange(Func<IEnumerable<State>, bool> action, IEnumerable<State> statesToApply)
        {
            var currentStates = new List<State>(currentActiveStates.ActiveStates);

            var changed = action?.Invoke(statesToApply);
            if (changed == false) return;

            previousActiveStates.Set(currentStates);
            _lastSentStates.Set(currentActiveStates);
            OnStatesChanged();
        }

        public void RegisterListener(IMultiStateListener listener)
        {
            if (listener == null) return;
            _listeners.Add(listener);
        }

        public void UnregisterListener(IMultiStateListener listener)
        {
            _listeners.Remove(listener);
        }

        private void OnStatesChanged()
        {
            for (var i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i].OnStateChanged(previousActiveStates, currentActiveStates);
            }
        }

        private void OnAvailableStatesChanged()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[MultiStateManagerBase] Changing available states during runtime is not explicitly supported! Weird behaviour may occur, setting the current active states to default to play it safe.");
            }
            var defaultStates = defaultActiveStates.ActiveStates.Intersect(states);
            defaultActiveStates = new MultiStateValue(states, defaultStates);
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
            previousActiveStates.Set(_lastSentStates);
            _lastSentStates.Set(currentActiveStates);
            OnStatesChanged();
        }

        private void OnValidate()
        {
            if (defaultActiveStates == null) defaultActiveStates = new MultiStateValue(states);
            if (currentActiveStates == null) currentActiveStates = new MultiStateValue(states, defaultActiveStates);
            if (previousActiveStates == null) previousActiveStates = new MultiStateValue(states, defaultActiveStates);
            if (_lastSentStates == null) _lastSentStates = new MultiStateValue(states, defaultActiveStates);

            if (Application.isPlaying) return;
            ResetStateListsToMatchDefault();
        }

        private void ResetStateListsToMatchDefault()
        {
            if (defaultActiveStates == null) defaultActiveStates = new MultiStateValue(states);
            currentActiveStates = new MultiStateValue(states, defaultActiveStates);
            previousActiveStates = new MultiStateValue(states, defaultActiveStates);
            _lastSentStates = new MultiStateValue(states, defaultActiveStates);
        }
    }
#endif
}
