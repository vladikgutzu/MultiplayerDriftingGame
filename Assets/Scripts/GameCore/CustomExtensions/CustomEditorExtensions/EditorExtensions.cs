using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameCore.CustomExtensions.CustomEditorExtensions.FilePrefixes;
using GameCore.CustomExtensions.DebugSystemExtensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameCore.CustomExtensions.CustomEditorExtensions
{
	public static class EditorExtensions
	{
		public static void MarkObjectDirty(Object objectToMark)
		{
#if UNITY_EDITOR
			EditorUtility.SetDirty(objectToMark);
#endif
		}

		public static void MarkPrefabPartObjectDirty(Object objectToMark)
		{
#if UNITY_EDITOR
			EditorUtility.SetDirty(objectToMark);
			PrefabUtility.RecordPrefabInstancePropertyModifications(objectToMark);
#endif
		}
		
		public static void SaveAndRefreshAssets()
		{
#if UNITY_EDITOR
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
#endif
		}

#if UNITY_EDITOR
		public static List<T> SearchFilesOfTypeInAsset<T>(DefaultAsset assetToSearch)
		{
			if (!assetToSearch)
				return new List<T>();
			
			string directoryPath = AssetDatabase.GetAssetPath(assetToSearch);
			IEnumerable<object> loadedObjects = Directory.EnumerateFiles(directoryPath);

			List<T> resultFiles = new List<T>();

			foreach (object obj in loadedObjects)
			{
				string localPath = obj.ToString();
				object file = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

				if (file != null)
				{
					resultFiles.Add((T) file);
				}
			}

			return resultFiles;
		}

		public static List<T> SearchFilesOfDefaultTypesInAsset<T>(DefaultAsset assetToSearch, bool isDirectoryType) where T : Object
		{
			List<string> foundFilesPaths;
			if (isDirectoryType)
			{
				foundFilesPaths = Directory.GetDirectories(AssetDatabase.GetAssetPath(assetToSearch)).ToList();
			}
			else
			{
				foundFilesPaths = Directory.GetFiles(AssetDatabase.GetAssetPath(assetToSearch)).ToList();
			}

			List<T> foundFiles = foundFilesPaths.
				Select(AssetDatabase.LoadAssetAtPath<T>).Where(file => file).ToList();

			return foundFiles;
		}

        public static List<T> SearchFilesInRootAsset<T>(DefaultAsset assetToSearch, string searchPattern = "*.prefab") where T : Object
        {
            List<string> totalFilesPaths = new List<string>();
            Queue<DirectoryInfo> directoriesInfoToSearchQueue = new Queue<DirectoryInfo>();
            
            DirectoryInfo rootDirectoryInfo = new DirectoryInfo(AssetDatabase.GetAssetPath(assetToSearch));
            directoriesInfoToSearchQueue.Enqueue(rootDirectoryInfo);

            while (directoriesInfoToSearchQueue.TryDequeue(out DirectoryInfo directoryToSearch))
            {
                IEnumerable<DirectoryInfo> foundDirectories = directoryToSearch.EnumerateDirectories();
                foreach (DirectoryInfo foundDirectory in foundDirectories) 
                    directoriesInfoToSearchQueue.Enqueue(foundDirectory);

                string directoryPath = "Assets" + directoryToSearch.FullName.Split("Assets")[1];
                IEnumerable<string> foundFilesPaths = Directory.GetFiles(directoryPath);
                totalFilesPaths.AddRange(foundFilesPaths);
            }

            List<T> resultFiles = totalFilesPaths
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(file => file).ToList();
            return resultFiles;
        }

        public static bool TryRemoveComponentInPrefab<T>(GameObject prefabObject, InteractionMode interactionMode = InteractionMode.UserAction) where T : Component
        {
            Scene activeScene = SceneManager.GetActiveScene();
            GameObject spawnedPrefab = PrefabUtility.InstantiatePrefab(prefabObject, activeScene) as GameObject;
            if (!spawnedPrefab)
                return false;

            bool result = false;
            if (spawnedPrefab.TryGetComponent(out T componentToDestroy))
            {
                Object.DestroyImmediate(componentToDestroy);
                T prefabComponent = prefabObject.GetComponent<T>();
                    
                PrefabUtility.ApplyRemovedComponent(spawnedPrefab.gameObject, prefabComponent, interactionMode);
                MarkObjectDirty(prefabObject);
                
                result = true;
            }
                
            Object.DestroyImmediate(spawnedPrefab.gameObject);
            return result;
        }

		public static bool TryToRenameAsset(this Object assetToRename, string newName)
		{
			bool result = RenameAssetName(assetToRename, newName);
			if (!result)
			{
				DebugExtensions.DebugMessage("Failed to rename asset!",
					DebugExtensions.MessageType.Warning);
				return false;
			}

			MarkObjectDirty(assetToRename);
			SaveAndRefreshAssets();

			return true;
		}

		public static bool RenameAssetName(Object assetToReset, string newName)
		{
			if (string.IsNullOrEmpty(newName))
				return false;

			string assetPath = AssetDatabase.GetAssetPath(assetToReset);
			string resultString = AssetDatabase.RenameAsset(assetPath, newName);

			return string.IsNullOrEmpty(resultString);
		}

		public static string GetAssetPath(this Object objectToFind, out bool found)
		{
			found = false;
			
			if (!objectToFind)
				return string.Empty;

			found = true;

			string assetPath = AssetDatabase.GetAssetPath(objectToFind);
			return assetPath;
		}

		public static string GetAssetInFolderWithNameAbsolutePath(DefaultAsset folder, string assetName, FilePrefixType prefixType)
		{
			if (!folder) 
				return string.Empty;
			
			string folderPath = AssetDatabase.GetAssetPath(folder);
			string prefixName = FilePrefixesExtensions.PrefixesConversion(prefixType);
			
			string absolutePath = $"{folderPath}/{assetName}{prefixName}";
			return absolutePath;
		}
		
		public static bool ContainsAssetWithNameInFolder(DefaultAsset folder, string assetName, FilePrefixType prefixType)
		{
			if (!folder)
				return false;

			string absolutePath = GetAssetInFolderWithNameAbsolutePath(folder, assetName, prefixType);

			Object loadedObject = AssetDatabase.LoadAssetAtPath<Object>(absolutePath);
			return loadedObject;
		}

		public static T LoadAssetWithNameFromFolder<T>(DefaultAsset folder, string assetName, FilePrefixType prefixType) where T : Object
		{
			if (!folder)
				return null;
			
			string absolutePath = GetAssetInFolderWithNameAbsolutePath(folder, assetName, prefixType);

			T foundObject = AssetDatabase.LoadAssetAtPath<T>(absolutePath);
			return foundObject;
		}

		public static void ClearFolderFromAssetsType<T>(DefaultAsset folderToClear) where T : Object
		{
			if (!folderToClear)
				return;
			
			List<T> foundAssets = SearchFilesOfTypeInAsset<T>(folderToClear);

			string[] paths = foundAssets.Select(AssetDatabase.GetAssetPath).ToArray();
			List<string> failedPaths = new List<string>();
			AssetDatabase.DeleteAssets(paths, failedPaths);

			if (failedPaths.Count > 0)
			{
				DebugExtensions.DebugMessage($"Failed to remove {failedPaths.Count} pahts!",
					DebugExtensions.MessageType.Warning);
			}
			
			SaveAndRefreshAssets();
		}

		public static T SaveAssetToFolder<T>(DefaultAsset folder, T assetInstance, FilePrefixType prefixType) where T : Object
		{
			if (!folder || !assetInstance)
				return null;

			string possibleSavePath = GetAssetInFolderWithNameAbsolutePath(folder, assetInstance.name, prefixType);
			string savePath = AssetDatabase.GenerateUniqueAssetPath(possibleSavePath);
			
			AssetDatabase.CreateAsset(assetInstance, savePath);
			SaveAndRefreshAssets();
			
			return assetInstance;
		}
		
		public static T SaveAsPrefabAsset<T>(DefaultAsset folder, T assetInstance,
			FilePrefixType prefixType, out bool success, bool connect = true) where T : Object
		{
			GameObject assetInstanceGameObject = ConvertObjectToGameObject(assetInstance);
			success = false;
			
			if (!folder || !assetInstanceGameObject)
				return null;
			
			string possibleSavePath = GetAssetInFolderWithNameAbsolutePath(folder, assetInstance.name, prefixType);
			string savePath = AssetDatabase.GenerateUniqueAssetPath(possibleSavePath);

			T assetPrefab;
			if (connect)
			{
				GameObject assetGameObjectPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
					assetInstanceGameObject, savePath, InteractionMode.UserAction, out bool created);

				success = created;
				assetPrefab = assetGameObjectPrefab.GetComponent<T>();
			}
			else
			{
				GameObject assetGameObjectPrefab = PrefabUtility.SaveAsPrefabAsset(
					assetInstanceGameObject, savePath, out bool created);

				success = created;
				assetPrefab = assetGameObjectPrefab.GetComponent<T>();
			}

			SaveAndRefreshAssets();
			
			return assetPrefab;
		}

		private static GameObject ConvertObjectToGameObject(Object obj)
		{
			return obj switch
			{
				GameObject gameObject => gameObject,
				Component component => component.gameObject,
				_ => null
			};
		}

#endif
	}
}
