using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelAnimation : MonoBehaviour
{
    public List<BD_ButtonSlideAnimation> objectsToAnimate;
    public GameObject objectToDisable;
    public GameObject objectToShow; 
    private Coroutine waitForPanelsCoroutine;
    private MonoBehaviour transitionHost;

    public void HideAllPanels()
    {
        if (objectToShow == null || objectToShow == objectToDisable) {
            Debug.LogWarning("Panel transition has no valid destination.", this);
            return;
        }
        foreach (var panel in objectsToAnimate ?? new List<BD_ButtonSlideAnimation>())
        {
            if (panel != null)
                panel.HideOffset();
            
        }
        
        if (objectToDisable != null && objectToDisable.name == "Wheel Customization Slider")
        {
            BD_MainMenuManager.Instance?._customizer?.SaveStats();
        }
        
        if (waitForPanelsCoroutine != null)
            (transitionHost != null ? transitionHost : this).StopCoroutine(waitForPanelsCoroutine);
        // The clicked button is often inside the panel being animated out.
        // Hosting the coroutine there stops it as soon as that panel disables.
        transitionHost = BD_MainMenuManager.Instance != null ? BD_MainMenuManager.Instance : this;
        waitForPanelsCoroutine = transitionHost.StartCoroutine(WaitUntilAllPanelsHidden());
    }
    

    private IEnumerator WaitUntilAllPanelsHidden()
    {
        float deadline = Time.realtimeSinceStartup + 1f;
        while (!AreAllPanelsHidden() && Time.realtimeSinceStartup < deadline)
            yield return new WaitForSecondsRealtime(.1f);

        // Detach the running coroutine before SetActive invokes OnDisable
        // synchronously. This lets the transition finish without OnDisable trying
        // to stop the coroutine that is currently switching the panels.
        waitForPanelsCoroutine = null;

        GameObject panelToDisable = objectToDisable;
        GameObject panelToShow = objectToShow;
        if (BD_MainMenuManager.Instance != null && panelToShow != null) {
            if (panelToDisable != null && !panelToShow.transform.IsChildOf(panelToDisable.transform))
                panelToDisable.SetActive(false);
            BD_MainMenuManager.Instance.OpenMenu(panelToShow);
            yield break;
        }
        if (panelToDisable != null && panelToDisable.activeSelf)
            panelToDisable.SetActive(false);
        if (panelToShow != null && !panelToShow.activeSelf)
            panelToShow.SetActive(true);
        if (panelToShow != null) {
            for (Transform parent = panelToShow.transform.parent; parent != null; parent = parent.parent)
                if (!parent.gameObject.activeSelf) parent.gameObject.SetActive(true);
        }
    }

    private bool AreAllPanelsHidden()
    {
        foreach (var panel in objectsToAnimate ?? new List<BD_ButtonSlideAnimation>())
        {
            if (panel != null && panel.gameObject.activeInHierarchy)
            {
                return false;
            }
        }
        return true;
        
    }

    private void OnEnable()
    {
        if (objectsToAnimate == null)
            return;

        foreach (var panel in objectsToAnimate)
        {
            // Restoring children while their parent is being disabled causes a
            // recursive SetActive call. Prepare them only after the parent is active.
            if (panel != null && panel.activateOnDisable && !panel.gameObject.activeSelf)
                panel.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (waitForPanelsCoroutine != null && transitionHost == this)
        {
            StopCoroutine(waitForPanelsCoroutine);
            waitForPanelsCoroutine = null;
        }
    }
}

