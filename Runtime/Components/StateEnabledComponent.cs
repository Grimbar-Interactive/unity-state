using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using GI.UnityToolkit.Attributes;
#endif

namespace GI.UnityToolkit.State.Components
{
#if !UNITY_2019
    public abstract class StateEnabledComponent<TState> : StateEnabledComponentBase<TState>, IStateListener<TState> where TState : StateBase
    {
#if ODIN_INSPECTOR
        [SerializeField, PropertyOrder(-3)]
#else
        [SerializeField]
#endif
        private StateManagerBase<TState> stateManager;
        
#if ODIN_INSPECTOR
        [SerializeField, ShowIf("@this.StateManagerIsSet && this.delayDisable"), ValueDropdown(nameof(StateOptions)), Space(4)]
#else
        [SerializeField, ShowIf(EConditionOperator.And,nameof(StateManagerIsSet), nameof(delayDisable)), Space(4), BoxGroup("Delay")]
#endif
        protected List<TState> delayedStates;
        
        private enum StateComparison
        {
            AnyAreActive = 0,
            NoneAreActive = 1
        }

#if ODIN_INSPECTOR
        [SerializeField, PropertyOrder(-1)]
#else
        [SerializeField, BoxGroup("State")]
#endif
        private StateComparison enableWhen = StateComparison.AnyAreActive;
        
        protected void Awake()
        {
            stateManager.RegisterListener(this);
        }

        protected void OnEnable()
        {
            OnStateChanged(stateManager.PreviousState, stateManager.CurrentState);
        }

        protected void OnDestroy()
        {
            stateManager.UnregisterListener(this);
        }
        
        public void OnStateChanged(TState previousState, TState newState)
        {
            var enabled = enableWhen switch
            {
                StateComparison.AnyAreActive => activeStates.Any(s => s == newState),
                StateComparison.NoneAreActive => activeStates.All(s => s != newState),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (enabled)
            {
                HandleEnable();
            }
            else
            {
                HandleDisable(delayDisable && delayDuration > 0 && delayedStates.Contains(newState));
            }
        }
        
#if ODIN_INSPECTOR
        protected override List<TState> StateOptions => stateManager != null ? stateManager.States : new List<TState>();
#endif
        protected override bool StateManagerIsSet => stateManager != null;
    }
#endif
}