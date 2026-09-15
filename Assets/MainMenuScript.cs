using UnityEngine;
using UnityEngine.UI;
public class MainMenuScript : MonoBehaviour
{
    [SerializeField] private GameObject SettingsMenu;
    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        TransitionScene.Instance.setTransition(TypeOfTransition.ChangeScene);
        TransitionScene.Instance.LoadSceneWithTransition("HubWorld");
        Button[] buttons = GetComponentsInChildren<Button>();
            foreach (Button btn in buttons)
            {
                btn.OnDeselect(null); // Force deselect state
            }
    }
    public void ShowSettings(bool activate)
    {
        SettingsMenu.SetActive(activate);
        Button[] buttons = GetComponentsInChildren<Button>();
            foreach (Button btn in buttons)
            {
                btn.OnDeselect(null); // Force deselect state
            }
    }
}
