using GI.UnityToolkit.Variables;

namespace GI.UnityToolkit.State
{
    [System.Serializable]
    public abstract class StateBase : DataObject
    {
        public override string ToString() => name;
    }
}