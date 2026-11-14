using GameCore.CustomExtensions.ObjectExtensions;
using GameCore.Services.ServiceStructure;
using UnityEngine;

namespace GameCore.ScreenManagement.ScreenTransition.ScreenBlock
{
    public class ScreenBlocker : InstantMonoBehaviourService
    {
        [Header("References")]

        [SerializeField] private GameObject _screenBlockPrefab;

        private GameObject _screenBlocker;

        private bool _initialized;
        
        public override void InitializeService()
        {
            if (_initialized)
                return;
            
            InitializeBlocker();

            _initialized = true;
        }

        public void ChangeBlockScreenState(bool isActiveState)
            => ChangeBlockerState(isActiveState);

        private void InitializeBlocker()
        {
            _screenBlocker = CustomObjectExtensions.SmartInstantiate(_screenBlockPrefab);
            DontDestroyOnLoad(_screenBlocker);
            ChangeBlockerState(false);
        }

        private void ChangeBlockerState(bool enable)
            => _screenBlocker.SetActive(enable);
    }
}