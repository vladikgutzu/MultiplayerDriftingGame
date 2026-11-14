using System.Collections.Generic;
using Race.Infrastructure;
using UnityEngine;

namespace Race.RacesControl
{
    /// <summary>Completes ordered waypoint races without allowing finish-line shortcuts.</summary>
    public sealed class RaceWaypointProgress : MonoBehaviour
    {
        private readonly List<Transform> _waypoints = new();
        private BD_GameplayManager _gameplayManager;
        private RaceType _raceType;
        private int _nextWaypoint;
        private bool _completed;
        private float _triggerRadiusSquared;
        private int _lap = 1;
        private LineRenderer _marker;
        private Material _markerMaterial;
        private bool _finishPlayerSession = true;
        private int _passed;
        public bool RacingEnabled { get; set; } = true;
        public bool Completed => _completed;
        public int NextWaypointIndex => _nextWaypoint;
        public float FinishTime { get; private set; }
        public event System.Action<RaceWaypointProgress> Finished;
        public float RaceProgress {
            get {
                if (_completed) return float.MaxValue;
                if (_waypoints.Count < 2) return 0f;
                int next = Mathf.Clamp(_nextWaypoint, 0, _waypoints.Count - 1);
                Vector3 a = _waypoints[(next + _waypoints.Count - 1) % _waypoints.Count].position;
                Vector3 segment = _waypoints[next].position - a;
                return _passed + Mathf.Clamp01(Vector3.Dot(transform.position - a, segment) / Mathf.Max(.01f, segment.sqrMagnitude));
            }
        }
        public string ProgressLabel => _completed ? "Finished" : _raceType == RaceType.Cycle ? "Lap " + _lap + "/3 — checkpoint " + (_nextWaypoint + 1) + "/" + _waypoints.Count : "Checkpoint " + (_nextWaypoint + 1) + "/" + _waypoints.Count;

        public bool Initialize(BD_GameplayManager gameplayManager, RaceType raceType,
            RCCP_AIWaypointsContainer container, float triggerRadius = 18f, bool finishPlayerSession = true)
        {
            _gameplayManager = gameplayManager;
            _raceType = raceType;
            _completed = false;
            _lap = 1;
            _passed = 0;
            FinishTime = 0f;
            RacingEnabled = true;
            _finishPlayerSession = finishPlayerSession;
            _waypoints.Clear();

            if (_gameplayManager == null || container == null)
                return false;

            RCCP_Waypoint[] foundWaypoints = container.GetComponentsInChildren<RCCP_Waypoint>(true);
            System.Array.Sort(foundWaypoints, CompareWaypoints);
            foreach (RCCP_Waypoint waypoint in foundWaypoints)
                if (waypoint != null)
                    _waypoints.Add(waypoint.transform);

            if (_waypoints.Count < 2)
                return false;

            _nextWaypoint = 1;
            _triggerRadiusSquared = Mathf.Max(1f, triggerRadius) * Mathf.Max(1f, triggerRadius);
            if (_finishPlayerSession) CreateMarker();
            return true;
        }

        private void CreateMarker()
        {
            if (_marker != null) return;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            if (shader == null) return;
            _marker = new GameObject("Next Checkpoint Gate").AddComponent<LineRenderer>();
            _markerMaterial = new Material(shader);
            _markerMaterial.color = new Color(0f, 1f, .7f, 1f);
            _marker.sharedMaterial = _markerMaterial;
            _marker.positionCount = 4;
            _marker.startWidth = _marker.endWidth = .35f;
            _marker.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _marker.receiveShadows = false;
        }

        private void Update()
        {
            if (_marker != null) _marker.enabled = !_completed && _gameplayManager != null && _gameplayManager.raceStarted;
            if (_completed || !RacingEnabled || _gameplayManager == null || !_gameplayManager.raceStarted ||
                _nextWaypoint < 0 || _nextWaypoint >= _waypoints.Count)
                return;

            Transform waypoint = _waypoints[_nextWaypoint];
            if (_marker != null && waypoint != null) {
                Vector3 forward = waypoint.position - _waypoints[(_nextWaypoint + _waypoints.Count - 1) % _waypoints.Count].position;
                Vector3 right = Vector3.Cross(Vector3.up, forward.normalized) * 9f;
                Vector3 centre = waypoint.position + Vector3.up * .2f;
                _marker.SetPosition(0, centre - right);
                _marker.SetPosition(1, centre - right + Vector3.up * 6f);
                _marker.SetPosition(2, centre + right + Vector3.up * 6f);
                _marker.SetPosition(3, centre + right);
            }
            if (waypoint == null)
                return;

            Vector3 previous = _waypoints[(_nextWaypoint + _waypoints.Count - 1) % _waypoints.Count].position;
            Vector3 segment = Vector3.ProjectOnPlane(waypoint.position - previous, Vector3.up);
            Vector3 fromPrevious = Vector3.ProjectOnPlane(transform.position - previous, Vector3.up);
            float along = Vector3.Dot(fromPrevious, segment) / Mathf.Max(.01f, segment.sqrMagnitude);
            float lateral = Vector3.Distance(fromPrevious, segment * Mathf.Clamp01(along));
            bool insideCheckpoint = (transform.position - waypoint.position).sqrMagnitude <= _triggerRadiusSquared;
            bool crossedGate = along >= 1f && along <= 1.5f && lateral <= Mathf.Sqrt(_triggerRadiusSquared) * 1.35f;
            if (!insideCheckpoint && !crossedGate)
                return;

            _passed++;

            if (_raceType == RaceType.Cycle && _nextWaypoint == 0) {
                if (_lap >= 3) { Complete(); return; }
                _lap++;
                _nextWaypoint = 1;
                return;
            }
            _nextWaypoint++;
            if (_nextWaypoint < _waypoints.Count)
                return;

            if (_raceType == RaceType.Cycle)
            {
                _nextWaypoint = 0;
                return;
            }

            Complete();
        }

        private void Complete()
        {
            _completed = true;
            FinishTime = Mathf.Max(0f, _gameplayManager.targetTime - _gameplayManager.currentTimer);
            Finished?.Invoke(this);
            if (_finishPlayerSession) _gameplayManager.FinishRace();
        }

        private void OnDestroy()
        {
            if (_marker != null) Destroy(_marker.gameObject);
            if (_markerMaterial != null) Destroy(_markerMaterial);
        }

        private static int CompareWaypoints(RCCP_Waypoint left, RCCP_Waypoint right)
        {
            return ExtractTrailingNumber(left != null ? left.name : string.Empty)
                .CompareTo(ExtractTrailingNumber(right != null ? right.name : string.Empty));
        }

        private static int ExtractTrailingNumber(string value)
        {
            int start = value.Length;
            while (start > 0 && char.IsDigit(value[start - 1]))
                start--;

            return start < value.Length && int.TryParse(value.Substring(start), out int number)
                ? number
                : int.MaxValue;
        }
    }
}
