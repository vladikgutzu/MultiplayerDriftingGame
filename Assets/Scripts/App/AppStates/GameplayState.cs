using App.AppStatesInfra;
using Cysharp.Threading.Tasks;
using GameCore.RuntimeSystems.PubSubSystem;
using Race.Events;
using Race.RacesControl;
using UnityEngine;
using Zenject;

namespace App.AppStates
{
    public class GameplayState : AppState
    {
        private ActiveRaceService _activeRaceService;
        private RaceControlService _raceControlService;
        
        [Inject]
        private void Construct(
            ActiveRaceService activeRaceService,
            RaceControlService raceControlService)
        {
            _activeRaceService = activeRaceService;
            _raceControlService = raceControlService;
        }
        
        public override async UniTask Enter()
        {
            await _activeRaceService.LoadRaceTrack();
            if (_activeRaceService.SpawnedRaceTrack == null)
            {
                Debug.LogError("Cannot enter gameplay state: race track failed to load.");
                return;
            }

            PubSubService.Publish(new SessionStartedEvent());

            if (!await _raceControlService.PrepareRace())
            {
                PubSubService.Publish(new SessionFinishedEvent());
                Debug.LogError("Cannot enter gameplay state: race preparation failed.");
                return;
            }

            await _raceControlService.StartRace();
        }

        public override UniTask Exit()
        {
            PubSubService.Publish(new SessionFinishedEvent());
            return UniTask.CompletedTask;
        }
    }
}
