#if !UNITY_2019
using System.Collections;
using System.Collections.Generic;
using GI.UnityToolkit.Utilities;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using GI.UnityToolkit.Attributes;
#endif
#endif

namespace GI.UnityToolkit.State.Components
{
#if !UNITY_2019
    public abstract class StateEnabledComponentBase<TState> : MonoBehaviour where TState : StateBase
    {
        protected abstract bool StateManagerIsSet { get; }
        
#if ODIN_INSPECTOR
        protected abstract List<TState> StateOptions { get; }

        [SerializeField, ShowIf(nameof(StateManagerIsSet)), ValueDropdown(nameof(StateOptions)), Space(4), PropertyOrder(-2)]
#else
        [SerializeField, ShowIf(nameof(StateManagerIsSet)), Space(4), BoxGroup("State")]
#endif
        protected List<TState> activeStates;
        
#if ODIN_INSPECTOR
        [SerializeField, ShowIf(nameof(StateManagerIsSet)), Space(4)]
#else
        [SerializeField, ShowIf(nameof(StateManagerIsSet)), Space(4), BoxGroup("Delay")]
#endif    
        protected bool delayDisable = false;
        
#if ODIN_INSPECTOR
        [SerializeField, ShowIf("@this.StateManagerIsSet && this.delayDisable"), Space(4)]
#else
        [SerializeField, ShowIf(EConditionOperator.And,nameof(StateManagerIsSet), nameof(delayDisable)), Space(4), BoxGroup("Delay")]
#endif
        protected float delayDuration = 1f;

        private Coroutine _delayRoutine;

        protected void HandleEnable()
        {
            if (_delayRoutine != null)
            {
                StopCoroutine(_delayRoutine);
                _delayRoutine = null;
            }
            SetEnabled(true);
        }
        
        protected void HandleDisable(bool delayed)
        {
            // If we're in an inactive state but this is already disabled, there's nothing else to do.
            if (IsEnabled == false) return;

            // If we're in an inactive state but there's no disable delay, or we're not in a delayed inactive state,
            // just disable the Canvas immediately.
            if (delayed == false)
            {
                if (_delayRoutine != null)
                {
                    StopCoroutine(_delayRoutine);
                    _delayRoutine = null;
                }
                SetEnabled(false);
                return;
            }

            _delayRoutine ??= StartCoroutine(WaitBeforeDisable(delayDuration));
            return;

            IEnumerator WaitBeforeDisable(float duration)
            {
                yield return Wait.Time(duration);
                SetEnabled(false);
                _delayRoutine = null;
            }
        }
        
        protected abstract void SetEnabled(bool enable);
        protected abstract bool IsEnabled { get; }
    }
#endif
}
