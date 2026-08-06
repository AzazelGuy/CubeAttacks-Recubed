using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class WeaponControllerPlayer : MonoBehaviour
{
    #region Inicializa��o de Variaveis
    private float xDir, yDir;

    [Header("Gun Configurations")]
    public Transform gunPivot;
    public Transform canoDaArma; // Crie um Empty Object na ponta da arma e arraste aqui
    public SpriteRenderer gunsprite;
    [SerializeField] private Rigidbody2D playerRb; // Arraste o Rigidbody2D do seu Player aqui no Inspector

    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private PlayerMovement playerMovement;

    // --- VARI�VEIS DE ALERTA/ISQUEIRO DA ARMA ---
    [HideInInspector] public bool alert = false;
    private float alertTimer;
    [SerializeField] private float alertTime = 5f; // Ajustado para 5 segundos conforme pediu

    private enum FireMode
    {
        _8directional,
        mouseAim
    }

    [SerializeField] private FireMode currAim = FireMode._8directional;

    //Inventario de armas (maximo 3 em jogo)
    [Header("Armas")]
    
    private int currentWeapon;
    private float nextFireTime; // Controla o cooldown entre os tiros

    [Header("Efeitos Visuais")]
    public GameObject rastroPrefab; // Crie um Prefab que contenha APENAS o componente LineRenderer
    #endregion

    private void Start()
{
    // GUARD 1: If player doesn't use weapons, shut down this script's visual features
    if (PlayerInGameDataHolder.Instance != null && !PlayerInGameDataHolder.Instance.playerCurrData.usesWeapon)
    {
        if (gunsprite != null) gunsprite.enabled = false;
        this.enabled = false; // Disable update loop for melee characters!
        return;
    }

    // Initialize persistent ammo tracker inside the Singleton Manager
    if (PlayerInGameDataHolder.Instance != null && PlayerInGameDataHolder.Instance.Arsenal.Count > 0)
    {
        PlayerInGameDataHolder.Instance.InitializeAmmo(PlayerInGameDataHolder.Instance.Arsenal);
    }

    ResetarAlerta(false);
    UpdateWeaponAppearance();
}

    private void Update()
{
    if (PauseMenuActions.instance != null && PauseMenuActions.instance.Paused) return;

    xDir = Input.GetAxisRaw("Horizontal");
    yDir = Input.GetAxisRaw("Vertical");

    GunRotation(xDir, yDir);
    handleFire();
    handleSwap();
    HandleAlertTimer();

    // UPDATE UI SAFELY
    if (PlayerInGameDataHolder.Instance.Arsenal.Count > 0 && PlayerInGameDataHolder.Instance != null)
    {
        int currentAmmo = PlayerInGameDataHolder.Instance.GetAmmo(currentWeapon);
        UIControllerInGame.Instance.getWeaponInfo(
            PlayerInGameDataHolder.Instance.Arsenal[currentWeapon].WeaponName, 
            PlayerInGameDataHolder.Instance.Arsenal[currentWeapon].BulletsMax, 
            currentAmmo
        );
    }

    if (Input.GetKeyDown(KeyCode.Tab))
    {
        currAim = (currAim == FireMode._8directional) ? FireMode.mouseAim : FireMode._8directional;
    }
}

    // Gerencia o tempo que a arma fica exposta
    private void HandleAlertTimer()
    {
        if (alert)
        {
            alertTimer -= Time.deltaTime;
            if (alertTimer <= 0)
            {
                ResetarAlerta(false); // Esconde a arma
            }
        }
    }

    public bool isMouseAim()
    {
        return currAim == FireMode.mouseAim;
    }
    // Fun��o auxiliar para ativar/desativar a arma visualmente
    private void ResetarAlerta(bool estaAtivo)
    {
        alert = estaAtivo;
        gunsprite.enabled = estaAtivo; // Liga ou desliga o sprite na tela

        if (estaAtivo)
        {
            alertTimer = alertTime; // Reseta o cron�metro para 5 segundos
        }
    }

    // L�gica para rotacionar/mirar a arma em 8 dire��es
    private void GunRotation(float dirX, float dirY)
    {
        switch (currAim)
        {
            case FireMode._8directional:
                Vector2 direction = new Vector2(dirX, dirY);

                if (direction != Vector2.zero)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    float smoothAngle = Mathf.MoveTowardsAngle(
                    gunPivot.eulerAngles.z, angle, rotateSpeed * Time.deltaTime);
                    gunPivot.rotation = Quaternion.Euler(0, 0, smoothAngle);

                    Vector3 scale = gunPivot.localScale;
                    scale.y = (direction.x < 0) ? -1f : 1f;
                    gunPivot.localScale = scale;
                }
                else
                {
                    int dir = playerMovement.IsFacingLeft ? -1 : 1;

                    float angle = Mathf.Atan2(0, dir) * Mathf.Rad2Deg;
                    float smoothAngle = Mathf.MoveTowardsAngle(
                    gunPivot.eulerAngles.z, angle, rotateSpeed * Time.deltaTime);
                    gunPivot.rotation = Quaternion.Euler(0, 0, smoothAngle);

                    Vector3 scale = gunPivot.localScale;
                    scale.y = (dir < 0) ? -1f : 1f;
                    gunPivot.localScale = scale;
                }
                break;

            case FireMode.mouseAim:
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPosition.z = 0f;

                Vector2 mdirection = mouseWorldPosition - gunPivot.position;

                if (mdirection.sqrMagnitude > 0.01f)
                {
                    float angle = Mathf.Atan2(mdirection.y, mdirection.x) * Mathf.Rad2Deg;

                    float smoothAngle = Mathf.MoveTowardsAngle(
                        gunPivot.eulerAngles.z, angle, rotateSpeed * Time.deltaTime);
                    gunPivot.rotation = Quaternion.Euler(0, 0, smoothAngle);

                    Vector3 scale = gunPivot.localScale;
                    scale.y = (mdirection.x < 0) ? -1f : 1f;
                    gunPivot.localScale = scale;
                }
                else
                {
                    int dir = playerMovement.IsFacingLeft ? -1 : 1;

                    float angle = Mathf.Atan2(0, dir) * Mathf.Rad2Deg;
                    float smoothAngle = Mathf.MoveTowardsAngle(
                        gunPivot.eulerAngles.z, angle, rotateSpeed * Time.deltaTime);
                    gunPivot.rotation = Quaternion.Euler(0, 0, smoothAngle);

                    Vector3 scale = gunPivot.localScale;
                    scale.y = (dir < 0) ? -1f : 1f;
                    gunPivot.localScale = scale;
                }
                break;
        }
    }

    // Cuidar dos tiros da arma
    // UPDATE handleFire() ammo checks to use Singleton:
private void handleFire()
{
    if (PlayerInGameDataHolder.Instance.Arsenal.Count == 0) return;

    WeaponData armaAtual = PlayerInGameDataHolder.Instance.Arsenal[currentWeapon];
    bool querAtirar = (currAim == FireMode._8directional) 
        ? (armaAtual.isAutomatic ? Input.GetKey(KeyCode.H) : Input.GetKeyDown(KeyCode.H))
        : (armaAtual.isAutomatic ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0));

    if (querAtirar) ResetarAlerta(true);

    // Read ammo from Singleton!
    int ammoAvailable = PlayerInGameDataHolder.Instance.GetAmmo(currentWeapon);

    if (querAtirar && Time.time >= nextFireTime && ammoAvailable > 0)
    {
        nextFireTime = Time.time + armaAtual.coolDownTime;
        Atirar(armaAtual);
    }
}

    // UPDATE Atirar() ammo deductions:
private void Atirar(WeaponData arma)
{
    ResetarAlerta(true);

    // Recoil Pushback
    if (playerMovement != null && arma.recoilStrenght > 0)
    {
        Vector2 direcaoEmpurrao = -canoDaArma.right;
        float intensidadeForca = arma.recoilStrenght * 0.3f;
        playerMovement.AplicarEmpurraoRecuo(direcaoEmpurrao * intensidadeForca, 0.2f);
    }

    // PROJECTILE WEAPONS
    if (arma.firingMethod == WeaponData.firingType.Projectile)
    {
        PlayerInGameDataHolder.Instance.ConsumeAmmo(currentWeapon, 1);
        if (arma.bulletPrefab != null)
        {
            Instantiate(arma.bulletPrefab, canoDaArma.position, canoDaArma.rotation);
        }
        return;
    }

    // RAYCAST WEAPONS
    float erroAtual = arma.imprecisao;
    int ammoAvailable = PlayerInGameDataHolder.Instance.GetAmmo(currentWeapon);

    switch (arma.firingMethod)
    {
        case WeaponData.firingType.Single:
            if (ammoAvailable >= 1)
            {
                PlayerInGameDataHolder.Instance.ConsumeAmmo(currentWeapon, 1);
                ExecutarRaycast(AplicarImprecisao(canoDaArma.right, erroAtual), arma.bulletDamage);
            }
            break;

        case WeaponData.firingType.Double:
            if (ammoAvailable >= 2)
            {
                PlayerInGameDataHolder.Instance.ConsumeAmmo(currentWeapon, 2);
                ExecutarRaycast(AplicarImprecisao(canoDaArma.right, erroAtual), arma.bulletDamage);
                ExecutarRaycast(AplicarImprecisao(canoDaArma.right, erroAtual), arma.bulletDamage);
            }
            break;

        case WeaponData.firingType.Spread:
            if (ammoAvailable >= 1)
            {
                PlayerInGameDataHolder.Instance.ConsumeAmmo(currentWeapon, 1);
                for (int i = 1; i <= arma.NumOfBullets; i++)
                {
                    ExecutarRaycast(AplicarImprecisao(Quaternion.Euler(0, 0, 0.5f * i) * canoDaArma.right, erroAtual), arma.bulletDamage);
                }
            }
            break;
    }
}

    private Vector2 AplicarImprecisao(Vector2 direcaoOriginal, float anguloImprecisao)
    {
        if (anguloImprecisao <= 0) return direcaoOriginal;

        float desvioAleatorio = Random.Range(-anguloImprecisao, anguloImprecisao);
        return Quaternion.Euler(0, 0, desvioAleatorio) * direcaoOriginal;
    }

    private void ExecutarRaycast(Vector2 direcao, float dano)
    {
        float alcance = 50f;
        RaycastHit2D hit = Physics2D.Raycast(canoDaArma.position, direcao, alcance, targetLayers);

        Vector3 pontoFinal = canoDaArma.position + (Vector3)direcao * alcance;

        if (hit.collider != null)
        {
            pontoFinal = hit.point;
            EnemyBase inimigo = hit.collider.GetComponentInParent<EnemyBase>();
                if (inimigo != null)
                {
                    inimigo.OnHit(dano);
                }
        }

        if (rastroPrefab != null)
        {
            GameObject rastroObjeto = Instantiate(rastroPrefab, canoDaArma.position, Quaternion.identity);
            LineRenderer linha = rastroObjeto.GetComponent<LineRenderer>();

            if (linha != null)
            {
                StartCoroutine(AnimarERestruirRastro(linha, canoDaArma.position, pontoFinal));
            }
        }
    }

    IEnumerator AnimarERestruirRastro(LineRenderer linha, Vector3 inicio, Vector3 fim)
    {
        linha.positionCount = 2;
        linha.SetPosition(0, inicio);
        linha.SetPosition(1, fim);
        linha.enabled = true;

        yield return new WaitForSeconds(0.1f);

        Destroy(linha.gameObject);
    }

    private void handleSwap()
{
    if (PlayerInGameDataHolder.Instance.Arsenal.Count <= 1) return; // Don't swap if holding 1 or 0 weapons

    int previousWeapon = currentWeapon;

    // Cycle Forward (E)
    if (Input.GetKeyDown(KeyCode.E))
    {
        currentWeapon = (currentWeapon + 1) % PlayerInGameDataHolder.Instance.Arsenal.Count;
    }
    // Cycle Backward (Q)
    else if (Input.GetKeyDown(KeyCode.Q))
    {
        currentWeapon = (currentWeapon - 1 + PlayerInGameDataHolder.Instance.Arsenal.Count) % PlayerInGameDataHolder.Instance.Arsenal.Count;
    }

    // Only run holster/sprite updates if the weapon actually changed
    if (currentWeapon != previousWeapon)
    {
        UpdateWeaponAppearance();
        ResetarAlerta(true); // Draw the newly equipped weapon immediately
    }
}

    private void UpdateWeaponAppearance()
{
    if (PlayerInGameDataHolder.Instance.Arsenal.Count == 0 || gunsprite == null) return;

    // Safety check bounds
    if (currentWeapon >= 0 && currentWeapon < PlayerInGameDataHolder.Instance.Arsenal.Count)
    {
        gunsprite.sprite = PlayerInGameDataHolder.Instance.Arsenal[currentWeapon].WeaponImage;
    }
}

}