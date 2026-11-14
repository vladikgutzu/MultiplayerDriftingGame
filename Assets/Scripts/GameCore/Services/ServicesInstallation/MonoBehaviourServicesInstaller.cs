using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameCore.CustomExtensions.CustomEditorExtensions;
using GameCore.CustomExtensions.Utilities;
using GameCore.Services.ServiceStructure;
using NaughtyAttributes;
using UnityEngine;
using Zenject;

namespace GameCore.Services.ServicesInstallation
{
    public class MonoBehaviourServicesInstaller : MonoInstaller
    {
        [Header("References")]

        [SerializeField] private List<InstantMonoBehaviourService> _instantMonoBehaviourServices = new();

        [SerializeField] private List<AsynchronousMonoBehaviourService> _asynchronousMonoBehaviourServices = new();

        [Header("Values")]

        [SerializeField] private bool _isEntryPoint;
        [SerializeField] private bool _dontDestroyOnLoad;

        private bool _initialized;
        private Task _initializationTask;

        public bool MarkedAsDontDestroy => _dontDestroyOnLoad;

        public override async void Start()
        {
            base.Start();
            
            if (!_isEntryPoint)
                return;

            try
            {
                await InitializeServices(destroyCancellationToken);
            }
            catch (OperationCanceledException) when (destroyCancellationToken.IsCancellationRequested)
            {
                // Expected when the installer is destroyed during initialization.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        public override void InstallBindings()
        {
            foreach (InstantMonoBehaviourService instantService in _instantMonoBehaviourServices)
            {
                if (instantService != null)
                    InstallService(Container, instantService);
            }

            foreach (AsynchronousMonoBehaviourService asyncService in _asynchronousMonoBehaviourServices)
            {
                if (asyncService != null)
                    InstallService(Container, asyncService);
            }
        }

        public async Task InitializeServices(CancellationToken cancellationToken)
        {
            if (_initialized)
                return;

            if (_initializationTask != null)
            {
                await _initializationTask;
                return;
            }

            _initializationTask = InitializeServicesInternal(cancellationToken);

            try
            {
                await _initializationTask;
            }
            finally
            {
                if (!_initialized)
                    _initializationTask = null;
            }
        }

        private async Task InitializeServicesInternal(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            InitializeInstantServices();
            await InitializeAsynchronousServices(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (_dontDestroyOnLoad) 
                DontDestroyOnLoad(gameObject);
            
            _initialized = true;
        }

        private void InstallService(DiContainer container, MonoBehaviourService serviceToInstall)
        {
            Type serviceType = serviceToInstall.GetType();
            container.Bind(serviceType).FromInstance(serviceToInstall).AsSingle();
        }

        private void InitializeInstantServices()
        {
            foreach (InstantMonoBehaviourService instantService in _instantMonoBehaviourServices)
            {
                if (instantService != null)
                    instantService.InitializeService();
            }
        }

        private async Task InitializeAsynchronousServices(CancellationToken cancellationToken)
        {
            foreach (AsynchronousMonoBehaviourService asyncService in _asynchronousMonoBehaviourServices)
            {
                if (asyncService == null)
                    continue;

                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                
                await asyncService.InitializeService(cancellationToken);
            }
        }

#if UNITY_EDITOR

        [Button("(Re)Load Services")]
        private void ReloadServices()
        {
            _instantMonoBehaviourServices.Clear();
            _asynchronousMonoBehaviourServices.Clear();
            
            _instantMonoBehaviourServices.LoadComponentsOfTypeFromParent(transform);
            _asynchronousMonoBehaviourServices.LoadComponentsOfTypeFromParent(transform);
            
            EditorExtensions.MarkObjectDirty(this);
            EditorExtensions.SaveAndRefreshAssets();
        }
        
#endif
    }
}
