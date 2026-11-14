using System.Collections.Generic;
using System.Linq;
using GameCore.CustomExtensions.CustomEditorExtensions;
using GameCore.ScriptableObjectsManagement;
using NaughtyAttributes;
using UnityEngine;

namespace Race.RacesStorage
{
    [CreateAssetMenu(fileName = "Race Configs Container", 
        menuName = SoPathConstants.SoRaces + "Race Configs Container")]
    public class RaceConfigsContainerSo : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private UnityEditor.DefaultAsset _folderToLoad;
#endif
        
        [SerializeField] private List<RaceConfigSo> _raceConfigs = new();

        public IReadOnlyList<RaceConfigSo> RacesConfigs => _raceConfigs;

#if UNITY_EDITOR

        [Button("(Re)Load Races Configs")]
        private void LoadConfigs()
        {
            _raceConfigs.Clear();
            List<RaceConfigSo> foundConfigs = EditorExtensions.SearchFilesOfTypeInAsset<RaceConfigSo>(_folderToLoad);
            _raceConfigs.AddRange(foundConfigs);
            
            EditorExtensions.MarkObjectDirty(this);
            EditorExtensions.SaveAndRefreshAssets();
        }

        [Button("(Re)Set races ids")]
        private void SetRacesIds()
        {
            HashSet<uint> occupiedIds = new();
            foreach (RaceConfigSo constantConfig in _raceConfigs
                         .Where(raceConfig => raceConfig.ConstantId))
            {
                occupiedIds.Add(constantConfig.RaceId);
            }

            uint idToCheck = 0;
            foreach (RaceConfigSo configToSet in _raceConfigs
                         .Where(raceConfig => !raceConfig.ConstantId))
            {
                while (occupiedIds.Contains(idToCheck))
                {
                    idToCheck++;
                }

                uint availableId = idToCheck;
                occupiedIds.Add(availableId);
                configToSet.ChangeRaceId(availableId);
            }
            
            EditorExtensions.SaveAndRefreshAssets();
        }

        [Button("(Re)Name races")]
        private void RenameRaces()
        {
            foreach (RaceConfigSo raceConfig in _raceConfigs)
            {
                raceConfig.RenameRace();
            }
        }
        
#endif
    }
}