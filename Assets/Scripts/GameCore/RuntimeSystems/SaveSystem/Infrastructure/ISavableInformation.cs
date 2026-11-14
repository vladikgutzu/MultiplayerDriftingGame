namespace GameCore.RuntimeSystems.SaveSystem.Infrastructure
{
    public interface ISavableInformation
    {
        /// <summary>
        /// Save active object information using SaveLoad system
        /// </summary>
        /// <param name="savePath"></param>
        public void SaveInformation(string savePath);
        
        /// <summary>
        /// Load available information about current object using SaveLoad system
        /// </summary>
        /// <param name="loadPath"></param>
        /// <returns></returns>
        public bool LoadInformation(string loadPath);

        /// <summary>
        /// Creates a new information on current path (relative) and DOESN'T save it
        /// </summary>
        /// <param name="createPath"></param>
        public void CreateInformation(string createPath);
    }
}