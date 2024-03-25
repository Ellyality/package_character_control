using System.Linq;
using UnityEngine;

namespace Ellyality.Runtime.Demo
{
    public class SpecialInput : MonoBehaviour
    {
        public void OnPoleInteract()
        {
            GameObject.FindObjectsOfType<SpecialPole>().ToList().ForEach(x => { x.OnPoleInteract(); });
        }
    }
}
