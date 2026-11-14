using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GameCore.CustomExtensions.Utilities;
using GameCore.RuntimeSystems.SaveSystem.Infrastructure;
using UnityEngine;

namespace GameCore.RuntimeSystems.SaveSystem.SaveSystemControl
{
    public static class SaveLoadService
    {
        public static void SaveInformation<T>(T information, string savePath) where T : ISaveSerializable
        {
            if (!TryGetFullPath(savePath, out string fullSavePath))
                return;

            try
            {
                string directory = Path.GetDirectoryName(fullSavePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                string serializedData = JsonUtility.ToJson(information);
                File.WriteAllText(fullSavePath, serializedData);
                MarkPathAsOverwritten(savePath);
                PlayerPrefs.Save();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to save information to '{savePath}': {exception}");
            }
        }

        public static T LoadInformation<T>(string loadPath, out bool result) where T : ISaveSerializable
        {
            result = false;
            
            if (!TryGetFullPath(loadPath, out string fullLoadPath))
                return default;

            if (NecessaryToCreateNewInformation(loadPath))
                return default;

            try
            {
                string serializedData = File.ReadAllText(fullLoadPath);
                T deserializedData = JsonUtility.FromJson<T>(serializedData);
                if (ReferenceEquals(deserializedData, null))
                    return default;

                result = true;
                return deserializedData;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to load information from '{loadPath}': {exception}");
                return default;
            }
        }

        public static bool IsCreatingNewInformationNecessary(string path) 
            => NecessaryToCreateNewInformation(path);

        private static bool NecessaryToCreateNewInformation(string path)
        {
            if (!TryGetFullPath(path, out string fullPath))
                return true;

            bool result = !File.Exists(fullPath) || 
                          !IsPathOverwritten(path);
            return result;
        }

        private static string GetDefaultDataPath()
            => Path.GetFullPath(Application.persistentDataPath);

        private static bool TryGetFullPath(string relativePath, out string fullPath)
        {
            fullPath = null;
            if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            {
                Debug.LogError("Save path must be a non-empty relative path.");
                return false;
            }

            try
            {
                string dataPath = GetDefaultDataPath();
                string candidatePath = Path.GetFullPath(Path.Combine(dataPath, relativePath));
                string allowedPrefix = dataPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                if (!candidatePath.StartsWith(allowedPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError($"Save path '{relativePath}' escapes the persistent data directory.");
                    return false;
                }

                fullPath = candidatePath;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Invalid save path '{relativePath}': {exception.Message}");
                return false;
            }
        }

        private static bool IsPathOverwritten(string path)
            => PlayerPrefs.GetInt("SaveSystem.Written." + path) == 1 || PlayerPrefs.GetInt(path) == 1;
        
        //TODO: Maybe more optimized mark system (for reset information)
        private static void MarkPathAsOverwritten(string path) 
            => PlayerPrefs.SetInt("SaveSystem.Written." + path, 1);
    }
}
