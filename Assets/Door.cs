using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public string targetScene;
    public string targetSpawnID;
    private bool playerInside;

    [SerializeField]TypeOfTransition typeOfTransition;
    void Update()
    {
        if (!playerInside) return;

        //  Checa null ANTES de acessar .active
        if (TransitionScene.Instance != null)
        {
            if (TransitionScene.Instance.state != TransitionState.Stoped) return; // já tem transição rolando

            switch(typeOfTransition)
            {
                case TypeOfTransition.ChangeScene:
                TransitionScene.Instance.setTransition(TypeOfTransition.ChangeScene);
                TransitionScene.Instance.LoadSceneWithTransition(targetScene);
                break;

                case TypeOfTransition.TeleportInScene:
                    PlayerInGameDataHolder.Instance.setSpawnID(targetSpawnID);
                    PlayerInGameDataHolder.Instance.Teleport();
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
            // Fallback se o TransitionScene não existir
            PlayerInGameDataHolder.Instance.setSpawnID(targetSpawnID);
            SceneManager.LoadScene(targetScene);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerInteract")) 
        {
            playerInside = true;
        }
    }

    void OiggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PlayerInteract")) 
        {
            playerInside = false;
        }
    }
}
