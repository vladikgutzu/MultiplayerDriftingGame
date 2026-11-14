using App.AppStatesInfra;
using Cysharp.Threading.Tasks;
using GameCore.SceneManagement.Infrastructure;
using GameCore.SceneManagement.ScenesControl;
using UnityEngine;
using Zenject;

namespace App.AppStates
{
    public class BootState : AppState
    {
        private SceneLoaderService _sceneLoaderService;
        
        [Inject]
        private void Construct(
            SceneLoaderService sceneLoaderService)
        {
            _sceneLoaderService = sceneLoaderService;
        }


        public override async UniTask Enter()
        {
            bool loadResult = await _sceneLoaderService.LoadScene(SceneType.Menu,
                true, LoadSceneRelationship.Child, null, true);

            if (!loadResult)
            {
                Debug.LogError("Boot failed: Menu scene could not be loaded.");
                return;
            }

            await _sceneLoaderService.UnloadScene(SceneType.Boot);
        }

        public override UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}
