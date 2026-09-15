using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public string targetScene;
    public string targetSpawnID;
    private bool playerInside;
    private bool isProcessing = false; // Trava contra duplo disparo

    [SerializeField] private TypeOfTransition typeOfTransition;

    private void Update()
    {
        if (!playerInside || isProcessing) return;

        if (TransitionScene.Instance != null)
        {
            // Se já tem transição rodando, não faz nada
            if (TransitionScene.Instance.state != TransitionState.Stoped) return;

            isProcessing = true; // Ativa a trava

            switch (typeOfTransition)
            {
                case TypeOfTransition.ChangeScene:
                    TransitionScene.Instance.setTransition(TypeOfTransition.ChangeScene);
                    TransitionScene.Instance.LoadSceneWithTransition(targetScene);
                    break;

                case TypeOfTransition.TeleportInScene:
                    PlayerInGameDataHolder.Instance.setSpawnID(targetSpawnID);
                    PlayerInGameDataHolder.Instance.Teleport();
                    isProcessing = false; // Libera se for apenas teleport local rápido
                    break;

                case TypeOfTransition.ChangeSceneAndTP:
                    PlayerInGameDataHolder.Instance.setSpawnID(targetSpawnID);
                    TransitionScene.Instance.setTransition(TypeOfTransition.ChangeSceneAndTP);
                    TransitionScene.Instance.LoadSceneWithTransition(targetScene);
                    break;
            }

            playerInside = false;
        }
        else
        {
            isProcessing = true;
            PlayerInGameDataHolder.Instance.setSpawnID(targetSpawnID);
            SceneManager.LoadScene(targetScene);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerInteract") && !isProcessing)
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PlayerInteract"))
        {
            playerInside = false;
        }
    }
}