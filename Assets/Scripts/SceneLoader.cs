using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    private void Start()
    {
        string sceneName = PlayerPrefs.GetString("NextSceneName", "Boot");
        if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Cannot load saved target scene '{sceneName}'. Falling back to Main Menu.");
            sceneName = Application.CanStreamedLevelBeLoaded("Main Menu") ? "Main Menu" : "Boot";
            PlayerPrefs.SetString("NextSceneName", sceneName);
        }

        Debug.Log($"Loading scene '{sceneName}'.");
        StartCoroutine(TransitionAndLoadSceneAsync(sceneName));

    }

    IEnumerator TransitionAndLoadSceneAsync(string sceneName)
    {
        // Unload the previous scene asynchronously
        // AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        //
        // // Wait until the previous scene is unloaded
        // while (!unloadOperation.isDone)
        // {
        //     yield return null;
        // }

        // Load the new scene asynchronously
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        if (loadOperation == null)
        {
            Debug.LogError($"Unity failed to start loading scene '{sceneName}'.");
            yield break;
        }

        // Don't allow scene to activate until we allow it
        loadOperation.allowSceneActivation = false;
        
        // Wait until the new scene is fully loaded
        while (!loadOperation.isDone)
        {
            // Check if the load has finished
            if (loadOperation.progress >= 0.9f)
            {
                // Activate the scene to transition to it
                loadOperation.allowSceneActivation = true;
            }

            yield return null;
        }
        
    }
}
