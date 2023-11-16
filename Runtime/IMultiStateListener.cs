namespace GI.UnityToolkit.State
{
    public interface IMultiStateListener<TState> where TState : StateBase
    {
        void OnStateChanged(MultiStateValue<TState> previousActiveStates, MultiStateValue<TState> newActiveStates);
    }
}
