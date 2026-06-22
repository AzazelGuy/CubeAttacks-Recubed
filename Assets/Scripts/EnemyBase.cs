using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] private GameObject popUpDamage;

    [SerializeField] protected EnemyData enemyData;

    protected float hpAtual;
    protected float defesaAtual;

    protected virtual void Awake()
    {
        if (enemyData != null)
        {
            hpAtual = enemyData.hpMaximo;
            defesaAtual = enemyData.defesa;
        }
    }
    public virtual void onHit(float damage)
    {
        float critChance = .2f;
        bool chancecrit = (Random.Range(0f, 1f) <= critChance);

        if (chancecrit)
        {
            damage *= 2;
            Debug.Log("critico!");
        }
        float damageFinal = damage * (255f/(255+defesaAtual));
        hpAtual -= damageFinal;
        Debug.Log(hpAtual);
        Vector3 offset = new Vector3 (Random.Range(-1f,1f), Random.Range(-1f, 1f)); ;
        var damagepopUp = Instantiate(popUpDamage);
        damagepopUp.transform.position = transform.position + offset;
        damagepopUp.GetComponent<DamagePopUp>().Setup(damage);
        if (chancecrit) damagepopUp.GetComponent<DamagePopUp>().ONCRIT();
    }
}
