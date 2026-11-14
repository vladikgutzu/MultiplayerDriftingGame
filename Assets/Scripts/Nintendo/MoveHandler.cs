using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MoveHandler : MonoBehaviour,IMoveHandler,ISelectHandler
{


    public SVImageControl SvImageControl;
    
    
   // public UnityEvent positionChanged; 


    private RectTransform rect;
    private float _verticalInput;
    private float _horizontalInput;
    public float moveSpeed = 500f; 
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }
    
  


    public void ChangePickerPosition()
    {

        if (EventSystem.current == null || rect == null || SvImageControl == null ||
            EventSystem.current.currentSelectedGameObject!=this.gameObject) return;
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;
            _horizontalInput = (keyboard.rightArrowKey.isPressed ? 1f : 0f) - (keyboard.leftArrowKey.isPressed ? 1f : 0f);
            _verticalInput = (keyboard.upArrowKey.isPressed ? 1f : 0f) - (keyboard.downArrowKey.isPressed ? 1f : 0f);
            Vector2 moveDirection = new Vector2(_horizontalInput, _verticalInput);
            rect.anchoredPosition += moveDirection * moveSpeed * Time.deltaTime;
            SvImageControl.UpdateColor(rect.anchoredPosition);

        
        
    }


    private void FixedUpdate()
    {
        ChangePickerPosition();
    }

    public void ObjectMoved()
    {
        if (SvImageControl != null && rect != null) SvImageControl.UpdateColor(rect.anchoredPosition);
    }

    public void OnMove(AxisEventData eventData)
    {
        //ObjectMoved();
        
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Selection may be assigned in the same frame the palette is enabled,
        // before every picker dependency has completed OnEnable/Start. The
        // palette already initializes its output texture itself; updating the
        // color here is redundant and used to throw during that first frame.
        if (rect == null)
            rect = GetComponent<RectTransform>();

    }
}
