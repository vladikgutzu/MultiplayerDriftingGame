using System.Collections.Generic;
using GameCore.CustomExtensions.Collections;
using GameCore.CustomExtensions.CustomEditorExtensions;
using GameCore.CustomExtensions.Utilities;
using GameCore.ScriptableObjectsManagement;
using NaughtyAttributes;
using Race.Infrastructure;
using UnityEngine;

namespace Race.RacesStorage
{
    [CreateAssetMenu(fileName = "Races Types Container", 
        menuName = SoPathConstants.SoRaces + "Races Types Container")]
    public class RacesTypesContainerSo : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private UnityEditor.DefaultAsset _folderToLoad;
#endif
        
        [SerializeField] private GenericDictionary<RaceType, RaceTypeConfigSo> _raceTypeConfigsDict = new();

        public GenericDictionary<RaceType, RaceTypeConfigSo> RaceTypeConfigsDict => _raceTypeConfigsDict;

#if UNITY_EDITOR

        [Button("(Re)Load Race Types Configs")]
        private void LoadConfigs()
        {
            _raceTypeConfigsDict.Clear();
            List<RaceTypeConfigSo> foundConfigs = EditorExtensions
                .SearchFilesOfTypeInAsset<RaceTypeConfigSo>(_folderToLoad);

            foreach (RaceTypeConfigSo raceTypeConfig in foundConfigs)
            {
                _raceTypeConfigsDict.AddOrApplyElement(raceTypeConfig.AttachedRaceType, raceTypeConfig);
            }
            
            EditorExtensions.MarkObjectDirty(this);
            EditorExtensions.SaveAndRefreshAssets();
        }
        
#endif
    }
}