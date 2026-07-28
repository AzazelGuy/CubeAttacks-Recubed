using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

public class PauseMenuActions : MonoBehaviour
{
    public static PauseMenuActions instance;
    [SerializeField] private Animator animator;

    [SerializeField] private GameObject pauseMenuGroup;

    private bool isPaused;
    private bool playingAnimation;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        pauseMenuGroup.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !playingAnimation)
        {
            string pauseTrigger = isPaused ? "Unpause" : "Pause";
            animator.SetTrigger(pauseTrigger);
        }
    }

    public void Resume()
    {
        if (playingAnimation) return;
        string pauseTrigger = isPaused ? "Unpause" : "Pause";
        animator.SetTrigger(pauseTrigger);

    }

    #region Animation Methods
    public void Pause()
    {
        isPaused = true;

        Time.timeScale = isPaused ? 0f : 1f;
    }
    public void UnPause()
{
    isPaused = false;
    EventSystem.current.SetSelectedGameObject(null);
    
    // Reset button animations
    Button[] buttons = pauseMenuGroup.GetComponentsInChildren<Button>();
    foreach (Button btn in buttons)
    {
        btn.OnDeselect(null); // Force deselect state
    }
    
    Time.timeScale = isPaused ? 0f : 1f;
}
    public void StartAnimation()
    {
        playingAnimation = true;
        CanvasGroup canvasGroup = pauseMenuGroup.GetComponent<CanvasGroup>();
        if (canvasGroup) canvasGroup.blocksRaycasts = false;
    }

    public void EndAnimation()
    {
        playingAnimation = false;
        CanvasGroup canvasGroup = pauseMenuGroup.GetComponent<CanvasGroup>();
        if (canvasGroup) canvasGroup.blocksRaycasts = true;
        ChangeActive();
    }
    public void ChangeActive()
    {
        pauseMenuGroup.SetActive(isPaused);
    }
    #endregion
    public bool Paused()
    {
        return isPaused;
    }
}
