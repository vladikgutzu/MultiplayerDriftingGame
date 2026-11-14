using UnityEngine;

namespace Script.Menu
{
    public class WorldMapModeCategory : MonoBehaviour
    {
        public GameMode _gameMode;
        public GameObject _worldmapMainCanvasWS;
        public GameObject _worldmapMainCanvas;
        public GameObject[] _raceModeCanvas;

        public void OpenRaceMenu(int _raceMode)
        {
            switch (_raceMode)
            {
                case 1:
                    _gameMode = GameMode.FreeRide;
                    break;
                case 2:
                    _gameMode = GameMode.Multiplayer;
                    break;
                case 3:
                    _gameMode = GameMode.Drift;
                    break;
                case 4:
                    _gameMode = GameMode.Circle;
                    break;
                case 5:
                    _gameMode = GameMode.Sprint;
                    break;
                case 6:
                    _gameMode = GameMode.Tournament;
                    break;
                case 7:
                    _gameMode = GameMode.Career;
                    break;
                case 8:
                    _gameMode = GameMode.CountDown;
                    break;
            }
            BD_MapSelection.SelectMode(_gameMode);
            if (_gameMode == GameMode.Career) BD_CareerProgress.ChallengeSelected = false;
            foreach (var panel in _raceModeCanvas) BD_MapSelection.Refresh(panel);
            _worldmapMainCanvasWS.SetActive(false);
            _worldmapMainCanvas.SetActive(false);
        }

        public void CloseRaceMenu()
        {
            _gameMode = GameMode.None;
            BD_MapSelection.Clear();
            foreach (var t in _raceModeCanvas)
            {
                t.SetActive(false);
            }
            _worldmapMainCanvasWS.SetActive(true);
            _worldmapMainCanvas.SetActive(true);

        }
    }
}
