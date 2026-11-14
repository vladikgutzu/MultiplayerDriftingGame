using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarShopFade : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public Animator _fadeGO;

    private void OnEnable()
    {
        if (_fadeGO != null)
            _fadeGO.SetTrigger("StartFade");
        if (canvasGroup != null)
            StartCoroutine(Fade());
    }

    private IEnumerator Fade() {

        canvasGroup.alpha = 0f;

        float timer = 3f;

        while (timer > 0) {

            timer -= Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, Time.unscaledDeltaTime * .4f);
            yield return null;

        }

        canvasGroup.alpha = 1f;

    }

    private void OnDisable() {
        
        StopAllCoroutines();
        if (_fadeGO != null)
            _fadeGO.ResetTrigger("StartFade");
    }
}
