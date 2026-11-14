using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollController : MonoBehaviour
{
    [SerializeField]private ScrollRect _rect;
    private void Start()
    {
        _rect = GetComponent<ScrollRect>();
    }

    public void UpdateScroll(RectTransform itemRect)
    {
        //Debug.LogError(_rect.gameObject.name);
        ScrollViewFocus.FocusOnItem(_rect, itemRect);
    }
}
