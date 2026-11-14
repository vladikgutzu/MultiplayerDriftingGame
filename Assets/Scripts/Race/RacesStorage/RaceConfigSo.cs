using GameCore.CustomExtensions.CustomEditorExtensions;
using GameCore.ScriptableObjectsManagement;
using NaughtyAttributes;
using Race.Infrastructure;
using Race.RacesControl;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Race.RacesStorage
{
    [CreateAssetMenu(fileName = "Race Config", 
        menuName = SoPathConstants.SoRaces + "Race Config")]
    public class RaceConfigSo : ScriptableObject
    {
        [SerializeField] private uint _raceId;
        [SerializeField] private bool _constantId;
        
        [SerializeField] private RaceType _raceType;
        [SerializeField] private RaceDifficulty _raceDifficulty;

        [SerializeField] private string _raceName;

        [SerializeField] private AssetReference _trackReference;

        public uint RaceId => _raceId;
        public bool ConstantId => _constantId;

        public RaceType AttachedRaceType => _raceType;
        public RaceDifficulty RaceDifficulty => _raceDifficulty;

        public string RaceName => _raceName;
        public AssetReference TrackReference => _trackReference;

#if UNITY_EDITOR

        [Button("(Re)Name Asset")]
        private void RenameAsset()
        {
            string newName = $"{_raceId}({_raceType}) {_raceName}";
            this.TryToRenameAsset(newName);
        }

        public void ChangeRaceId(uint newId)
        {
            if (_constantId)
                return;

            _raceId = newId;
            _constantId = true;
            
            EditorExtensions.MarkObjectDirty(this);
        }

        public void RenameRace() => RenameAsset();

#endif
    }
}