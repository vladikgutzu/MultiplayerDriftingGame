using UnityEngine;
using UnityEngine.EventSystems;

public class AutoScroll : MonoBehaviour, ISelectHandler
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private ScrollController scrollController;

    private void Start()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (scrollController==null)
        {
            scrollController = FindAnyObjectByType<ScrollController>();
        }
    }

    public void SetScroll()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        if (scrollController == null)
            scrollController = FindAnyObjectByType<ScrollController>();
        if (scrollController != null && rectTransform != null)
            scrollController.UpdateScroll(rectTransform);
    }

    public void OnSelect(BaseEventData eventData)
    {
        SetScroll();
    }
}
