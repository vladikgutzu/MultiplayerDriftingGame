using System.Collections.Generic;
using GameCore.CustomExtensions.Utilities;
using GameCore.Services.ServiceStructure;
using Race.Infrastructure;
using Race.RacesStorage;
using UnityEngine;

namespace Race.RacesControl
{
    public class RacesCollectionService : InstantMonoBehaviourService
    {
        [SerializeField] private RacesTypesContainerSo _racesTypesContainer;
        [SerializeField] private RaceConfigsContainerSo _raceConfigsContainer;

        private readonly Dictionary<uint, RaceConfigSo> _raceConfigsDict = new();
        private readonly Dictionary<RaceControlType, int> _rccSettingsBehavioursIndexes = new();

        private readonly Dictionary<RaceControlType, string> _rccBehavioursNames = new()
        {
            { RaceControlType.Drift, "Drift" },
            { RaceControlType.Race, "Racing" }
        };

        private RCCP_Settings _rccSettings;
        
        public RaceConfigSo GetRaceConfig(uint raceId)
            => _raceConfigsDict.ContainsKey(raceId) 
                ? _raceConfigsDict[raceId] 
                : null;

        public RaceConfigSo GetFirstValidRaceConfig()
        {
            if (_raceConfigsContainer == null || _raceConfigsContainer.RacesConfigs == null)
                return null;

            foreach (RaceConfigSo raceConfig in _raceConfigsContainer.RacesConfigs)
                if (raceConfig != null && raceConfig.TrackReference != null && raceConfig.TrackReference.RuntimeKeyIsValid())
                    return raceConfig;

            return null;
        }

        public RaceTypeConfigSo GetRaceTypeConfig(RaceType raceType)
            => _racesTypesContainer != null && _racesTypesContainer.RaceTypeConfigsDict != null &&
               _racesTypesContainer.RaceTypeConfigsDict.ContainsKey(raceType)
                ? _racesTypesContainer.RaceTypeConfigsDict[raceType]
                : null;

        public int GetRccBehaviourIndex(RaceControlType key)
            => _rccSettingsBehavioursIndexes.ContainsKey(key) 
                ? _rccSettingsBehavioursIndexes[key] 
                : -1;

        public override void InitializeService()
        {
            _rccSettings = RCCP_Settings.Instance;
            
            FillRacesConfigs();
            FillRccBehaviours();
        }

        private void FillRacesConfigs()
        {
            _raceConfigsDict.Clear();
            if (_raceConfigsContainer == null || _raceConfigsContainer.RacesConfigs == null)
            {
                Debug.LogError("Race configs container is missing.");
                return;
            }

            foreach (RaceConfigSo raceConfig in _raceConfigsContainer.RacesConfigs)
            {
                if (raceConfig == null)
                    continue;

                if (raceConfig.TrackReference == null || !raceConfig.TrackReference.RuntimeKeyIsValid())
                {
                    Debug.LogWarning($"Race '{raceConfig.name}' is excluded because its track Addressable is missing.", raceConfig);
                    continue;
                }

                _raceConfigsDict.AddOrApplyElement(raceConfig.RaceId, raceConfig);
            }
        }

        private void FillRccBehaviours()
        {
            _rccSettingsBehavioursIndexes.Clear();
            if (_rccSettings == null || _rccSettings.behaviorTypes == null)
            {
                Debug.LogError("RCCP settings or behavior list is missing.");
                return;
            }

            foreach (KeyValuePair<RaceControlType, string> rccBehavioursNamePair in _rccBehavioursNames)
            {
                RaceControlType raceControlType = rccBehavioursNamePair.Key;
                string behaviourName = rccBehavioursNamePair.Value;

                int foundIndex = _rccSettings.behaviorTypes.FindIndex(
                    behaviourType => behaviourType.behaviorName == behaviourName);
                if (foundIndex < 0)
                {
                    Debug.LogError($"behaviour type with control {raceControlType} and name {behaviourName} not found!");
                    continue;
                }
                
                _rccSettingsBehavioursIndexes.AddOrApplyElement(raceControlType, foundIndex);
            }
        }
    }
}
