using System.Linq;
using GameCore.CustomExtensions.CustomEditorExtensions;
using GameCore.CustomExtensions.ObjectExtensions;
using NaughtyAttributes;
using Race.RacesStorage;
using UnityEngine;

namespace Race.RacesControl
{
    public class RaceConstructor : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private UnityEditor.DefaultAsset _racePrefabsFolder;
        [SerializeField] private UnityEditor.DefaultAsset _raceConfigsFolder;
#endif
        
        [SerializeField] private RaceConfigsContainerSo _raceConfigsContainer;

        [SerializeField] private uint _raceToLoad;
        [SerializeField] private RaceConfigSo _raceConfig;
        [SerializeField] private RaceTrack _loadedTrack;

        public uint RaceTackId => _loadedTrack.RaceTrackConfig.RaceId;
        public RaceTrack LoadedTrack => _loadedTrack;

#if UNITY_EDITOR

        [Button("(Re)Load Race Config")]
        private void LoadConfig()
        {
            RaceConfigSo foundRace = _raceConfigsContainer.RacesConfigs.FirstOrDefault(
                raceConfig => raceConfig.RaceId == _raceToLoad);
            if (!foundRace)
            {
                Debug.LogError($"race with id {_raceToLoad} not found!");
                return;
            }
            
            UnloadCurrentRace();

            RaceTrack foundRacePrefab = foundRace.TrackReference.Asset as RaceTrack;
            CustomObjectExtensions.SmartInstantiate(foundRacePrefab, transform);
            
            EditorExtensions.MarkObjectDirty(this);
        }

        [Button("Set track name")]
        private void SetTrackName()
        {
            if (!_loadedTrack)
                return;

            string newName = GetTrackName();
            _loadedTrack.name = newName;

            EditorExtensions.MarkObjectDirty(_loadedTrack);
            EditorExtensions.SaveAndRefreshAssets();
        }

        [Button("Set Prefab track name")]
        private void SetPrefabTrackName()
        {
            if (!_loadedTrack)
                return;
            
            Object foundPrefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(_loadedTrack);
            if (!foundPrefab)
                return;

            GameObject foundPrefabObject = ((RaceTrack)foundPrefab).gameObject;
            string trackName = GetTrackName();
            foundPrefabObject.TryToRenameAsset(trackName);
        }

        [Button("Set Race Config")]
        private void SetRaceConfig()
        {
            if (!_loadedTrack || !_raceConfig)
                return;
            
            _loadedTrack.ChangeRaceConfig(_raceConfig);
        }

        private void UnloadCurrentRace()
        {
            if (!_loadedTrack)
                return;
            
            CustomObjectExtensions.SmartDestroy(_loadedTrack);
            _loadedTrack = null;
        }
        
        private string GetTrackName()
            => $"{_loadedTrack.RaceTrackConfig.RaceId}_{_loadedTrack.RaceTrackConfig.RaceName}_Track";
        
#endif
    }
}