using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private float rainbowSpeed = 0.5f;
    private bool isProcessing = false;
    [SerializeField] private float loadingtime = 5;
    private float loadingTimer = 999999f;
    private float currentHue = 0f;

    public void Start()
    {
        loadingTimer = loadingtime;
    }

    private void Update()
    {
        loadingTimer -= Time.deltaTime;
        // Continuously cycle the hue from 0.0 to 1.0
        currentHue = Mathf.Repeat(currentHue + Time.deltaTime * rainbowSpeed, 1f);
        
        // Convert HSV (Hue, Saturation, Value) to RGB
        loadingText.color = Color.HSVToRGB(currentHue, 1f, 1f);
        if (loadingTimer <= 0){ 
        if (!isProcessing){
        TransitionScene.Instance.setTransition(TypeOfTransition.ChangeScene);
        TransitionScene.Instance.LoadSceneWithTransition("Logos");}
        isProcessing = true;
        }
    }
}