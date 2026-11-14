using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.CustomExtensions.CustomEditorExtensions;
using GameCore.CustomExtensions.Utilities;
using GameCore.Services.ServiceStructure;
using NaughtyAttributes;
using UnityEngine;

namespace App.AppStatesInfra
{

    public class AppStateMachineService : InstantMonoBehaviourService
    {
        [SerializeField] private List<AppState> _appStatesList = new();
        
        private readonly Dictionary<Type, AppState> _states = new();
        private AppState _activeState;

        public override void InitializeService()
        {
            _states.Clear();
            foreach (AppState appState in _appStatesList)
            {
                if (appState == null)
                    continue;

                Type stateType = appState.GetType();
                _states.AddOrApplyElement(stateType, appState);
            }
        }

        public async UniTask Enter<TState>() where TState : AppState
        {
            TState state = GetState<TState>();

            if (_activeState == null)
            {
                await SetActiveState(state);
                return;
            }

            if (_activeState.Equals(state))
                return;

            await _activeState.Exit();
            await SetActiveState(state);
        }

        private TState GetState<TState>() where TState: AppState
        {
            if (!_states.TryGetValue(typeof(TState), out AppState state) || state == null)
                throw new InvalidOperationException($"Application state '{typeof(TState).Name}' is not registered.");

            return state as TState;
        }

        private async UniTask SetActiveState<TState>(TState state) where TState : AppState
        {
            await state.Enter();
            _activeState = state;
        }

#if UNITY_EDITOR

        [Button("(Re)Load States")]
        private void LoadStates()
        {
            _appStatesList.Clear();
            _appStatesList.LoadComponentsOfTypeFromParent(transform);
            
            EditorExtensions.MarkObjectDirty(this);
            EditorExtensions.SaveAndRefreshAssets();
        }
        
#endif
    }
}
