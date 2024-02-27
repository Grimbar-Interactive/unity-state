namespace GI.UnityToolkit.State
{
#if !UNITY_2019
    public interface IMultiStateListener<TState> where TState : StateBase
    {
        void OnStateChanged(MultiStateValue<TState> previousActiveStates, MultiStateValue<TState> newActiveStates);
    }
#else
    public interface IMultiStateListener
    {
        void OnStateChanged(MultiStateValue previousActiveStates, MultiStateValue newActiveStates);
    }
#endif
}
