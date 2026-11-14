using System;
using DG.Tweening;
using GameCore.CustomExtensions.CustomEditorExtensions;
using UnityEngine;

namespace GameCore.CustomExtensions.Presets.AnimationsPresets.Button
{
    public class SmoothAnimButtonClick : AnimationPreset
    {
        [SerializeField] private UnityEngine.UI.Button _targetButton; 
        [SerializeField] private RectTransform _targetRect;

        [Header("Values")]

        [SerializeField] private bool _blockInteractionOnAnimation = true;
        [SerializeField] private bool _animationUnscaleTime = true;

        private Vector3 defaultLocalScale;

        public UnityEngine.UI.Button TargetButton => _targetButton;

        private void Awake()
        {
            defaultLocalScale = _targetRect.localScale;
        }
        
        public override void Animate(Action completeCallback = null, Action completeMainPartCallback = null, bool reset = true)
        {
            if (reset)
            {
                ToDefaultState();
            }
            else
            {
                KillTweenAnims();
            }
            
            ChangeButtonInteraction(false);

            const float loopTime = 0.15f;
            const Ease playInEase = Ease.InSine;
            const Ease playOutEase = Ease.OutSine;

            Vector3 endValue = defaultLocalScale - Vector3.one * 0.2f;

            _targetRect.DOScale(endValue, loopTime)
                .SetEase(playInEase)
                .SetUpdate(isIndependentUpdate: _animationUnscaleTime)
                .OnComplete(() =>
            {
                completeMainPartCallback?.Invoke();
                _targetRect.DOScale(defaultLocalScale, loopTime).SetEase(playOutEase).OnComplete(() =>
                {
                    ChangeButtonInteraction(true);
                    completeCallback?.Invoke();
                });
            });
        }

        private void KillTweenAnims()
        {
            _targetRect.DOKill();
        }

        private void ChangeButtonInteraction(bool active)
        {
            if (!_blockInteractionOnAnimation)
                return;

            _targetButton.interactable = active;
        }

        private void ToDefaultState()
        {
            KillTweenAnims();

            _targetRect.localScale = defaultLocalScale;

            _targetButton.interactable = true;
        }

        private void OnDisable()
        {
            ToDefaultState();
        }

#if UNITY_EDITOR

        private void Reset()
        {
            _targetButton = GetComponentInChildren<UnityEngine.UI.Button>();
            _targetRect = GetComponent<RectTransform>();
            
            EditorExtensions.MarkObjectDirty(this);
            EditorExtensions.SaveAndRefreshAssets();
        }

#endif
    }
}