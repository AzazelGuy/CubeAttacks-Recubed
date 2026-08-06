using UnityEngine;

public class EnemyBulletProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float travelSpeed = 12f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float aliveTime = 5f;
    [SerializeField] private float ignoreWallsTimer = 1f;
    private Vector2 moveDirection = Vector2.left;
    
    private GameObject EnemyOrigin;
    private bool isInitialized;

    private void Update()
    {
        if (!isInitialized)
            return;

        transform.Translate(moveDirection * travelSpeed * Time.deltaTime, Space.World);
        aliveTime -= Time.deltaTime;
        ignoreWallsTimer -= Time.deltaTime;
        if (aliveTime <= 0) Destroy(gameObject);
    }

    public void Initialize(Vector2 direction, float projectileDamage, float projectileSpeed)
    {
        moveDirection = direction.normalized;
        damage = projectileDamage;
        travelSpeed = projectileSpeed;
        isInitialized = true;

        if (moveDirection.x < 0f)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (ignoreWallsTimer > 0) return;
        Destroy(gameObject);
    }
    public void Initialize(Vector2 direction, float projectileDamage)
    {
        Initialize(direction, projectileDamage, travelSpeed);
    }

    public float AttackPlayer()
    {
        return damage;
    }

    public float GetDamage()
    {
        return damage;
    }

    public void SetEnemy(GameObject entity)
    {
        EnemyOrigin = entity;
    }

    public GameObject GetEnemy => EnemyOrigin;
}
