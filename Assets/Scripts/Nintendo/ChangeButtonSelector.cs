using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;




public enum AnimationDirection
{
    Right,Left,None
}

public class ChangeButtonSelector : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public GameObject specialObject;


    [Header("Color")]
    public Color selectedColor;
    public Color defaultColor;
    
    [Header("Objects to Change")]
    public TextMeshProUGUI[] text;
    public GameObject[] gameObjectsToChange;
    public Sprite defaultSprite;
    public Sprite selectedSprite;
    public Image imageToChange;
    
    [Header("Select Animation")]
    private Vector2 originalPosition;
    

    [Header("Animation Direction")]
    [SerializeField] private AnimationDirection _animationDirection;
    
   

    private void Awake()
    {
        originalPosition = GetComponent<RectTransform>().anchoredPosition;
    }

    public void OnSelect(BaseEventData eventData)
    {
       ActivateSpecialObject(true);
       ChangeColors(selectedColor);
       SelectAnimation(true);
       ChangeImage(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ActivateSpecialObject(false);
        ChangeColors(defaultColor);
        SelectAnimation(false);
        ChangeImage(false);

       
    }


    public void OnDisable()
    {
        ActivateSpecialObject(false);
        ChangeColors(defaultColor);
        var rect = GetComponent<RectTransform>();
        if (rect != null) { rect.DOKill(); rect.anchoredPosition = originalPosition; }
        ChangeImage(false);

    }
    
    
    private void ActivateSpecialObject(bool activate)
    {
        if (specialObject != null)
        {
            specialObject.SetActive(activate);
        }
    }

    private void ChangeColors(Color color)
    {
        foreach (GameObject obj in gameObjectsToChange)
        {
            Image image = obj.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
        }

        foreach (TextMeshProUGUI txt in text)
        {
            if (txt != null)
            {
                txt.color = color;
            }
        }
    }



    public void SelectAnimation(bool select)
    {
        var rect = GetComponent<RectTransform>();
        if (rect == null || !isActiveAndEnabled) return;
        rect.DOKill();
        
        if (select)
        {
            switch (_animationDirection)
            {
                case AnimationDirection.Right:
                GetComponent<RectTransform>().DOAnchorPos(new Vector2(originalPosition.x+50,originalPosition.y),0.4f,false);
                    break;
                case AnimationDirection.Left:
                    GetComponent<RectTransform>().DOAnchorPos(new Vector2(originalPosition.x-50,originalPosition.y),0.4f,false);
                    break;
                case AnimationDirection.None :
                    break;
            }
            
            
        }
        else
        {
            if (_animationDirection!=AnimationDirection.None)
            {
                GetComponent<RectTransform>().DOAnchorPos(new Vector2(originalPosition.x,originalPosition.y),0.4f,false);

            }

            /*GetComponent<RectTransform>().anchoredPosition = originalPosition;*/
        }


    }


    public void ChangeImage(bool select)
    {
        if (imageToChange!=null)
        {
            imageToChange.sprite = select ? selectedSprite : defaultSprite;
        }
    }
}
