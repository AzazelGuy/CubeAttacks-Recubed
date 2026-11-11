using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExtraParam
{
    [Tooltip("Nome EXATO do parâmetro no Animator Controller")]
    public string name;

    [Tooltip("Tipo do parâmetro (precisa ser igual ao do Controller)")]
    public AnimatorControllerParameterType type = AnimatorControllerParameterType.Trigger;
}

public class PlayerAnimExtras : MonoBehaviour
{
    

    [Header("Referências")]
    [Tooltip("Animator do objeto filho dos sprites. Se vazio, procura nos filhos.")]
    [SerializeField] Animator animator;

    [Header("Parâmetros extras deste personagem")]
    [SerializeField] List<ExtraParam> extras = new List<ExtraParam>();
    
    public void playAnim(string name)
    {
        ExtraParam animtoPlay = null;
        foreach (ExtraParam extra in extras)
        {
            if (extra.name == name){
                animtoPlay = extra;
            }
        }

        if (animtoPlay != null)
        {
            switch (animtoPlay.type) {
                case AnimatorControllerParameterType.Trigger:
                    animator.SetTrigger(animtoPlay.name);
                    break;
            }
        }
    }
}
