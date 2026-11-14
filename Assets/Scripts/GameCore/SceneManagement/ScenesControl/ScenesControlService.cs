using System;
using System.Linq;
using GameCore.SceneManagement.Infrastructure;
using GameCore.SceneManagement.SceneStorage;
using GameCore.Services.ServiceStructure;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameCore.SceneManagement.ScenesControl
{
    public class ScenesControlService : InstantMonoBehaviourService
    {
        [SerializeField] private SceneContainerSo _sceneContainer;

        private SceneSo activeScene;
        private bool isSubscribed;
        
        public event Action<SceneSo> ActiveSceneChangedEvent;

        public SceneSo ActiveScene => activeScene;

        public override void InitializeService()
        {
            if (isSubscribed)
                return;

            SceneManager.activeSceneChanged += ActiveSceneChanged;
            isSubscribed = true;
            ActiveSceneChanged(default, SceneManager.GetActiveScene());
        }

        public bool TryChangeActiveScene(SceneType sceneTypeToActivate)
        {
            if (_sceneContainer == null || !_sceneContainer.ContainsKey(sceneTypeToActivate))
                return false;

            SceneSo sceneInfo = _sceneContainer[sceneTypeToActivate];
            if (sceneInfo == null || string.IsNullOrWhiteSpace(sceneInfo.SceneName))
                return false;

            string sceneName = sceneInfo.SceneName;
            Scene sceneToActivate = SceneManager.GetSceneByName(sceneName);
            if (!sceneToActivate.IsValid() || !sceneToActivate.isLoaded)
                return false;

            return SceneManager.SetActiveScene(sceneToActivate);
        }

        private void ActiveSceneChanged(Scene from, Scene to)
        {
            if (_sceneContainer == null || !to.IsValid())
                return;

            SceneSo foundScene = _sceneContainer.ScenesCollection.FirstOrDefault(scene =>
                scene != null && string.Equals(scene.SceneName, to.name, StringComparison.Ordinal));
            if (!foundScene)
                return;

            activeScene = foundScene;
            RaiseActiveSceneChangedEvent(foundScene);
        }

        private void RaiseActiveSceneChangedEvent(SceneSo activeScene)
        {
            ActiveSceneChangedEvent?.Invoke(activeScene);
        }

        private void OnDestroy()
        {
            if (isSubscribed)
            {
                SceneManager.activeSceneChanged -= ActiveSceneChanged;
                isSubscribed = false;
            }
        }
    }
}
