using Cysharp.Threading.Tasks;
using UnityEngine;

namespace App.AppStatesInfra
{
    public abstract class AppState : MonoBehaviour
    {
        public abstract UniTask Enter();
        public abstract UniTask Exit();
    }
}
