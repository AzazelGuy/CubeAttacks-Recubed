using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private float rainbowSpeed = 0.5f;

    private float currentHue = 0f;

    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private void Update()
    {
        // Continuously cycle the hue from 0.0 to 1.0
        currentHue = Mathf.Repeat(currentHue + Time.deltaTime * rainbowSpeed, 1f);
        
        // Convert HSV (Hue, Saturation, Value) to RGB
        loadingText.color = Color.HSVToRGB(currentHue, 1f, 1f);
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}