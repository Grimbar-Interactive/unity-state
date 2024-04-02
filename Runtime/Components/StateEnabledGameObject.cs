namespace GI.UnityToolkit.State.Components
{
    public class StateEnabledGameObject : StateEnabledComponent<State>
    {
        protected override void SetEnabled(bool enable) => gameObject.SetActive(enable);
        protected override bool IsEnabled => gameObject.activeSelf;
    }
}