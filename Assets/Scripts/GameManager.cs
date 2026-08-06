using UnityEngine;
// Inside GameManager.cs or your persistent Boot script:

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public PlayerSaveData currentSaveData { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Load save file on game boot
            currentSaveData = SaveSystem.LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveCurrentProgress()
    {
        SaveSystem.SaveGame(currentSaveData);
    }
}