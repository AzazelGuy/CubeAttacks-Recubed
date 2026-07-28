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
    public List<WeaponData> Arsenal;
    private int lenghtArsenal;

    private int[] bulletAmmount;
    private int currentWeapon;
    private float nextFireTime; // Controla o cooldown entre os tiros

    [Header("Efeitos Visuais")]
    public GameObject rastroPrefab; // Crie um Prefab que contenha APENAS o componente LineRenderer
    #endregion

    private void Start()
    {
        lenghtArsenal = Arsenal.Count - 1;
        Debug.Log(lenghtArsenal);

        bulletAmmount = new int[Arsenal.Count];
        for (int i = 0; i < bulletAmmount.Length; i++)
        {
            bulletAmmount[i] = Arsenal[i].BulletsMax;
        }

        // Come�a com a arma guardada
        ResetarAlerta(false);
        UpdateWeaponAppearance();
    }

    private void Update()
    {
        if (PauseMenuActions.instance.Paused()) return;
        xDir = Input.GetAxisRaw("Horizontal");
        yDir = Input.GetAxisRaw("Vertical");


        GunRotation(xDir, yDir);
        handleFire();
        handleSwap();
        HandleAlertTimer(); // L�gica para contar o tempo do coldre

        // Evita erro se o arsenal come�ar vazio por algum motivo
        if (Arsenal.Count > 0)
        {
            UIControllerInGame.Instance.getWeaponInfo(Arsenal[currentWeapon].WeaponName, Arsenal[currentWeapon].BulletsMax, bulletAmmount[currentWeapon]);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (currAim == FireMode._8directional) { currAim = FireMode.mouseAim; } else { currAim = FireMode._8directional; }
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
    private void handleFire()
    {
        if (Arsenal.Count == 0) return;

        WeaponData armaAtual = Arsenal[currentWeapon];
        bool querAtirar = false;
        if (currAim == FireMode._8directional) { querAtirar = armaAtual.isAutomatic ? Input.GetKey(KeyCode.H) : Input.GetKeyDown(KeyCode.H); }
        if (currAim == FireMode.mouseAim) { querAtirar = armaAtual.isAutomatic ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0); }

        // Se o player apertar para atirar, a arma saca imediatamente (mesmo se estiver sem balas ou em cooldown)
        if (querAtirar)
        {
            ResetarAlerta(true);
        }

        // S� atira se: Apertou o bot�o E o cooldown passou E tem muni��o
        if (querAtirar && Time.time >= nextFireTime && bulletAmmount[currentWeapon] > 0)
        {
            nextFireTime = Time.time + armaAtual.coolDownTime; // Aplica o Cooldown
            Atirar(armaAtual);
        }
    }

    private void Atirar(WeaponData arma)
    {
        // Garante que o timer vai renovar para 5 segundos ao efetivar o tiro
        ResetarAlerta(true);

        // --- SISTEMA DE MOVIMENTO POR RECUO ---
        if (playerMovement != null && arma.recoilStrenght > 0)
        {
            // canoDaArma.right � para onde o tiro vai. Negativo (-) joga o jogador para tr�s.
            Vector2 direcaoEmpurrao = -canoDaArma.right;

            // Se a for�a do Scriptable Object for baseada no �ngulo visual (ex: 5, 10, 15), 
            // multiplicamos por um fator menor (ex: 0.3f) para n�o isolar o quadrado da tela.
            float intensidadeForca = arma.recoilStrenght * 0.3f;
            Vector2 vetorForcaFinal = direcaoEmpurrao * intensidadeForca;

            // Tempo em segundos que o jogador perde o controle do teclado para o recuo agir (0.15s � �timo)
            float tempoBloqueioTeclado = 0.2f;

            playerMovement.AplicarEmpurraoRecuo(vetorForcaFinal, tempoBloqueioTeclado);
        }

        if (arma.firingMethod == WeaponData.firingType.Projectile)
        {
            bulletAmmount[currentWeapon]--;
            if (arma.bulletPrefab != null)
            {
                Instantiate(arma.bulletPrefab, canoDaArma.position, canoDaArma.rotation);
            }
            return;
        }

        float erroAtual = arma.imprecisao;

        switch (arma.firingMethod)
        {
            case WeaponData.firingType.Single:
                if (bulletAmmount[currentWeapon] >= 1)
                {
                    bulletAmmount[currentWeapon]--;
                    Vector2 dirComImprecisao = AplicarImprecisao(canoDaArma.right, erroAtual);
                    ExecutarRaycast(dirComImprecisao, arma.bulletDamage);
                }
                break;

            case WeaponData.firingType.Double:
                if (bulletAmmount[currentWeapon] >= 2)
                {
                    bulletAmmount[currentWeapon] -= 2;
                    ExecutarRaycast(AplicarImprecisao(canoDaArma.right, erroAtual), arma.bulletDamage);
                    ExecutarRaycast(AplicarImprecisao(canoDaArma.right, erroAtual), arma.bulletDamage);
                }
                break;

            case WeaponData.firingType.Spread:
                if (bulletAmmount[currentWeapon] >= 1)
                {
                    bulletAmmount[currentWeapon]--;
                    for (int i = 0; i <=8; i++)
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
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentWeapon < lenghtArsenal)
            {
                currentWeapon++;
                UpdateWeaponAppearance();
                ResetarAlerta(true); // Mostra a arma nova que acabou de puxar
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (currentWeapon > 0)
            {
                currentWeapon--;
                UpdateWeaponAppearance();
                ResetarAlerta(true); // Mostra a arma nova que acabou de puxar
            }
        }
    }

    private void UpdateWeaponAppearance()
    {
        if (Arsenal.Count > 0)
        {
            gunsprite.sprite = Arsenal[currentWeapon].WeaponImage;
        }
    }

    public void RecarregarArmas()
    {
        for (int i = 0; i < bulletAmmount.Length; i++)
        {
            bulletAmmount[i] = Arsenal[i].BulletsMax;
        }

        Debug.Log("Muni��o recarregada!");
    }
}