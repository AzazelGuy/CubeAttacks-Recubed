using UnityEngine;

public class EnemyBulletProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float travelSpeed = 12f;
    [SerializeField] private float damage = 10f;

    private Vector2 moveDirection = Vector2.left;
    private bool isInitialized;

    private void Update()
    {
        //if (!isInitialized)
            //return;

        transform.Translate(moveDirection * travelSpeed * Time.deltaTime, Space.World);
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

    public void Initialize(Vector2 direction, float projectileDamage)
    {
        Initialize(direction, projectileDamage, travelSpeed);
    }

    public float GetDamage()
    {
        return damage;
    }
}
