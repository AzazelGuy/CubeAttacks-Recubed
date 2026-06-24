using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionLogic : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] HealthControlPlayer hpContrl;
    [SerializeField] private WeaponController weaponController;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.CompareTag("PickUp"))
            {
                weaponController.RecarregarArmas();
                Destroy(collision.gameObject);
            }

        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("DamageBoxEnemy"))
            {
                EnemyBase inimigo = collision.GetComponentInParent<EnemyBase>();
                if (inimigo != null)
                {
                    playerData.playerHealthCurrent -= inimigo.AttackPlayer();
                    hpContrl.ResetTimer();
                }
            }
    }
}
