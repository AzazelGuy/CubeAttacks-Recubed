using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public enum TransitionState
{
    Stoped,
    Entering,
    Loading,
    Exiting
}

public enum TypeOfTransition
{
    ChangeScene,
    TeleportInScene,
    ChangeSceneAndTP
}

public class TransitionScene : MonoBehaviour
{
    public static TransitionScene Instance;

    [Header("UI & Visuals")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject loadingScreenUI;
    [SerializeField] private Image progressBar;

    [Header("Flavor Text UI")]
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("Loading Effects Settings")]
    [SerializeField] private float rainbowSpeed = 2.0f;
    [SerializeField] private float dotsInterval = 0.4f;
    [SerializeField] [TextArea(2, 4)] private string[] gameTips;

    [Header("Settings")]
    [Tooltip("Tempo máximo de espera da animação caso o Animator falhe ou não tenha Animation Event.")]
    [SerializeField] private float animationTimeLimit = 1.5f;
    [Tooltip("Tempo mínimo que a tela de carregamento deve ser exibida.")]
    [SerializeField] private float minLoadingTime = 1.0f;

    public TransitionState CurrState { get; private set; } = TransitionState.Stoped;
    private TypeOfTransition type = TypeOfTransition.ChangeScene;

    private bool isEnterAnimationFinished = false;
    private bool isExitAnimationFinished = false;

    private Coroutine rainbowCoroutine;
    private Coroutine dotsCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (loadingScreenUI != null)
                loadingScreenUI.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSceneWithTransition(string sceneName)
    {
        if (CurrState != TransitionState.Stoped)
        {
            Debug.LogWarning("[TransitionScene] Transição já em andamento. Chamada duplicada ignorada.");
            return;
        }

        StartCoroutine(TransitionCoroutine(sceneName));
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        // Reset time scale in case game was paused when leaving a level
        Time.timeScale = 1f;

        isEnterAnimationFinished = false;
        isExitAnimationFinished = false;

        CurrState = TransitionState.Entering;

        bool animatorExists = animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null;

        if (animatorExists)
        {
            animator.SetTrigger("Enter");

            float runTime = 0f;
            while (!isEnterAnimationFinished && runTime < animationTimeLimit)
            {
                runTime += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // --- 2. TELA DE LOADING & FLAVOR EFFECTS ---
        CurrState = TransitionState.Loading;

        if (loadingScreenUI != null)
            loadingScreenUI.SetActive(true);

        if (tipText != null && gameTips != null && gameTips.Length > 0)
        {
            tipText.text = gameTips[Random.Range(0, gameTips.Length)];
        }

        if (loadingText != null)
        {
            rainbowCoroutine = StartCoroutine(AnimateRainbowText());
            dotsCoroutine = StartCoroutine(AnimateLoadingDots());
        }

        // Low gives the smoothest UI frame rates during loading
        Application.backgroundLoadingPriority = ThreadPriority.Low; 

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float timer = 0f;

        while (!op.isDone)
        {
            timer += Time.unscaledDeltaTime;

            float targetProgress = Mathf.Clamp01(op.progress / 0.9f);

            if (progressBar != null)
            {
                progressBar.fillAmount = targetProgress;
            }

            if (op.progress >= 0.9f && timer >= minLoadingTime)
            {
                op.allowSceneActivation = true;
            }

            yield return null;
        }

        // Stop loading text animations before switching active state
        if (rainbowCoroutine != null) StopCoroutine(rainbowCoroutine);
        if (dotsCoroutine != null) StopCoroutine(dotsCoroutine);

        if (type == TypeOfTransition.ChangeSceneAndTP)
        {
            PlayerInGameDataHolder.Instance.Teleport();
            setTransition(TypeOfTransition.ChangeScene);
        }

        if (loadingScreenUI != null)
            loadingScreenUI.SetActive(false);
        Application.backgroundLoadingPriority = ThreadPriority.Normal;
        // --- 3. ANIMAÇÃO DE SAÍDA (EXIT) ---
        if (animatorExists)
        {
            CurrState = TransitionState.Exiting;
            animator.SetTrigger("Exit");

            float runTime = 0f;
            while (!isExitAnimationFinished && runTime < animationTimeLimit)
            {
                runTime += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        CurrState = TransitionState.Stoped;
    }

    private IEnumerator AnimateRainbowText()
    {
        float hue = 0f;
        while (true)
        {
            hue = (hue + Time.unscaledDeltaTime * rainbowSpeed) % 1.0f;
            loadingText.color = Color.HSVToRGB(hue, 0.8f, 1.0f);
            yield return null;
        }
    }

    private IEnumerator AnimateLoadingDots()
    {
        int dotCount = 0;
        string baseText = "LOADING";

        while (true)
        {
            string dots = new string('.', dotCount);
            loadingText.text = baseText + dots;

            dotCount = (dotCount + 1) % 4;
            yield return new WaitForSecondsRealtime(dotsInterval);
        }
    }

    public void OnEnterAnimationComplete() => isEnterAnimationFinished = true;
    public void OnExitAnimationComplete() => isExitAnimationFinished = true;

    public TransitionState state => CurrState;

    public void setTransition(TypeOfTransition type)
    {
        this.type = type;
    }
}