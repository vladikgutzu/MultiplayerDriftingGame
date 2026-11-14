using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ScrollViewFocus 
{
	public static Vector2 FocusedScrollPosition(this ScrollRect scrollView, Vector2 focusPoint)
	{
		Vector2 contentSize = scrollView.content.rect.size;
		Vector2 viewportSize = ((RectTransform)scrollView.content.parent).rect.size;
		Vector2 contentScale = scrollView.content.localScale;

		contentSize.Scale(contentScale);
		focusPoint.Scale(contentScale);

		Vector2 scrollPosition = scrollView.normalizedPosition;
		if (scrollView.horizontal && contentSize.x > viewportSize.x)
			scrollPosition.x = Mathf.Clamp01((focusPoint.x - viewportSize.x * 0.5f) / (contentSize.x - viewportSize.x));
		if (scrollView.vertical && contentSize.y > viewportSize.y)
			scrollPosition.y = Mathf.Clamp01((focusPoint.y - viewportSize.y * 0.5f) / (contentSize.y - viewportSize.y));

		return scrollPosition;
	}
	public static Vector2 CalculateFocusedScrollPosition(ScrollRect scrollView, RectTransform item)
	{

		Vector2 itemCenterPoint = scrollView.content.
			InverseTransformPoint(item.transform.TransformPoint(item.rect.center));

		Vector2 contentSizeOffset = scrollView.content.rect.size;
		contentSizeOffset.Scale(scrollView.content.pivot);

		return scrollView.FocusedScrollPosition(itemCenterPoint + contentSizeOffset);
	}
	public static void FocusOnItem(ScrollRect scrollView, RectTransform item)
	{
		scrollView.normalizedPosition = CalculateFocusedScrollPosition(scrollView,item);
	}
}
