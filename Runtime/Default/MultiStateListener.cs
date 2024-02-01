using UnityEngine;

#if UNITY_2019
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.Serialization;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
#endif

namespace GI.UnityToolkit.State
{
    [AddComponentMenu("Grimbar Interactive/State/Multi-State Listener")]
#if !UNITY_2019
    public class MultiStateListener : MultiStateListenerBase<State, MultiStateManager> {}
#else
    public class MultiStateListener : MonoBehaviour, IMultiStateListener
    {
#if ODIN_INSPECTOR
        [Title("Settings")]
#else
        [Header("Settings")]
#endif
        [SerializeField] private MultiStateManager manager = null;
        
#if ODIN_INSPECTOR
        private bool StateManagerIsNull => manager == null;
        private List<State> StateOptions => manager != null ? manager.States : new List<State>();

        [FormerlySerializedAs("activeStates")] [SerializeField, HideIf(nameof(StateManagerIsNull)), ValueDropdown(nameof(StateOptions)), Space(4)]
#else
        [FormerlySerializedAs("activeStates")] [SerializeField, Space(4)]
#endif
        private List<State> statesListenedFor = null;
        
        private enum StateComparison
        {
            AnyAreActive = 0,
            AllAreActive = 1,
            NoneAreActive = 2
        }

        [SerializeField] private StateComparison activeWhen = StateComparison.AnyAreActive;
        
#if ODIN_INSPECTOR
        [Title("Events"), HideIfGroup("Events", Condition = nameof(StateManagerIsNull))]
#else
        [Header("Events")]
#endif
        [SerializeField]
        private UnityEvent activeResponse = null;
#if ODIN_INSPECTOR
        [HideIfGroup("Events", Condition = nameof(StateManagerIsNull))]
#endif
        [SerializeField]
        private UnityEvent inactiveResponse = null;

        private void OnEnable()
        {
            if (!manager) return;
            manager.RegisterListener(this);

            if (IsActiveBasedOnConditions(manager.CurrentActiveStates))
            {
                activeResponse?.Invoke();
            }
            else
            {
                inactiveResponse?.Invoke();
            }
        }

        private void OnDisable()
        {
            if (!manager) return;
            manager.UnregisterListener(this);
        }

        public void OnStateChanged(MultiStateValue previousActiveStates, MultiStateValue newActiveStates)
        {
            var wasActive = IsActiveBasedOnConditions(previousActiveStates);
            var shouldBeActive = IsActiveBasedOnConditions(newActiveStates);

            switch (wasActive)
            {
                case false when shouldBeActive:
                    activeResponse?.Invoke();
                    break;
                case true when !shouldBeActive:
                    inactiveResponse?.Invoke();
                    break;
            }
        }
        
        private bool IsActiveBasedOnConditions(MultiStateValue value)
        {
            switch (activeWhen)
            {
                case StateComparison.AnyAreActive:
                    return statesListenedFor.Any(value.IsActive);
                case StateComparison.AllAreActive:
                    return statesListenedFor.All(value.IsActive);
                case StateComparison.NoneAreActive:
                    return !statesListenedFor.Any(value.IsActive);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
#endif
}