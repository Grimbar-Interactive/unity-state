using UnityEngine;
using GI.UnityToolkit.Variables;

namespace GI.UnityToolkit.State
{
    [CreateAssetMenu(menuName = "Data Objects/State/State")]
    [System.Serializable]
    public class State : DataObject
    {
        public string Name => name;
    }
}