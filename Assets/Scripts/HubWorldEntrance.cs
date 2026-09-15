using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HubWorldEntrance : MonoBehaviour
{
    [SerializeField] private string targetScene;
    [SerializeField] private string levelName;
    [SerializeField] private TextMeshProUGUI stagename_ui;
    
    private bool playerInside;
    private bool isProcessing = false; // Trava contra duplo disparo

    private void Update()
    {
        if (!playerInside || isProcessing) return;

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            if (TransitionScene.Instance != null)
            {
                // Se já tem transição rodando, não faz nada
                if (TransitionScene.Instance.state != TransitionState.Stoped) return;
                
                isProcessing = true; // Ativa a trava

                TransitionScene.Instance.setTransition(TypeOfTransition.ChangeScene);
                TransitionScene.Instance.LoadSceneWithTransition(targetScene);
                playerInside = false;
            }
            else
            {
                isProcessing = true;
                SceneManager.LoadScene(targetScene);
            }
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerInteract") && !isProcessing)
        {
            playerInside = true;
            stagename_ui.text = levelName;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerInteract"))
        {
            playerInside = false;
            stagename_ui.text = "";
        }
    }
}