using System;
using System.Collections.Generic;
using System.Linq;
using GI.UnityToolkit.State;
using UnityEngine;

#if !UNITY_2019
[Serializable]
public class MultiStateValue<TState> where TState : StateBase
{
    [HideInInspector, SerializeField] private List<TState> _activeStates = new List<TState>();
    [HideInInspector, SerializeField] private List<TState> _availableStates;
    
    public MultiStateValue(IEnumerable<TState> availableStates)
    {
        InitializeAvailableStates(availableStates);
    }

    public MultiStateValue(IEnumerable<TState> availableStates, IEnumerable<TState> initialValue)
    {
        InitializeAvailableStates(availableStates);
        Set(initialValue);
    }

    public MultiStateValue(IEnumerable<TState> availableStates, MultiStateValue<TState> initialValue)
    {
        InitializeAvailableStates(availableStates);
        Set(initialValue);
    }

    private void InitializeAvailableStates(IEnumerable<TState> availableStates)
    {
        _availableStates = new List<TState>();
        _availableStates.AddRange(availableStates.Where(s => s != null));
    }

    /// <summary>
    /// Checks whether a state is active.
    /// </summary>
    /// <param name="state">The state to check.</param>
    /// <returns>Whether the state is currently active.</returns>
    public bool IsActive(TState state)
    {
        return _activeStates.Contains(state);
    }

    /// <summary>
    /// Checks whether all states provided are active.
    /// </summary>
    /// <param name="states">The states to check.</param>
    /// <returns>Whether all states are currently active.</returns>
    public bool IsActive(IEnumerable<TState> states)
    {
        return states.All(IsActive);
    }

    /// <summary>
    /// Checks whether all available states are active.
    /// </summary>
    /// <returns>Whether all available states are currently active.</returns>
    public bool AreAllActive() => IsActive(_availableStates);

    /// <summary>
    /// Sets the given state as active. (Additive)
    /// </summary>
    /// <param name="state">The state to set as active.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetActive(TState state)
    {
        if (state == null)
        {
            Debug.LogError($"[MultiStateValue] Attempted to set null as active state!");
            return false;
        }
        
        if (_availableStates.Contains(state) == false)
        {
            Debug.LogError($"[MultiStateValue] Attempted to set active state \"{state.name}\" that is not in the available states list!");
            return false;
        }

        if (_activeStates.Contains(state)) return false;
        
        _activeStates.Add(state);
        _activeStates = _activeStates.OrderBy(s => _availableStates.IndexOf(s)).ToList();
        return true;
    }

    /// <summary>
    /// Sets the given states as active. (Additive)
    /// </summary>
    /// <param name="states">The states to set as active.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetActive(IEnumerable<TState> states)
    {
        return states.Aggregate(false, (current, state) => current || SetActive(state));
    }
    
    /// <summary>
    /// Sets this MultiStateValue's active states as active based on the active states of another MultiStateValue. (Additive)
    /// </summary>
    /// <param name="value">The MultiStateValue whose states should be set as inactive.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetActive(MultiStateValue<TState> value)
    {
        return value._activeStates.Aggregate(false, (current, state) => current || SetActive(state));
    }

    /// <summary>
    /// Unsets the given state as active. (Subtractive)
    /// </summary>
    /// <param name="state">The state to unset.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetInactive(TState state)
    {
        if (state == null)
        {
            Debug.LogError($"[MultiStateValue] Attempted to set null as inactive state!");
            return false;
        }

        if (_availableStates.Contains(state)) return _activeStates.Remove(state);
        
        Debug.LogError($"[MultiStateValue] Attempted to set inactive state \"{state.name}\" that is not in the available states list!");
        return false;
    }

    /// <summary>
    /// Unsets the given states as active. (Subtractive)
    /// </summary>
    /// <param name="states">The states to unset.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetInactive(IEnumerable<TState> states)
    {
        return states.Aggregate(false, (current, state) => current || SetInactive(state));
    }

    /// <summary>
    /// Sets this MultiStateValue's active states as inactive based on the active states of another MultiStateValue. (Subtractive)
    /// </summary>
    /// <param name="value">The MultiStateValue whose states should be set as inactive.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetInactive(MultiStateValue<TState> value)
    {
        return value._activeStates.Aggregate(false, (current, state) => current || SetInactive(state));
    }

    /// <summary>
    /// Sets this MultiStateValue's active states to exactly what is given. All other states are unset. (Hard set)
    /// </summary>
    /// <param name="states">The states that should be active.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool Set(IEnumerable<TState> states)
    {
        var statesList = states.ToList();
        if (_activeStates.SequenceEqual(statesList)) return false;
        
        _activeStates.Clear();
        _activeStates.AddRange(statesList);
        _activeStates = _activeStates.OrderBy(s => _availableStates.IndexOf(s)).ToList();
        return true;
    }

    /// <summary>
    /// Sets this MultiStateValue's active states to exactly what is given. All other states are unset. (Hard set)
    /// </summary>
    /// <param name="value">The MultiStateValue whose states should be set active.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool Set(MultiStateValue<TState> value)
    {
        return Set(value._activeStates);
    }

    public bool Toggle(TState state)
    {
        if (state == null)
        {
            Debug.LogError($"[MultiStateValue] Attempted to toggle a null state!");
            return false;
        }

        if (_availableStates.Contains(state) == false)
        {
            Debug.LogError($"[MultiStateValue] Attempted to toggle state \"{state.name}\" that is not in the available states list!");
            return false;
        }

        return _activeStates.Contains(state) ? SetInactive(state) : SetActive(state);
    }

    /// <summary>
    /// Sets all available states as active.
    /// </summary>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetAllActive()
    {
        if (_availableStates.Except(_activeStates).Any() == false) return false;
        
        _activeStates.Clear();
        _activeStates.AddRange(_availableStates);
        _activeStates = _activeStates.OrderBy(s => _availableStates.IndexOf(s)).ToList();
        return true;
    }

    /// <summary>
    /// Unsets all available states.
    /// </summary>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetAllInactive()
    {
        if (_activeStates.Count == 0) return false;
        
        _activeStates.Clear();
        return true;
    }

    /// <summary>
    /// Returns all currently active states.
    /// </summary>
    public List<TState> ActiveStates => _activeStates;

    /// <summary>
    /// Returns all states available to this MultiStateValue.
    /// </summary>
    public List<TState> AvailableStates => _availableStates;

    public override string ToString()
    {
        return string.Join(", ", ActiveStates.Select(s => s.name));
    }
}
#else
[Serializable]
public class MultiStateValue
{
    [HideInInspector, SerializeField] private List<State> _activeStates = new List<State>();
    [HideInInspector, SerializeField] private List<State> _availableStates;
    
    public MultiStateValue(IEnumerable<State> availableStates)
    {
        InitializeAvailableStates(availableStates);
    }

    public MultiStateValue(IEnumerable<State> availableStates, IEnumerable<State> initialValue)
    {
        InitializeAvailableStates(availableStates);
        Set(initialValue);
    }

    public MultiStateValue(IEnumerable<State> availableStates, MultiStateValue initialValue)
    {
        InitializeAvailableStates(availableStates);
        Set(initialValue);
    }

    private void InitializeAvailableStates(IEnumerable<State> availableStates)
    {
        _availableStates = new List<State>();
        _availableStates.AddRange(availableStates.Where(s => s != null));
    }

    /// <summary>
    /// Checks whether a state is active.
    /// </summary>
    /// <param name="state">The state to check.</param>
    /// <returns>Whether the state is currently active.</returns>
    public bool IsActive(State state)
    {
        return _activeStates.Contains(state);
    }

    /// <summary>
    /// Checks whether all states provided are active.
    /// </summary>
    /// <param name="states">The states to check.</param>
    /// <returns>Whether all states are currently active.</returns>
    public bool IsActive(IEnumerable<State> states)
    {
        return states.All(IsActive);
    }

    /// <summary>
    /// Checks whether all available states are active.
    /// </summary>
    /// <returns>Whether all available states are currently active.</returns>
    public bool AreAllActive() => IsActive(_availableStates);

    /// <summary>
    /// Sets the given state as active. (Additive)
    /// </summary>
    /// <param name="state">The state to set as active.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetActive(State state)
    {
        if (state == null)
        {
            Debug.LogError($"[MultiStateValue] Attempted to set null as active state!");
            return false;
        }
        
        if (_availableStates.Contains(state) == false)
        {
            Debug.LogError($"[MultiStateValue] Attempted to set active state \"{state.name}\" that is not in the available states list!");
            return false;
        }

        if (_activeStates.Contains(state)) return false;
        
        _activeStates.Add(state);
        _activeStates = _activeStates.OrderBy(s => _availableStates.IndexOf(s)).ToList();
        return true;
    }

    /// <summary>
    /// Sets the given states as active. (Additive)
    /// </summary>
    /// <param name="states">The states to set as active.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetActive(IEnumerable<State> states)
    {
        return states.Aggregate(false, (current, state) => current || SetActive(state));
    }
    
    /// <summary>
    /// Sets this MultiStateValue's active states as active based on the active states of another MultiStateValue. (Additive)
    /// </summary>
    /// <param name="value">The MultiStateValue whose states should be set as inactive.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetActive(MultiStateValue value)
    {
        return value._activeStates.Aggregate(false, (current, state) => current || SetActive(state));
    }

    /// <summary>
    /// Unsets the given state as active. (Subtractive)
    /// </summary>
    /// <param name="state">The state to unset.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetInactive(State state)
    {
        if (state == null)
        {
            Debug.LogError($"[MultiStateValue] Attempted to set null as inactive state!");
            return false;
        }

        if (_availableStates.Contains(state)) return _activeStates.Remove(state);
        
        Debug.LogError($"[MultiStateValue] Attempted to set inactive state \"{state.name}\" that is not in the available states list!");
        return false;
    }

    /// <summary>
    /// Unsets the given states as active. (Subtractive)
    /// </summary>
    /// <param name="states">The states to unset.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetInactive(IEnumerable<State> states)
    {
        return states.Aggregate(false, (current, state) => current || SetInactive(state));
    }

    /// <summary>
    /// Sets this MultiStateValue's active states as inactive based on the active states of another MultiStateValue. (Subtractive)
    /// </summary>
    /// <param name="value">The MultiStateValue whose states should be set as inactive.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetInactive(MultiStateValue value)
    {
        return value._activeStates.Aggregate(false, (current, state) => current || SetInactive(state));
    }

    /// <summary>
    /// Sets this MultiStateValue's active states to exactly what is given. All other states are unset. (Hard set)
    /// </summary>
    /// <param name="states">The states that should be active.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool Set(IEnumerable<State> states)
    {
        if (states == null) return false;
        var statesList = states.ToList();
        if (_activeStates.SequenceEqual(statesList)) return false;
        
        _activeStates.Clear();
        _activeStates.AddRange(statesList);
        _activeStates = _activeStates.OrderBy(s => _availableStates.IndexOf(s)).ToList();
        return true;
    }

    /// <summary>
    /// Sets this MultiStateValue's active states to exactly what is given. All other states are unset. (Hard set)
    /// </summary>
    /// <param name="value">The MultiStateValue whose states should be set active.</param>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool Set(MultiStateValue value)
    {
        return Set(value._activeStates);
    }

    public bool Toggle(State state)
    {
        if (state == null)
        {
            Debug.LogError($"[MultiStateValue] Attempted to toggle a null state!");
            return false;
        }

        if (_availableStates.Contains(state) == false)
        {
            Debug.LogError($"[MultiStateValue] Attempted to toggle state \"{state.name}\" that is not in the available states list!");
            return false;
        }

        return _activeStates.Contains(state) ? SetInactive(state) : SetActive(state);
    }

    /// <summary>
    /// Sets all available states as active.
    /// </summary>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetAllActive()
    {
        if (_availableStates.Except(_activeStates).Any() == false) return false;
        
        _activeStates.Clear();
        _activeStates.AddRange(_availableStates);
        _activeStates = _activeStates.OrderBy(s => _availableStates.IndexOf(s)).ToList();
        return true;
    }

    /// <summary>
    /// Unsets all available states.
    /// </summary>
    /// <returns>Whether the current active states were actually affected by this operation.</returns>
    public bool SetAllInactive()
    {
        if (_activeStates.Count == 0) return false;
        
        _activeStates.Clear();
        return true;
    }

    /// <summary>
    /// Returns all currently active states.
    /// </summary>
    public List<State> ActiveStates => _activeStates;

    /// <summary>
    /// Returns all states available to this MultiStateValue.
    /// </summary>
    public List<State> AvailableStates => _availableStates;

    public override string ToString()
    {
        return string.Join(", ", ActiveStates.Select(s => s.name));
    }
}
#endif