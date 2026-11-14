using GameCore.CustomExtensions.CustomEditorExtensions;
using NaughtyAttributes;
using UnityEngine;

namespace RaceCar.CarControl
{
    public class RaceCar : MonoBehaviour
    {
        [SerializeField] private BD_PlayerManager _playerManager;

        public BD_PlayerManager PlayerManager => _playerManager;

#if UNITY_EDITOR

        [Button("Load car refs")]
        private void LoadReferences()
        {
            _playerManager = GetComponent<BD_PlayerManager>();
            
            EditorExtensions.MarkObjectDirty(this);
            EditorExtensions.SaveAndRefreshAssets();
        }
        
#endif
    }
}