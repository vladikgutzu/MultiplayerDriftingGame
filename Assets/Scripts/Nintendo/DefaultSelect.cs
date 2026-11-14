using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DefaultSelect : MonoBehaviour
{

    
    public enum PageType
    {
        Default,Scroll,None
    }

    

    [SerializeField] private EventSystem _eventSystem;
    [SerializeField] private GameObject _defaultSelectGameObject;
    public PageType pageType; 
    private Coroutine selectCoroutine;
    
    void Awake()
    {
        _eventSystem = EventSystem.current;
    }

    private void OnEnable()
    {
        if (_eventSystem == null)
            _eventSystem = EventSystem.current;

        selectCoroutine = StartCoroutine(CheckPageType());
    }

    public  void SelectGameObject( GameObject gameObject)
    {

       
        if (_eventSystem == null)
            _eventSystem = EventSystem.current;

        if (_eventSystem == null || gameObject == null || !gameObject.activeInHierarchy)
            return;

        if (_eventSystem.currentSelectedGameObject == null || !_eventSystem.currentSelectedGameObject.activeInHierarchy)
        {
            _eventSystem.SetSelectedGameObject(gameObject);
        }
    }


    private IEnumerator CheckPageType()
    {

        switch (pageType)
        {
            
            case PageType.Default:
                
                yield return new WaitForSecondsRealtime(.1f);
                SelectGameObject(_defaultSelectGameObject);
                break;
            case PageType.Scroll:
                yield return new WaitForSecondsRealtime(.5f);
                if (_defaultSelectGameObject != null && _defaultSelectGameObject.transform.childCount > 0 &&
                    _defaultSelectGameObject.transform.GetChild(0).childCount > 0)
                    SelectGameObject(_defaultSelectGameObject.transform.GetChild(0).GetChild(0).gameObject);
                break;
            case PageType.None:
                SelectGameObject(_defaultSelectGameObject);
                break;
        }

        selectCoroutine = null;
    }


    private void OnDisable()
    {
        if (selectCoroutine != null)
        {
            StopCoroutine(selectCoroutine);
            selectCoroutine = null;
        }

        if (_eventSystem != null && _eventSystem.currentSelectedGameObject != null &&
            _eventSystem.currentSelectedGameObject.transform.IsChildOf(transform))
            _eventSystem.SetSelectedGameObject(null);
    }
}


