using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ColorPickerType
{
    BodyColor,
    NeonColor,
    WheelSmokeColor,
    RimColor,
    TintColor
}

public class SVImageControl : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    [SerializeField] private Image pickerImage;
    
    
    private RawImage SVImage;

    public ColorPickerType _ColorPickerType;
    
    public ColorPickerControl CC;
    public NeonColorPickerControl NCPC;
    public WheelSmokeColorPickerControl WSCPC;
    public RimColorPicker RCP;
    public TintColorPicker TCP;
    // public WheelSmokeColorPickerControl WSCPC;
    
    

    private RectTransform _rectTransform, _pickerTransform;

    private void Awake()
    {
        SVImage = GetComponent<RawImage>();
        
        _rectTransform = GetComponent<RectTransform>();

        if (!pickerImage || !_rectTransform)
        {
            Debug.LogWarning($"Color picker '{name}' is missing its picker image or RectTransform.", this);
            enabled = false;
            return;
        }

        _pickerTransform = pickerImage.rectTransform;
        _pickerTransform.position =
            new Vector2(-(_rectTransform.sizeDelta.x * 0.5f), -(_rectTransform.sizeDelta.y * 0.5f));
    }


   


    private void UpdateColor(PointerEventData eventData)
    {
        if (eventData == null || !_rectTransform || !_pickerTransform ||
            Mathf.Approximately(_rectTransform.rect.width, 0f) || Mathf.Approximately(_rectTransform.rect.height, 0f))
            return;

        Vector2 pointerPosition;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform,
            eventData.position, eventData.pressEventCamera, out pointerPosition)) return;
        Vector3 pos = pointerPosition;

        float width = _rectTransform.rect.width;
        float height = _rectTransform.rect.height;
        float deltaX = width * 0.5f;
        float deltaY = height * 0.5f;

        if (pos.x < -deltaX)
        {
            pos.x = -deltaX;
        }
        else if(pos.x > deltaX)
        {
            pos.x = deltaX;
        }

        if (pos.y < -deltaY)
        {
            pos.y = -deltaY;
        }
        else if(pos.y > deltaY)
        {
            pos.y = deltaY;
        }

        float x = pos.x + deltaX;
        float y = pos.y + deltaY;

        float xNorm = x / width;
        float yNorm = y / height;

        _pickerTransform.localPosition = pos;
        pickerImage.color = Color.HSVToRGB(0, 0, 1 - yNorm);

        switch (_ColorPickerType)
        {
            case ColorPickerType.BodyColor:
                if (CC) CC.SetSV(xNorm, yNorm);
                break;
            case ColorPickerType.NeonColor:
                if (NCPC) NCPC.SetSV(xNorm, yNorm);
                break;
            case ColorPickerType.WheelSmokeColor:
                if (WSCPC) WSCPC.SetSV(xNorm, yNorm);
                break;
            case ColorPickerType.RimColor:
                if (RCP) RCP.SetSV(xNorm, yNorm);
                break;
            case ColorPickerType.TintColor:
                if (TCP) TCP.SetSV(xNorm, yNorm);
                break;
        }
    }
    
    
    
    public void UpdateColor( Vector3 position)
    {
        if (!_rectTransform || !_pickerTransform ||
            Mathf.Approximately(_rectTransform.rect.width, 0f) || Mathf.Approximately(_rectTransform.rect.height, 0f))
            return;

        Vector3 pos = position;

        float width = _rectTransform.rect.width;
        float height = _rectTransform.rect.height;
        float deltaX = width * 0.5f;
        float deltaY = height * 0.5f;

        pos.x = Mathf.Clamp(pos.x, -deltaX, deltaX);
    
        // Ограничение позиции по оси Y в пределах прямоугольника
        pos.y = Mathf.Clamp(pos.y, -deltaY, deltaY);

        float x = pos.x + deltaX;
        float y = pos.y + deltaY;

        float xNorm = x / width;
        float yNorm = y / height;

        _pickerTransform.localPosition = pos;
        pickerImage.color = Color.HSVToRGB(0, 0, 1 - yNorm);

        switch (_ColorPickerType)
        {
            case ColorPickerType.BodyColor:
                if (CC) CC.SetSV(xNorm, yNorm);
                break;
            case ColorPickerType.NeonColor:
                if (NCPC) NCPC.SetSV(xNorm, yNorm);
                break;
            case ColorPickerType.WheelSmokeColor:
                if (WSCPC) WSCPC.SetSV(xNorm, yNorm);
                break;
            case ColorPickerType.RimColor:
                if (RCP) RCP.SetSV(xNorm, yNorm);
                break;
            case ColorPickerType.TintColor:
                if (TCP) TCP.SetSV(xNorm, yNorm);
                break;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
       UpdateColor(eventData);
    }
    
    

    public void OnPointerClick(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }

    
}
