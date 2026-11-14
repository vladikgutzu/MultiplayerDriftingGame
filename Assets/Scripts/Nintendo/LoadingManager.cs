using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
#if UNITY_SWITCH && !UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

public class LoadingManager : MonoBehaviour
{
    public GameObject loadingObject;
    public Image loadingImage; 
    public List<Sprite> spritesForLoading;
    
    
    private static LoadingManager instance;
    private bool isLoading;
    public static LoadingManager Instance {

        get {

            if (instance == null)
                instance = FindAnyObjectByType<LoadingManager>();

            return instance;
        }

    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // The loading canvas is serialized as active in the bootstrap scene. Keep it
        // hidden until LoadSceneAsync explicitly starts a transition; otherwise it
        // survives DontDestroyOnLoad and permanently covers the destination scene.
        if (loadingObject != null)
            loadingObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void ChangeLoadingState(bool isActive)
    {
        if (loadingObject != null)
            loadingObject.SetActive(isActive);
    }

    public void LoadScene(int sceneIndex)
    {
        if (isLoading)
            return;

        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"Cannot load scene index {sceneIndex}: it is not present in Build Settings.");
            return;
        }

        StartCoroutine(LoadSceneAsync(sceneIndex));
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        isLoading = true;
        SetRandomSprite();
        if (loadingObject != null)
            loadingObject.SetActive(true);

        // EventSystem.current.sendNavigationEvents = false;


#if UNITY_SWITCH && !UNITY_EDITOR
        if(Gamepad.current != null)
            InputSystem.DisableDevice(Gamepad.current);
#endif

        
        AsyncOperation loadingScene = SceneManager.LoadSceneAsync(sceneIndex);

        if (loadingScene == null)
        {
            if (loadingObject != null)
                loadingObject.SetActive(false);
            isLoading = false;
            Debug.LogError($"Unity failed to start loading scene index {sceneIndex}.");
            yield break;
        }
        
        Resources.UnloadUnusedAssets();

        while (loadingScene.isDone == false)
        {
            yield return null;
        }
#if UNITY_SWITCH && !UNITY_EDITOR
        if(Gamepad.current != null)
            InputSystem.EnableDevice(Gamepad.current);
#endif

        // EventSystem.current.sendNavigationEvents = true;
        
        if (loadingObject != null)
            loadingObject.SetActive(false);
        isLoading = false;
    }


    private void SetRandomSprite()
    {
        if (loadingImage != null && spritesForLoading != null && spritesForLoading.Count > 0)
        {
            loadingImage.sprite = spritesForLoading[Random.Range(0, spritesForLoading.Count)];
        }
        else
        {
            Debug.LogWarning("Loading screen has no optional loading sprites assigned.", this);
        }
    }
}
