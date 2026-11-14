using System;
using DG.Tweening;
using UnityEngine;

namespace Script.Menu
{
    public class ChangeTarget : MonoBehaviour
    {
        public static ChangeTarget _instance;

        public BD_ShowroomCamera _showroomCamera;
        public Transform _minimapTarget;
        
        public GameObject _worldMapCanvasWS;
        public GameObject _worldMapCanvas;
        public GameObject _mainMenuCanvas;
        public GameObject _carShopCanvas;
        
        public bool _isMinimap;
        public bool _isCarShop;

        [Header("Showroom camera poses")]
        public float mainMenuOrbitX = 120f;
        public float mainMenuOrbitY = 8f;
        public float mainMenuDistance = 7f;
        public float carShopOrbitX = 200f;
        public float carShopOrbitY = 10f;
        public float carShopDistance = 6.5f;

        private float _delay;
        private Tween mapReveal;

        private void Awake()
        {
            _instance = this;
        }
        
        public void ChangeCarShopState()
        {
            _isCarShop = !_isCarShop;
        }

        public void ChangeMinimapState()
        {
            _isMinimap = !_isMinimap;
        }
        
        public void AssignMinimapTargetToCamera()
        {
            mapReveal?.Kill();
            _showroomCamera.dragSpeed = _isMinimap ? 0 : 1;
            _worldMapCanvasWS.SetActive(_isMinimap);
            
            if(_isMinimap)
            {
                mapReveal = DOVirtual.DelayedCall(1f, () => {
                    if (_worldMapCanvas != null && _isMinimap) _worldMapCanvas.SetActive(true);
                });
            }
            else
            {
                _worldMapCanvas.SetActive(false);
            }
            
            _showroomCamera.target = _isMinimap ? _minimapTarget : BD_MainMenuManager.Instance._mainMenuTarget;
            _mainMenuCanvas.SetActive(!_isMinimap);
            if (Camera.main != null) Camera.main.nearClipPlane = _isMinimap ? 2.5f : .3f;

        }
        
        public void AssignCarShopTargetToCamera()
        {
            // _showroomCamera.dragSpeed = _isCarShop ? 0 : 1; 
            
            // SetCarShopTargetPosition();

            var menu = BD_MainMenuManager.Instance;
            if (menu == null || _showroomCamera == null) return;
            _showroomCamera.target = _isCarShop ? menu._carShopTarget : menu._mainMenuTarget;
            ApplyShowroomPose(_isCarShop);
            if (Camera.main != null)
                Camera.main.nearClipPlane = _isCarShop ? 3.3f : .3f;
        }

        public void SetCarShopState(bool active)
        {
            mapReveal?.Kill();
            _isCarShop = active;
            _isMinimap = false;
            if (_worldMapCanvas != null) _worldMapCanvas.SetActive(false);
            if (_worldMapCanvasWS != null) _worldMapCanvasWS.SetActive(false);
            AssignCarShopTargetToCamera();
        }

        /// <summary>
        /// Restores the presentation used by the screen-space menu flow.  The
        /// original Nintendo menu drove the 3D map and the menu canvas through
        /// separate persistent button events.  The current event browser lives
        /// inside the main canvas, so leaving the old world-map state enabled
        /// hides the very panel that was just opened.
        /// </summary>
        public void SetOverlayMenuState(bool carShop)
        {
            mapReveal?.Kill();
            _isMinimap = false;
            _isCarShop = carShop;

            if (_worldMapCanvas != null)
                _worldMapCanvas.SetActive(false);
            if (_worldMapCanvasWS != null)
                _worldMapCanvasWS.SetActive(false);
            if (_mainMenuCanvas != null)
                _mainMenuCanvas.SetActive(true);

            if (_showroomCamera != null)
            {
                _showroomCamera.dragSpeed = 1f;
                var menu = BD_MainMenuManager.Instance;
                if (menu != null)
                    _showroomCamera.target = carShop && menu._carShopTarget != null
                        ? menu._carShopTarget
                        : menu._mainMenuTarget;
                ApplyShowroomPose(carShop);
            }

            if (Camera.main != null)
                Camera.main.nearClipPlane = carShop ? 3.3f : .3f;
        }

        private void OnDestroy() { mapReveal?.Kill(); }

        public void SetCarShopTargetPosition()
        {
           //BD_MainMenuManager.Instance.SpawnShopCar(BD_MainMenuManager.Instance.selectedPlayerCarIndex);
           BD_MainMenuManager.Instance.TestShow(BD_MainMenuManager.Instance.selectedPlayerCarIndex);
        }

        private void ApplyShowroomPose(bool carShop)
        {
            if (_showroomCamera == null)
                return;

            _showroomCamera.orbitX = carShop ? carShopOrbitX : mainMenuOrbitX;
            _showroomCamera.orbitY = carShop ? carShopOrbitY : mainMenuOrbitY;
            _showroomCamera.distance = carShop ? carShopDistance : mainMenuDistance;
        }
    }
}
