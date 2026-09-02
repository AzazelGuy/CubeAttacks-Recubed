using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoScreen : MonoBehaviour
{
    [SerializeField] private AudioClip SFXZelosStudioLogo;
    [SerializeField] private AudioClip SFXMadeWithUnity;

    public void ZelosSFX()
    {
        AudioManager.Instance.PlaySFX(SFXZelosStudioLogo);
    }
    public void MadeWithUnitySFX()
    {
        AudioManager.Instance.PlaySFXRandomPitch(SFXMadeWithUnity);
    }
    
    public void goToMain()
    {
        TransitionScene.Instance.setTransition(TypeOfTransition.ChangeScene);
        TransitionScene.Instance.LoadSceneWithTransition("MainMenu");
    }
}
