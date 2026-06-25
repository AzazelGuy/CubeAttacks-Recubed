using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionLogic : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] HealthControlPlayer hpContrl;
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private float invenciTime = 0.2f;
    private float invenciTimer;

    void Update()
    {
        invenciTimer -= Time.deltaTime;
    }
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
                if (inimigo != null && invenciTimer < 0)
                {
                    invenciTimer = invenciTime;
                    playerData.playerHealthCurrent -= inimigo.AttackPlayer();
                    hpContrl.ResetTimer();
                }
            }
    }
}
