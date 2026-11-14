using GameCore.CustomExtensions.CustomEditorExtensions;
using GameCore.ScriptableObjectsManagement;
using NaughtyAttributes;
using Race.Infrastructure;
using UnityEngine;

namespace Race.RacesStorage
{
    [CreateAssetMenu(fileName = "Race Type Config", 
        menuName = SoPathConstants.SoRaces + "Race Type Config")]
    public class RaceTypeConfigSo : ScriptableObject
    {
        [SerializeField] private RaceType _raceType;
        [SerializeField] private string _raceTypeName;

        [Space(10f)]
        [SerializeField] private RaceControlType _raceControlType;

        [Space(10f)]
        [SerializeField] private int _carsAmount = 1;

        [HorizontalLine(color: EColor.Green)]
        [SerializeField] private Sprite _raceSprite;

        public RaceType AttachedRaceType => _raceType;
        public string RaceTypeName => _raceTypeName;
        public RaceControlType ControlType => _raceControlType;

        public int CarsAmount => _carsAmount;
        
        public Sprite RaceSprite => _raceSprite;

#if UNITY_EDITOR

        [Button("(Re)Name Asset")]
        private void RenameAsset()
        {
            string raceTypeString = _raceType.ToString();
            string newName = raceTypeString + " Race Type Config";
            
            if (string.IsNullOrEmpty(_raceTypeName))
            {
                _raceTypeName = raceTypeString;
            }

            this.TryToRenameAsset(newName);
        }
        
#endif
    }
}