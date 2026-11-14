using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Race.RacesControl
{
    public abstract class RaceTrackVisualElement : MonoBehaviour
    {
        public abstract UniTask Initialize();
        public abstract UniTask Animate();
    }
}