using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameCore.RuntimeSystems.PubSubSystem;
using GameCore.Services.ServiceStructure;
using Race.Events;
using Race.RacesControl;
using UnityEngine;
using Zenject;
#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
#endif

namespace RaceCar.CarControl
{
    public class RaceCarsSpawnService : InstantMonoBehaviourService
    {
        /// <summary>Shared offline spawn path. Never steals the player's camera or input.</summary>
        public static RCCP_CarController SpawnLocalOpponent(GameObject prefab, Vector3 position,
            Quaternion rotation, RCCP_AIWaypointsContainer waypoints, int opponentIndex = 0, bool driftOpponent = false)
        {
            if (prefab == null || prefab.GetComponent<RCCP_CarController>() == null || waypoints == null)
                return null;
            var scene = RCCP_SceneManager.Instance;
            bool autoRegister = scene != null && scene.registerLastVehicleAsPlayer;
            var staging = new GameObject("Opponent staging");
            staging.SetActive(false);
            GameObject spawned = null;
            try {
                if (scene != null) scene.registerLastVehicleAsPlayer = false;
                spawned = Instantiate(prefab, position, rotation, staging.transform);
                spawned.SetActive(false);
                spawned.transform.SetParent(null, true);
                var car = spawned.GetComponent<RCCP_CarController>();
                car.ineffectiveBehavior = true;
                // Some legacy prefabs (notably Miata) have an unassigned differential.
                // Repair only the local instance; never overwrite another car's tune.
                foreach (var differential in spawned.GetComponentsInChildren<RCCP_Differential>(true))
                    if (differential.connectedAxle == null)
                        differential.connectedAxle = spawned.GetComponentsInChildren<RCCP_Axle>(true).FirstOrDefault(a => !a.isSteer && a.leftWheelCollider != null && a.rightWheelCollider != null);
                // Do not let local opponents read/write the selected player's
                // parts, paint or performance preferences during Start/OnEnable.
                foreach (var behaviour in spawned.GetComponentsInChildren<MonoBehaviour>(true))
                    if (behaviour is BD_ModApplier || behaviour.GetType().Name.StartsWith("BD_VehicleUpgrade_", System.StringComparison.Ordinal))
                        behaviour.enabled = false;
                var tuning = spawned.AddComponent<BD_BotTuning>();
                tuning.Configure(opponentIndex, BD_PlayerProgression.CompletedRaces);
                var scoring = spawned.GetComponent<BD_PlayerManager>();
                if (scoring != null) { scoring.IsLocalOpponent = true; scoring.canScore = driftOpponent; scoring.enabled = driftOpponent; }
#if PHOTON_UNITY_NETWORKING
                // A local clone has no network owner. Lobby connections may
                // otherwise interpolate it back to its spawn and overwrite AI.
                foreach (var sync in spawned.GetComponentsInChildren<RCCP_PhotonSync>(true)) {
                    sync.enabled = false;
                    Destroy(sync);
                }
                foreach (var view in spawned.GetComponentsInChildren<PhotonView>(true)) Destroy(view);
#endif
                foreach (var camera in spawned.GetComponentsInChildren<Camera>(true)) camera.enabled = false;
                foreach (var listener in spawned.GetComponentsInChildren<AudioListener>(true)) listener.enabled = false;
                var ai = spawned.GetComponent<RCCP_AI>();
                if (driftOpponent) { if (ai != null) ai.enabled = false; }
                else {
                    if (ai == null) ai = spawned.AddComponent<RCCP_AI>();
                    ai.waypointsContainer = waypoints;
                    ai.navigationMode = RCCP_AI.NavigationMode.FollowWaypoints;
                    ai.directWaypointSteering = true;
                    ai.externallyManagedWaypoints = true;
                }
                car.canControl = false;
                spawned.SetActive(true);
                // Every local rival uses the same tyre and drift layer as the
                // player. Road modes still use ordered checkpoints and do not
                // award drift score, but bots may now rotate through corners
                // instead of grip-driving every circuit like a train.
                var drift = spawned.GetComponent<BD_DriftController>();
                if (drift == null) drift = spawned.AddComponent<BD_DriftController>();
                drift.enabled = true;
                if (car.AllWheelColliders != null)
                    foreach (var wheel in car.AllWheelColliders) if (wheel != null) wheel.driftMode = true;
                if (car.Inputs != null) {
                    car.Inputs.overrideInternalInputs = true;
                    car.Inputs.overrideExternalInputs = true;
                    car.Inputs.counterSteering = false;
                    // Bot recovery explicitly selects R/forward. Player-style
                    // auto-reverse otherwise cancels a deliberate reverse manoeuvre.
                    car.Inputs.autoReverse = false;
                    car.Inputs.inverseThrottleBrakeOnReverse = true;
                }
                if (car.Stability != null) { car.Stability.TCS = false; car.Stability.ESP = false; car.Stability.ABS = true; }
                car.SetEngine(true);
                if (car.Lights != null && BD_GameplayManager.Instance != null)
                    car.Lights.lowBeamHeadlights = BD_GameplayManager.Instance.isNight;
                return car;
            }
            catch {
                if (spawned != null) Destroy(spawned);
                throw;
            }
            finally {
                if (scene != null) scene.registerLastVehicleAsPlayer = autoRegister;
                Destroy(staging);
            }
        }

        private ActiveRaceService _activeRaceService;
        private BD_PlayerCars _playerCarsCollection;
        private BD_GameplayManager _bdGameplayManager;

        private RaceCar _activePlayerCar;
        private readonly List<RaceCar> _spawnedEnemiesCars = new();

        private int _spawnPointIndex = 0;

        public RaceCar ActivePlayerCar => _activePlayerCar;
        public IReadOnlyList<RaceCar> SpawnedEnemiesCars => _spawnedEnemiesCars;

        [Inject]
        private void Construct(ActiveRaceService activeRaceService)
        {
            _activeRaceService = activeRaceService;

            PubSubService.RegisterListener<SessionStartedEvent>(HandleGameplaySessionStarted);
            PubSubService.RegisterListener<SessionFinishedEvent>(HandleGameplaySessionFinished);
        }

        public async UniTask<bool> PreparePlayerCar()
        {
            LinkBdInformation();
            if (_bdGameplayManager == null || _playerCarsCollection == null)
            {
                Debug.LogError("Cannot prepare player car: gameplay manager or player car collection is missing.");
                return false;
            }

            Transform spawnPoint = GetAvailableSpawnPoint();
            if (!spawnPoint)
            {
                Debug.LogError("No spawn point found for car!");
                return false;
            }

            bool playerSpawned = false;
            bool spawnEventReceived = false;
            void HandlePlayerSpawned(BD_PlayerManager spawnedPlayer)
            {
                spawnEventReceived = true;
                if (spawnedPlayer != null && spawnedPlayer.TryGetComponent(out _activePlayerCar))
                    playerSpawned = true;
                else if (spawnedPlayer != null)
                    Destroy(spawnedPlayer.gameObject);
            }

            BD_GameplayManager.OnPlayerSpawned += HandlePlayerSpawned;

            try
            {
                _bdGameplayManager.SpawnPlayer(spawnPoint);

                float timeoutAt = Time.realtimeSinceStartup + 10f;
                while (!spawnEventReceived && Time.realtimeSinceStartup < timeoutAt && !destroyCancellationToken.IsCancellationRequested)
                    await UniTask.Yield();
            }
            finally
            {
                BD_GameplayManager.OnPlayerSpawned -= HandlePlayerSpawned;
            }

            if (!playerSpawned)
            {
                Debug.LogError("Player car spawn timed out or spawned prefab has no RaceCar component.");
                return false;
            }

            _spawnPointIndex++;
            return true;
        }
        
        public bool PrepareOpponentCar(int spawnCarIndex)
        {
            LinkBdInformation();
            
            RaceCar carPrefab = GetCarPrefab(spawnCarIndex);
            if (!carPrefab)
            {
                Debug.LogError($"car with index {spawnCarIndex} not found!");
                return false;
            }

            Transform spawnPoint = GetAvailableSpawnPoint();
            if (!spawnPoint)
            {
                Debug.LogError("No spawn point found for car!");
                return false;
            }

            if (_activeRaceService == null || _activeRaceService.SpawnedRaceTrack == null ||
                _activeRaceService.SpawnedRaceTrack.WaypointsContainer == null)
            {
                Debug.LogError("Cannot spawn opponent: active track or AI waypoints are missing.");
                return false;
            }

            RaceCar spawnedCar = Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);
            if (spawnedCar == null)
                return false;

            _spawnPointIndex++;

            RCCP_AI carAI = spawnedCar.GetComponent<RCCP_AI>();
            if (carAI == null)
                carAI = spawnedCar.gameObject.AddComponent<RCCP_AI>();
            carAI.waypointsContainer = _activeRaceService.SpawnedRaceTrack.WaypointsContainer;
            
            
            
            _spawnedEnemiesCars.Add(spawnedCar);
            return true;
        }

        private RaceCar GetCarPrefab(int carIndex)
        {
            if (_playerCarsCollection == null || _playerCarsCollection.playerCars == null)
                return null;

            BD_PlayerCars.SelectablePlayerCars[] selectablePlayerCarsArray = _playerCarsCollection.playerCars;
            if (carIndex < 0 || carIndex >= selectablePlayerCarsArray.Length)
                return null;

            GameObject carObjectPrefab = selectablePlayerCarsArray[carIndex]?.car;
            if (carObjectPrefab == null)
                return null;

            RaceCar carPrefab = carObjectPrefab.GetComponent<RaceCar>();

            return carPrefab;
        }

        private Transform GetAvailableSpawnPoint()
        {
            if (_activeRaceService == null || _activeRaceService.SpawnPoints == null ||
                _spawnPointIndex < 0 || _spawnPointIndex >= _activeRaceService.SpawnPoints.Count)
                return null;

            Transform foundSpawnPoint = _activeRaceService.SpawnPoints[_spawnPointIndex];
            return foundSpawnPoint;
        }

        private void HandleGameplaySessionFinished()
        {
            _spawnPointIndex = 0;
            ClearSpawnedCars();
        }

        private void HandleGameplaySessionStarted()
        {
            _spawnPointIndex = 0;
        }

        private void ClearSpawnedCars()
        {
            if (_activePlayerCar)
            {
                DestroyRaceCar(_activePlayerCar);
            }

            foreach (RaceCar enemyCar in _spawnedEnemiesCars.Where(car => car))
            {
                DestroyRaceCar(enemyCar);
            }

            _activePlayerCar = null;
            _spawnedEnemiesCars.Clear();
        }

        private static void DestroyRaceCar(RaceCar raceCar)
        {
            if (raceCar == null)
                return;

#if PHOTON_UNITY_NETWORKING
            PhotonView view = raceCar.GetComponent<PhotonView>();
            if (PhotonNetwork.InRoom && view != null && view.IsMine)
            {
                PhotonNetwork.Destroy(raceCar.gameObject);
                return;
            }
#endif
            Destroy(raceCar.gameObject);
        }

        private void LinkBdInformation()
        {
            _bdGameplayManager = BD_GameplayManager.Instance;
            _playerCarsCollection = BD_PlayerCars.Instance;
        }

        private void OnDestroy()
        {
            PubSubService.UnregisterListener<SessionStartedEvent>(HandleGameplaySessionStarted);
            PubSubService.UnregisterListener<SessionFinishedEvent>(HandleGameplaySessionFinished);
            ClearSpawnedCars();
        }
    }
}
