using UnityEngine;

public class PyromaniacFire : MonoBehaviour
{
    private float aliveTimer = 0f;

    [SerializeField] private float aliveTime = 15f;

    [SerializeField] private float damage = 5f;

    [SerializeField] private float tickDamageTime = 0.5f;
    private float tickDamageTimer = 0f;

    void Start()
    {
        aliveTimer = aliveTime;
    }
    void Update()
    {
        aliveTimer -= Time.deltaTime;
        tickDamageTimer -= Time.deltaTime;


        if (aliveTimer <= 0) Destroy(gameObject);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("DamageBoxEnemy"))
        {
            if (tickDamageTimer <= 0)
            {
                if (collision.GetComponentInParent<EnemyBase>() != null)
                {
                    collision.GetComponentInParent<EnemyBase>().OnHit(damage);
                    tickDamageTimer = tickDamageTime;
                }
            }
        }
    }
}
