using UnityEngine;

public class ReScoutWeapon : MonoBehaviour
{

    [Header("Gun Configurations")]
    public Transform gunPivot;
    public Transform canoDaArma; // Crie um Empty Object na ponta da arma e arraste aqui
    public SpriteRenderer gunsprite;

    private Vector2 AplicarImprecisao(Vector2 direcaoOriginal, float anguloImprecisao)
    {
        if (anguloImprecisao <= 0) return direcaoOriginal;

        float desvioAleatorio = Random.Range(-anguloImprecisao, anguloImprecisao);
        return Quaternion.Euler(0, 0, desvioAleatorio) * direcaoOriginal;
    }

    public void AimAt(Vector2 direction)
    {
        if (gunPivot == null || direction.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        gunPivot.rotation = Quaternion.Euler(0f, 0f, angle);

        Vector3 scale = gunPivot.localScale;
        scale.y = (direction.x < 0f) ? -1f : 1f;
        gunPivot.localScale = scale;
    }
    public void Shoot(WeaponData arma, Vector2 direction, int shotCount = 1)
    {
        if (arma == null || arma.bulletPrefab == null || canoDaArma == null || direction.sqrMagnitude < 0.0001f) return;

        AimAt(direction);

        for (int i = 0; i < shotCount; i++)
        {
            Vector2 shotDirection = AplicarImprecisao(direction, arma.imprecisao + 25f);
            float angle = Mathf.Atan2(shotDirection.y, shotDirection.x) * Mathf.Rad2Deg;
            GameObject bullet = Instantiate(arma.bulletPrefab, canoDaArma.position, Quaternion.Euler(0f, 0f, angle));

            EnemyBulletProjectile projectile = bullet.GetComponent<EnemyBulletProjectile>();
            if (projectile != null)
            {
                projectile.Initialize(shotDirection, arma.bulletDamage);
                projectile.SetEnemy(gameObject);
                Debug.Log(projectile.GetEnemy);
            }
        }
    }
}
