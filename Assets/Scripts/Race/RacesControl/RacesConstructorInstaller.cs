using UnityEngine;
using Zenject;

namespace Race.RacesControl
{
    public class RacesConstructorInstaller : MonoInstaller
    {
        [SerializeField] private RaceConstructor _raceConstructor;

        public override void InstallBindings()
        {
            Container.Bind<RaceConstructor>().FromInstance(_raceConstructor).AsSingle();
        }
        
        
    }
}