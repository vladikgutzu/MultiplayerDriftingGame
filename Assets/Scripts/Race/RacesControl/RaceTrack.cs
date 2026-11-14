using System.Collections.Generic;
using GameCore.CustomExtensions.CustomEditorExtensions;
using GameCore.CustomExtensions.Utilities;
using NaughtyAttributes;
using Race.Infrastructure;
using Race.RacesStorage;
using UnityEngine;

namespace Race.RacesControl
{
    public class RaceTrack : MonoBehaviour
    {
        [SerializeField] private RaceConfigSo _raceTrackConfig;

        [Header("Global Configuration")]
        [SerializeField] private RaceSpawnConfigSo _raceSpawnConfig;
        
        [Header("Track Parents")]
        [SerializeField] private Transform _obstaclesParent;
        [SerializeField] private Transform _visualElementsParent;

        [Header("Track Elements")]
        [SerializeField] private RCCP_AIWaypointsContainer _waypointsContainer;
        [SerializeField] private List<RaceTrackVisualElement> _raceTrackVisualElements;
        [SerializeField] private Transform _raceStartPoint;

        public RaceConfigSo RaceTrackConfig => _raceTrackConfig;
        public Transform ObstaclesParent => _obstaclesParent;
        
        public RCCP_AIWaypointsContainer WaypointsContainer => _waypointsContainer;
        public Transform RaceStartPoint => _raceStartPoint;
        public IReadOnlyList<RaceTrackVisualElement> RaceTrackVisualElements => _raceTrackVisualElements;

        public List<Vector3> ConstructSpawnPoints()
        {
            if (_raceTrackConfig == null || _raceSpawnConfig == null || _raceStartPoint == null)
            {
                Debug.LogError($"Cannot construct spawn points for '{name}': race config, spawn config, or start point is missing.");
                return new List<Vector3>();
            }

            RaceType raceType = _raceTrackConfig.AttachedRaceType;
            SpawnConfig spawnConfig = _raceSpawnConfig.GetSpawnConfig(raceType);
            if (spawnConfig == null)
            {
                Debug.LogError($"spawn config for race type {raceType} not found!");
                return new List<Vector3>();
            }
            
            int pointsAmount = Mathf.Max(1, spawnConfig.PointsAmount);
            List<Vector3> results = new(pointsAmount);

            Vector2 firstPointOffset = spawnConfig.PointOffset;
            Vector3 raceStartPointPos = _raceStartPoint.position;
            Vector3 firstPoint = raceStartPointPos 
                                 + _raceStartPoint.forward * firstPointOffset.y 
                                 + _raceStartPoint.right * firstPointOffset.x;
            results.Add(firstPoint);

            for (int i = 1; i < pointsAmount; i++)
            {
                float horOffsetMultiple = (i % 2 == 1) ? 1f : -1f;
                float horizontalOffset = horOffsetMultiple * spawnConfig.BetweenPointsOffset.x;
                float verticalOffset = spawnConfig.BetweenPointsOffset.y;
                Vector3 positionOffset = _raceStartPoint.forward * verticalOffset
                                         + _raceStartPoint.right * horizontalOffset;
                Vector3 prevPointPosition = results[^1];
                Vector3 pointPos = prevPointPosition + positionOffset;
                
                results.Add(pointPos);
            }

            return results;
        }

        private void OnDrawGizmos()
        {
            if (!_raceTrackConfig || !_raceSpawnConfig)
                return;
            
            RaceType raceType = _raceTrackConfig.AttachedRaceType;
            SpawnConfig spawnConfig = _raceSpawnConfig.GetSpawnConfig(raceType);
            if (spawnConfig == null)
                return;

            Gizmos.color = Color.red;
            List<Vector3> foundPoints = ConstructSpawnPoints();
            foreach (Vector3 foundPoint in foundPoints)
            {
                Gizmos.DrawSphere(foundPoint, 0.5f);
            }
        }

#if UNITY_EDITOR

        [Button]
        private void LoadVisualElements()
        {
            _raceTrackVisualElements.Clear();

            List<RaceTrackVisualElement> foundElements = new();
            foundElements.LoadComponentsOfTypeFromParent(_visualElementsParent);
            
            _raceTrackVisualElements.AddRange(foundElements);
            
            EditorExtensions.MarkObjectDirty(this);
            EditorExtensions.SaveAndRefreshAssets();
        }

        [Button]
        private void DebugSpawnPositions()
        {
            List<Vector3> constructedPositions = ConstructSpawnPoints();
            foreach (Vector3 constructedPosition in constructedPositions)
            {
                Debug.Log(constructedPosition);
            }
        }

        public void ChangeRaceConfig(RaceConfigSo raceConfig)
            => _raceTrackConfig = raceConfig;

#endif
    }
}
