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

        // Se o pai do Pivot estiver com a escala X negativa (virado para a esquerda),
        // precisamos inverter a direção horizontal no cálculo do ângulo para anular a inversão da escala.
        bool parentIsFlipped = gunPivot.lossyScale.x < 0;
        Vector2 adjustedDirection = direction;

        if (parentIsFlipped)
        {
            adjustedDirection.x = -adjustedDirection.x;
            adjustedDirection.y = -adjustedDirection.y; // Mantém a proporção ao inverter
        }

        float angle = Mathf.Atan2(adjustedDirection.y, adjustedDirection.x) * Mathf.Rad2Deg;
        gunPivot.localRotation = Quaternion.Euler(0f, 0f, angle);

        // Mantém a escala local da arma limpa sem multiplicar eixos negativos
        gunPivot.localScale = Vector3.one;
    }
    public void Shoot(WeaponData arma, Vector2 direction, int shotCount = 1)
    {
        if (arma == null || arma.bulletPrefab == null || canoDaArma == null || direction.sqrMagnitude < 0.0001f) return;

        AimAt(direction);

        for (int i = 0; i < shotCount; i++)
        {
            Vector2 shotDirection = AplicarImprecisao(direction, arma.imprecisao + 10f);
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
