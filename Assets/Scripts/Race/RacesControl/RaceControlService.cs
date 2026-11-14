using Cysharp.Threading.Tasks;
using GameCore.Services.ServiceStructure;
using Race.Infrastructure;
using Race.RacesStorage;
using RaceCar.CarControl;
using UnityEngine;
using Zenject;

namespace Race.RacesControl
{
    public class RaceControlService : InstantMonoBehaviourService
    {
        private RacesCollectionService _racesCollectionService;
        private ActiveRaceService _activeRaceService;
        private RaceCarsSpawnService _raceCarsSpawnService;
        
        private BD_GameplayManager _bdGameplayManager;
        private BD_PlayerCars _bdPlayerCars;
        private RCCP_Settings _rccSettings;
        
        [Inject]
        private void Construct(
            RacesCollectionService racesCollectionService,
            ActiveRaceService activeRaceService,
            RaceCarsSpawnService raceCarsSpawnService)
        {
            _racesCollectionService = racesCollectionService;
            _activeRaceService = activeRaceService;
            _raceCarsSpawnService = raceCarsSpawnService;
        }
        
        public async UniTask<bool> PrepareRace()
        {
            _bdGameplayManager = BD_GameplayManager.Instance;
            _bdPlayerCars = BD_PlayerCars.Instance;
            _rccSettings = RCCP_Settings.Instance;

            if (_bdGameplayManager == null || _bdPlayerCars == null || _rccSettings == null ||
                _activeRaceService == null || _racesCollectionService == null || _raceCarsSpawnService == null ||
                _activeRaceService.SpawnedRaceTrack == null ||
                _activeRaceService.SpawnedRaceTrack.RaceTrackConfig == null ||
                _activeRaceService.SpawnedRaceTrack.RaceStartPoint == null)
            {
                Debug.LogError("Cannot prepare race: one or more required services, settings, or track are missing.");
                return false;
            }
            
            if (!PrepareControlBehaviour() || !_bdGameplayManager.PrepareGameplay())
                return false;

            _bdGameplayManager.gameType = ConvertGameMode(_activeRaceService.ActiveTrackRaceType);
            
            if (!await PrepareRaceCars())
                return false;

            return PrepareRaceCompletion();
        }

        public UniTask StartRace()
        {
            _bdGameplayManager.StartGame();
            return UniTask.CompletedTask;
        }
        
        private bool PrepareControlBehaviour()
        {
            RaceType raceType = _activeRaceService.ActiveTrackRaceType;
            RaceTypeConfigSo raceTypeConfig = _racesCollectionService.GetRaceTypeConfig(raceType);
            if (raceTypeConfig == null)
            {
                Debug.LogError($"Cannot prepare controls: race type config for {raceType} is missing.");
                return false;
            }
            
            int behaviourIndex = _racesCollectionService.GetRccBehaviourIndex(raceTypeConfig.ControlType);
            if (behaviourIndex < 0)
            {
                Debug.LogError($"Cannot prepare controls: RCCP behaviour for {raceTypeConfig.ControlType} is missing.");
                return false;
            }

            _rccSettings.behaviorSelectedIndex = behaviourIndex;
            return true;
        }

        private static BD_GameplayManager.GameMode ConvertGameMode(RaceType raceType)
        {
            switch (raceType)
            {
                case RaceType.FreeRoad:
                    return BD_GameplayManager.GameMode.FreeRide;
                case RaceType.Sprint:
                    return BD_GameplayManager.GameMode.Sprint;
                case RaceType.Cycle:
                    return BD_GameplayManager.GameMode.Circle;
                case RaceType.Drift:
                    return BD_GameplayManager.GameMode.Drift;
                default:
                    return BD_GameplayManager.GameMode.FreeRide;
            }
        }

        private async UniTask<bool> PrepareRaceCars()
        {
            RaceType raceType = _activeRaceService.ActiveTrackRaceType;
            RaceTypeConfigSo raceTypeConfig = _racesCollectionService.GetRaceTypeConfig(raceType);
            if (!raceTypeConfig)
            {
                Debug.LogError($"Race Type config for {raceType} not found!");
                return false;
            }
            
            int carsAmount = Mathf.Max(1, raceTypeConfig.CarsAmount);
            int availableSpawnPoints = _activeRaceService.SpawnPoints != null ? _activeRaceService.SpawnPoints.Count : 0;
            if (availableSpawnPoints == 0)
            {
                Debug.LogError("Cannot prepare race cars: track has no spawn points.");
                return false;
            }

            int opponentsAmount = Mathf.Min(carsAmount - 1, availableSpawnPoints - 1);
            if (opponentsAmount < carsAmount - 1)
                Debug.LogWarning($"Race requests {carsAmount} cars but track only has {availableSpawnPoints} spawn points. Opponents were reduced.");
            
            if (!await _raceCarsSpawnService.PreparePlayerCar())
                return false;

            BD_PlayerCars.SelectablePlayerCars[] foundCars = _bdPlayerCars.playerCars;
            if (foundCars == null || foundCars.Length == 0)
            {
                Debug.LogError("Cannot prepare opponents: player car catalog is empty.");
                return false;
            }

            System.Collections.Generic.List<int> validOpponentIndexes = new System.Collections.Generic.List<int>();
            for (int i = 0; i < foundCars.Length; i++)
            {
                if (foundCars[i]?.car != null && foundCars[i].car.GetComponent<RaceCar.CarControl.RaceCar>() != null)
                    validOpponentIndexes.Add(i);
            }

            if (opponentsAmount > 0 && validOpponentIndexes.Count == 0)
            {
                Debug.LogError("Cannot prepare opponents: no car prefab contains RaceCar.");
                return false;
            }

            for (int i = 0; i < opponentsAmount; i++)
            {
                int carIndex = validOpponentIndexes[Random.Range(0, validOpponentIndexes.Count)];
                if (!_raceCarsSpawnService.PrepareOpponentCar(carIndex))
                    return false;
            }

            return true;
        }

        private bool PrepareRaceCompletion()
        {
            RaceType raceType = _activeRaceService.ActiveTrackRaceType;
            if (raceType != RaceType.Sprint && raceType != RaceType.Cycle)
                return true;

            RaceCar.CarControl.RaceCar activePlayerCar = _raceCarsSpawnService.ActivePlayerCar;
            RCCP_AIWaypointsContainer waypoints = _activeRaceService.SpawnedRaceTrack.WaypointsContainer;
            if (activePlayerCar == null || waypoints == null)
            {
                Debug.LogError($"Cannot prepare {raceType} completion: player car or track waypoints are missing.");
                return false;
            }

            RaceWaypointProgress progress = activePlayerCar.GetComponent<RaceWaypointProgress>();
            if (progress == null)
                progress = activePlayerCar.gameObject.AddComponent<RaceWaypointProgress>();

            if (!progress.Initialize(_bdGameplayManager, raceType, waypoints))
            {
                Debug.LogError($"Cannot prepare {raceType} completion: at least two ordered waypoints are required.");
                return false;
            }

            return true;
        }
    }
}
