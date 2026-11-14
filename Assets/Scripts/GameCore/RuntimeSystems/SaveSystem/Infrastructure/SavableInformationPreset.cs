using GameCore.RuntimeSystems.SaveSystem.SaveSystemControl;

namespace GameCore.RuntimeSystems.SaveSystem.Infrastructure
{
    public abstract class SavableInformationPreset<T> : ISavableInformation where T : ISaveSerializable
    {
        public void SaveInformation(string savePath)
        {
            T saveSerializable = SerializeInformation();
            SaveLoadService.SaveInformation(saveSerializable, savePath);
        }

        public bool LoadInformation(string loadPath)
        {
            if (SaveLoadService.IsCreatingNewInformationNecessary(loadPath))
                return false;

            T loadedSerializable = SaveLoadService.LoadInformation<T>(loadPath, out bool result);
            if (!result)
                return false;
            
            DeserializeInformation(loadedSerializable);

            return true;
        }

        public abstract void CreateInformation(string createPath);

        protected abstract T SerializeInformation();
        protected abstract void DeserializeInformation(T serializableInformation);
    }
}