using System.Threading;
using App.AppStates;
using App.AppStatesInfra;
using GameCore.Services.ServicesInstallation;
using UnityEngine;
using Zenject;

namespace App
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private MonoBehaviourServicesInstaller _servicesInstaller;

        private readonly CancellationTokenSource _bootCancellationTokenSource = new();
        
        private AppStateMachineService _appStateMachine;
        
        [Inject]
        private void Construct(
            AppStateMachineService appStateMachine)
        {
            _appStateMachine = appStateMachine;
        }
        
        private async void Start()
        {
            if (_servicesInstaller == null || _appStateMachine == null)
            {
                Debug.LogError("Cannot bootstrap application: services installer or state machine is missing.");
                return;
            }

            try
            {
                await _servicesInstaller.InitializeServices(_bootCancellationTokenSource.Token);

                if (_bootCancellationTokenSource.IsCancellationRequested)
                    return;
            
                await _appStateMachine.Enter<BootState>();

                if (_bootCancellationTokenSource.IsCancellationRequested)
                    return;

                await _appStateMachine.Enter<MainMenuState>();
            }
            catch (System.OperationCanceledException) when (_bootCancellationTokenSource.IsCancellationRequested)
            {
                // Expected when the bootstrap object is destroyed during startup.
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void OnDestroy()
        {
            _bootCancellationTokenSource.Cancel();
            _bootCancellationTokenSource.Dispose();
        }
    }
}
