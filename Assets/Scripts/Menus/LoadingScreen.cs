using UnityEngine;

public class LoadingScreen : MonoBehaviour
{

    public void Start()
    {
        TransitionScene.Instance.setTransition(TypeOfTransition.ChangeScene);
        TransitionScene.Instance.LoadSceneWithTransition("Logos");
    }
}