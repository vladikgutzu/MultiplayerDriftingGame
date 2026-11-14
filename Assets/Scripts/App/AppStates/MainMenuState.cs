using App.AppStatesInfra;
using Cysharp.Threading.Tasks;
using GameCore.SceneManagement.Infrastructure;
using GameCore.SceneManagement.ScenesControl;
using UnityEngine;
using Zenject;

namespace App.AppStates
{
    public class MainMenuState : AppState
    {
        private SceneLoaderService _sceneLoaderService;
        private LoadingManager _loadingManager;
        
        [Inject]
        private void Construct(
            SceneLoaderService sceneLoaderService)
        {
            _sceneLoaderService = sceneLoaderService;
        }

        public override async UniTask Enter()
        {
            bool loadResult = await _sceneLoaderService.LoadScene(
                SceneType.Menu, true, LoadSceneRelationship.None);
            if (!loadResult)
            {
                Debug.LogError("Menu scene load fault!");
                return;
            }

            await _sceneLoaderService.UnloadScene(SceneType.Boot);

            _loadingManager = LoadingManager.Instance;
            _loadingManager?.ChangeLoadingState(false);
        }

        public override UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}
