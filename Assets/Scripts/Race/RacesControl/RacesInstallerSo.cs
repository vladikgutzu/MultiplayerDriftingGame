using GameCore.ScriptableObjectsManagement;
using UnityEngine;
using Zenject;

namespace Race.RacesControl
{
    [CreateAssetMenu(fileName = "Races Installer", 
        menuName = SoPathConstants.SoInstallers + "Races Installer")]
    public class RacesInstallerSo : ScriptableObjectInstaller
    {
        
        
        public override void InstallBindings()
        {
            
        }
    }
}