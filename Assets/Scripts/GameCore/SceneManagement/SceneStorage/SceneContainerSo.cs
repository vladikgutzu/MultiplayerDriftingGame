using System.Collections.Generic;
using GameCore.CustomExtensions.Collections;
using GameCore.CustomExtensions.CustomEditorExtensions;
using GameCore.CustomExtensions.Utilities;
using GameCore.Infrastructure;
using GameCore.SceneManagement.Infrastructure;
using NaughtyAttributes;
using UnityEngine;

namespace GameCore.SceneManagement.SceneStorage
{
    [CreateAssetMenu(fileName = "Scene Container", 
        menuName = GameCoreSoPaths.SoSceneCollection + "Scene Container")]
    public class SceneContainerSo : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private UnityEditor.DefaultAsset _folderToLoad;
#endif
        
        [SerializeField] private GenericDictionary<SceneType, SceneSo> _scenesDict = 
            new GenericDictionary<SceneType, SceneSo>();

        public int Count => _scenesDict.Count;

        public bool ContainsKey(SceneType key)
            => _scenesDict.ContainsKey(key);

        public SceneSo this[SceneType key]
            => _scenesDict[key];

        public ICollection<SceneSo> ScenesCollection => _scenesDict.Values;

#if UNITY_EDITOR

        [Button("(Re)Load Scenes")]
        private void ReloadScenes()
        {
            _scenesDict.Clear();

            List<SceneSo> loadedScenes = EditorExtensions.SearchFilesOfTypeInAsset<SceneSo>(_folderToLoad);

            foreach (SceneSo scene in loadedScenes)
            {
                if (scene.SceneType == SceneType.None)
                    continue;

                _scenesDict.AddOrApplyElement(scene.SceneType, scene);
            }
            
            EditorExtensions.MarkObjectDirty(this);
            EditorExtensions.SaveAndRefreshAssets();
        }
        
#endif
    }
}