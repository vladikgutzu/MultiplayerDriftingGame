using App.AppStates;
using GameCore.Services.ServiceStructure;
using UnityEngine;
using Zenject;

namespace Race.RacesControl
{
    public class RaceConstructorGameplayMockService : InstantMonoBehaviourService
    {
        [SerializeField] private GameplayState _gameplayState;

        private RaceConstructor _raceConstructor;
        private ActiveRaceService _activeRaceService;
        
        private bool _initialized;

        [Inject]
        private void Construct(
            ActiveRaceService activeRaceService,
            RaceConstructor raceConstructor)
        {
            _activeRaceService = activeRaceService;
            _raceConstructor = raceConstructor;
        }
        
        public override async void InitializeService()
        {
            if (_initialized)
                return;

            if (_activeRaceService == null || _raceConstructor == null || _gameplayState == null ||
                _raceConstructor.LoadedTrack == null || _raceConstructor.LoadedTrack.RaceTrackConfig == null)
            {
                Debug.LogError("Cannot initialize mock gameplay race: required service, state, track, or race config is missing.");
                return;
            }

            _initialized = true;

            try
            {
                _activeRaceService.SetCurrentRace(_raceConstructor.RaceTackId);
                _activeRaceService.SetMockRaceTrack(_raceConstructor.LoadedTrack);
                await _gameplayState.Enter();
            }
            catch (System.Exception exception)
            {
                _initialized = false;
                Debug.LogException(exception, this);
            }
        }
        
        
    }
}
