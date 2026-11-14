using UnityEngine;

namespace Race.RacesStorage
{
    [System.Serializable]
    public class SpawnConfig
    {
        [Min(1), SerializeField] private int _pointsAmount = 1;
        [SerializeField] private Vector2 _pointOffset;
        [SerializeField] private Vector2 _betweenPointsOffset;

        public int PointsAmount => _pointsAmount;
        public Vector2 PointOffset => _pointOffset;
        public Vector2 BetweenPointsOffset => _betweenPointsOffset;
    }
}