using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using GameCore.RuntimeSystems.PubSubSystem;
using GameCore.Services.ServiceStructure;
using Race.Infrastructure;
using Race.Events;
using Race.RacesStorage;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace Race.RacesControl
{
    public class ActiveRaceService : InstantMonoBehaviourService
    {
        private RacesCollectionService _racesCollectionService;
        
        private uint _currentRaceId;
        private RaceTrack _spawnedRaceTrack;
        private bool _ownsSpawnedTrack;
        private bool _isLoadingTrack;

        private Transform _spawnPointsParent;
        private readonly List<Transform> _activeTrackSpawnPoints = new();

        public uint CurrentRaceId => _currentRaceId;
        public RaceTrack SpawnedRaceTrack => _spawnedRaceTrack;
        public RaceType ActiveTrackRaceType => _spawnedRaceTrack.RaceTrackConfig.AttachedRaceType;
        public List<Transform> SpawnPoints => _activeTrackSpawnPoints;

        [Inject]
        private void Construct(
            RacesCollectionService racesCollectionService)
        {
            _racesCollectionService = racesCollectionService;
            PubSubService.RegisterListener<SessionFinishedEvent>(HandleSessionFinished);
        }

        public void SetCurrentRace(uint raceId) 
            => _currentRaceId = raceId;

        public void SetMockRaceTrack(RaceTrack mockTrack)
        {
            if (!mockTrack || mockTrack.RaceTrackConfig == null || mockTrack.RaceStartPoint == null)
            {
                Debug.LogError("Cannot set mock race track: track config or start point is missing.");
                return;
            }

            ReleaseOwnedTrack();
            
            _spawnedRaceTrack = mockTrack;
            _ownsSpawnedTrack = false;
            ClearSpawnPoints();
            ConstructTrackSpawnPoints();
        }

        public async UniTask LoadRaceTrack()
        {
            if (_spawnedRaceTrack)
            {
                if (_activeTrackSpawnPoints.Count == 0)
                    ConstructTrackSpawnPoints();
                return;
            }

            if (_isLoadingTrack)
            {
                await UniTask.WaitWhile(() => _isLoadingTrack, cancellationToken: destroyCancellationToken);
                return;
            }

            if (_racesCollectionService == null)
            {
                Debug.LogError("Cannot load race track: races collection service is missing.");
                return;
            }
            
            RaceConfigSo raceConfig = _racesCollectionService.GetRaceConfig(_currentRaceId);
            if (raceConfig == null)
            {
                raceConfig = _racesCollectionService.GetFirstValidRaceConfig();
                if (raceConfig != null)
                {
                    Debug.LogWarning($"Race {_currentRaceId} is unavailable; loading valid fallback race {raceConfig.RaceId}.");
                    _currentRaceId = raceConfig.RaceId;
                }
            }

            if (raceConfig == null || raceConfig.TrackReference == null || !raceConfig.TrackReference.RuntimeKeyIsValid())
            {
                Debug.LogError($"Cannot load race {_currentRaceId}: race config or track Addressable is invalid.");
                return;
            }

            _isLoadingTrack = true;
            GameObject spawnedTrack = null;
            try
            {
                spawnedTrack = await raceConfig.TrackReference.InstantiateAsync();
                if (spawnedTrack == null || !spawnedTrack.TryGetComponent(out _spawnedRaceTrack) ||
                    _spawnedRaceTrack.RaceTrackConfig == null || _spawnedRaceTrack.RaceStartPoint == null)
                {
                    Debug.LogError($"Cannot load race {_currentRaceId}: spawned track is missing RaceTrack, its config, or start point.");
                    if (spawnedTrack != null)
                        Addressables.ReleaseInstance(spawnedTrack);
                    _spawnedRaceTrack = null;
                    return;
                }

                _ownsSpawnedTrack = true;

                ClearSpawnPoints();
                ConstructTrackSpawnPoints();
            }
            catch (OperationCanceledException)
            {
                if (spawnedTrack != null && !_ownsSpawnedTrack)
                    Addressables.ReleaseInstance(spawnedTrack);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                if (spawnedTrack != null)
                    Addressables.ReleaseInstance(spawnedTrack);
                _spawnedRaceTrack = null;
                _ownsSpawnedTrack = false;
            }
            finally
            {
                _isLoadingTrack = false;
            }
        }

        private void ConstructTrackSpawnPoints()
        {
            if (_spawnedRaceTrack == null || _spawnedRaceTrack.RaceStartPoint == null)
                return;

            List<Vector3> constructedPoints = _spawnedRaceTrack.ConstructSpawnPoints();
            if (constructedPoints == null || constructedPoints.Count == 0)
            {
                Debug.LogError("Cannot construct race spawn points: track returned no positions.");
                return;
            }

            GameObject spawnPointsParent = new("TrackSpawnPoints");
            _spawnPointsParent = spawnPointsParent.transform;
            
            int spawnPointIndex = 0;
            foreach (Vector3 point in constructedPoints)
            {
                Transform spawnPoint = new GameObject($"SpawnPoint_{spawnPointIndex}").transform;
                spawnPoint.position = point;
                spawnPoint.rotation = _spawnedRaceTrack.RaceStartPoint.rotation;
                
                spawnPoint.SetParent(_spawnPointsParent);
                _activeTrackSpawnPoints.Add(spawnPoint);
                spawnPointIndex++;
            }
        }
        
        private void ClearSpawnPoints()
        {
            if (_spawnPointsParent)
                Destroy(_spawnPointsParent.gameObject);

            _spawnPointsParent = null;
            _activeTrackSpawnPoints.Clear();
        }

        private void HandleSessionFinished()
        {
            ClearSpawnPoints();
            ReleaseOwnedTrack();
        }

        private void ReleaseOwnedTrack()
        {
            if (_ownsSpawnedTrack && _spawnedRaceTrack != null)
                Addressables.ReleaseInstance(_spawnedRaceTrack.gameObject);

            if (_ownsSpawnedTrack)
                _spawnedRaceTrack = null;
            _ownsSpawnedTrack = false;
        }

        private void OnDestroy()
        {
            PubSubService.UnregisterListener<SessionFinishedEvent>(HandleSessionFinished);
            ClearSpawnPoints();
            ReleaseOwnedTrack();
        }
    }
}
