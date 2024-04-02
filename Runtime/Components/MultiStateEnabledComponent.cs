#if !UNITY_2019
using System;
using System.Linq;
using UnityEngine;
#endif

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using System.Collections.Generic;
#else
using GI.UnityToolkit.Attributes;
#endif

namespace GI.UnityToolkit.State.Components
{
#if !UNITY_2019
    public abstract class MultiStateEnabledComponent<TState> : StateEnabledComponentBase<TState>, IMultiStateListener<TState>
        where TState : StateBase
    {
#if ODIN_INSPECTOR
        [SerializeField, PropertyOrder(-3)]
#else
        [SerializeField]
#endif
        private MultiStateManagerBase<TState> multiStateManager;
        
        private enum StateComparison
        {
            AnyAreActive = 0,
            AllAreActive = 1,
            NoneAreActive = 2,
            SomeAreActive = 3
        }

#if ODIN_INSPECTOR
        [SerializeField, PropertyOrder(-1)]
#else
        [SerializeField, BoxGroup("State")]
#endif
        private StateComparison enableWhen = StateComparison.AnyAreActive;
        
        protected void Awake()
        {
            multiStateManager.RegisterListener(this);
        }

        protected void OnEnable()
        {
            OnStateChanged(multiStateManager.PreviousActiveStates, multiStateManager.CurrentActiveStates);
        }

        protected void OnDestroy()
        {
            multiStateManager.UnregisterListener(this);
        }

        public void OnStateChanged(MultiStateValue<TState> previousActiveStates, MultiStateValue<TState> newActiveStates)
        {
            var enabled = enableWhen switch
            {
                StateComparison.AnyAreActive => activeStates.Any(newActiveStates.IsActive),
                StateComparison.AllAreActive => activeStates.All(newActiveStates.IsActive),
                StateComparison.NoneAreActive => !activeStates.Any(newActiveStates.IsActive),
                StateComparison.SomeAreActive => activeStates.Any(newActiveStates.IsActive) && !activeStates.All(newActiveStates.IsActive),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            if (enabled)
            {
                HandleEnable();
            }
            else
            {
                HandleDisable(delayDisable && delayDuration > 0);
            }
        }
        
#if ODIN_INSPECTOR
        protected override List<TState> StateOptions => multiStateManager != null ? multiStateManager.States : new List<TState>();
#endif
        protected override bool StateManagerIsSet => multiStateManager != null;
    }
#endif
}