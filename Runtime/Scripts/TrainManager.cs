using UnityEngine;

namespace HGS.RLAgents
{
    public class TrainManager : MonoBehaviour
    {
        [SerializeField] Brain brain;
        [SerializeField] GameObject agentPrefab;
        [SerializeField] string modelPath = "/SavedModels/model.json";
        [SerializeField] int epochs = 10;
        [SerializeField] int agentsByEpoch = 10;
        [SerializeField] int stepsByEpoch = 100;
        [SerializeField] float maxEpochDuration = 5f;
        [SerializeField] float saveRate = 2f;
        [SerializeField] float timeScale = 1;

        float _saveTimer = 0;


        // Método para salvar o modelo
        protected virtual void SaveModel()
        {
            string filePath = Application.dataPath + modelPath;
            brain.SaveModel(filePath);
        }

        private void SaveModelUpdate()
        {
            if (_saveTimer >= saveRate)
            {
                SaveModel();
                _saveTimer = 0;
            }
        }

        protected virtual void Update()
        {
            if (!brain.trainMode) return;

            SaveModelUpdate();

            _saveTimer += Time.unscaledDeltaTime;
            Time.timeScale = timeScale;
        }
    }
}
