using System;
using Cysharp.Threading.Tasks;
using GameCore.SceneManagement.Infrastructure;
using GameCore.SceneManagement.SceneStorage;
using GameCore.Services.ServiceStructure;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace GameCore.SceneManagement.ScenesControl
{
    public class SceneLoaderService : InstantMonoBehaviourService
    {
        [SerializeField] private SceneContainerSo _sceneContainer;

        private ZenjectSceneLoader _zenjectSceneLoader;
        
        [Inject]
        private void Construct(ZenjectSceneLoader zenjectSceneLoader)
        {
            _zenjectSceneLoader = zenjectSceneLoader;
        }
        
        public async UniTask<bool> LoadScene(
            SceneType sceneToLoad, 
            bool setSceneActive, 
            bool forceLoad = false)
        {
            if (_sceneContainer == null || !_sceneContainer.ContainsKey(sceneToLoad))
            {
                Debug.LogError($"Cannot load scene '{sceneToLoad}': it is not registered in the scene container.");
                return false;
            }
            
            SceneSo loadSceneInfo = _sceneContainer[sceneToLoad];
            string sceneName = loadSceneInfo.SceneName;
            Scene loadScene = SceneManager.GetSceneByName(sceneName);

            if (loadScene.isLoaded)
            {
                if (!forceLoad)
                {
                    SetSceneActivation(loadScene, setSceneActive);
                    return true;
                }

                await SceneManager.UnloadSceneAsync(loadScene);
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
            if (loadOperation == null)
            {
                Debug.LogError($"Unity failed to start loading scene '{sceneName}'.");
                return false;
            }

            await loadOperation;
            
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            SetSceneActivation(loadedScene, setSceneActive);

            return true;
        }

        public async UniTask<bool> LoadScene(
            SceneType sceneToLoad, 
            bool setSceneActive,
            LoadSceneRelationship loadSceneRelationship,
            IProgress<float> loadingProgress = null,
            bool forceLoad = false)
        {
            if (_sceneContainer == null || !_sceneContainer.ContainsKey(sceneToLoad))
            {
                Debug.LogError($"Cannot load scene '{sceneToLoad}': it is not registered in the scene container.");
                return false;
            }

            if (_zenjectSceneLoader == null)
            {
                Debug.LogError($"Cannot load scene '{sceneToLoad}': ZenjectSceneLoader is not available.");
                return false;
            }
            
            SceneSo loadSceneInfo = _sceneContainer[sceneToLoad];
            string sceneName = loadSceneInfo.SceneName;
            Scene loadScene = SceneManager.GetSceneByName(sceneName);

            if (loadScene.isLoaded)
            {
                if (!forceLoad)
                {
                    SetSceneActivation(loadScene, setSceneActive);
                    return true;
                }

                await SceneManager.UnloadSceneAsync(loadScene);
            }

            await _zenjectSceneLoader
                .LoadSceneAsync(sceneName, LoadSceneMode.Additive, null, loadSceneRelationship)
                .ToUniTask(progress: loadingProgress);

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            SetSceneActivation(loadedScene, setSceneActive);

            return true;
        }

        public async UniTask UnloadScene(SceneType sceneToUnload)
        {
            if (_sceneContainer == null || !_sceneContainer.ContainsKey(sceneToUnload))
                return;
            
            SceneSo loadSceneInfo = _sceneContainer[sceneToUnload];
            string sceneName = loadSceneInfo.SceneName;
            Scene unloadScene = SceneManager.GetSceneByName(sceneName);
            if (!unloadScene.isLoaded)
                return;

            await SceneManager.UnloadSceneAsync(unloadScene);
        }

        private void SetSceneActivation(Scene sceneToChangeActivation, bool isActive)
        {
            if (!isActive)
                return;

            if (!sceneToChangeActivation.IsValid() || !sceneToChangeActivation.isLoaded ||
                !SceneManager.SetActiveScene(sceneToChangeActivation))
                Debug.LogError($"Cannot activate scene '{sceneToChangeActivation.name}'.");
        }
        
    }
}
