namespace GI.UnityToolkit.State
{
    public interface IStateListener<in TState> where TState : StateBase
    {
        void OnStateChanged(TState state);
    }
}