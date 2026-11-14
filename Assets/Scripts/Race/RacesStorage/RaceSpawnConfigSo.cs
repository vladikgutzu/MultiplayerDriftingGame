using GameCore.CustomExtensions.Collections;
using GameCore.ScriptableObjectsManagement;
using Race.Infrastructure;
using UnityEngine;

namespace Race.RacesStorage
{
    [CreateAssetMenu(fileName = "Race Spawn Config", 
        menuName = SoPathConstants.SoRaces + "Race Spawn Config")]
    public class RaceSpawnConfigSo : ScriptableObject
    {
        [SerializeField] private GenericDictionary<RaceType, SpawnConfig> _raceSpawnConfigs = new();

        public SpawnConfig GetSpawnConfig(RaceType key)
            => _raceSpawnConfigs.ContainsKey(key) 
                ? _raceSpawnConfigs[key] 
                : null;
    }
}