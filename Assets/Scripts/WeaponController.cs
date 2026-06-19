using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    #region Inicialização de Variaveis
    private float xDir, yDir;

    [Header("Gun Configurations")]
    public Transform gunPivot;
    public Transform canoDaArma; // Crie um Empty Object na ponta da arma e arraste aqui
    public SpriteRenderer gunsprite;


    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private PlayerMovement playerMovement;

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
        UpdateWeaponAppearance();
    }

    private void Update()
    {
        xDir = Input.GetAxisRaw("Horizontal");
        yDir = Input.GetAxisRaw("Vertical");

        GunRotation(xDir, yDir);
        handleFire();
        handleSwap();

        // Evita erro se o arsenal começar vazio por algum motivo
        if (Arsenal.Count > 0)
        {
            UIController.Instance.getWeaponInfo(Arsenal[currentWeapon].WeaponName, Arsenal[currentWeapon].BulletsMax, bulletAmmount[currentWeapon]);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (currAim == FireMode._8directional){ currAim = FireMode.mouseAim; } else { currAim = FireMode._8directional; }
        }
    }

    // Lógica para rotacionar/mirar a arma em 8 direções
    private void GunRotation(float dirX, float dirY)
    {
        switch (currAim)
        {
            case FireMode._8directional:
                Vector2 direction = new Vector2(dirX, dirY);

                if (direction != Vector2.zero)
                {
                    // CORREÇÃO: Calcula o ângulo baseado no input do jogador (teclado/analógico)
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    float smoothAngle = Mathf.MoveTowardsAngle(
                        gunPivot.eulerAngles.z, angle, rotateSpeed * Time.deltaTime);
                    gunPivot.rotation = Quaternion.Euler(0, 0, smoothAngle);

                    // Flip Y baseado para onde o analógico/teclado está apontando
                    Vector3 scale = gunPivot.localScale;
                    scale.y = (direction.x < 0) ? -1f : 1f;
                    gunPivot.localScale = scale;
                }
                else
                {
                    // Se não há input de mira, a arma apenas segue para onde o player está olhando de forma reta
                    int dir = playerMovement.isFlipped() ? -1 : 1;

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
                    int dir = playerMovement.isFlipped() ? -1 : 1;

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
        // Define se o gatilho responde a "segurar o botão" (automática) ou "clicar" (semi)
        if (currAim == FireMode._8directional) {querAtirar = armaAtual.isAutomatic ? Input.GetKey(KeyCode.H) : Input.GetKeyDown(KeyCode.H); }
        if (currAim == FireMode.mouseAim) { querAtirar = armaAtual.isAutomatic ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0); }

        // Só atira se: Apertou o botão E o cooldown passou E tem munição
        if (querAtirar && Time.time >= nextFireTime && bulletAmmount[currentWeapon] > 0)
        {
            nextFireTime = Time.time + armaAtual.coolDownTime; // Aplica o Cooldown
            Atirar(armaAtual);
        }
    }

    private void Atirar(WeaponData arma)
    {
        if (arma.firingMethod == WeaponData.firingType.Projectile)
        {
            bulletAmmount[currentWeapon]--;
            if (arma.bulletPrefab != null)
            {
                Instantiate(arma.bulletPrefab, canoDaArma.position, canoDaArma.rotation);
            }
            return;
        }

        // Armas automáticas pegam a imprecisão do WeaponData
        float erroAtual = arma.imprecisao;

        switch (arma.firingMethod)
        {
            case WeaponData.firingType.Single:
                if (bulletAmmount[currentWeapon] >= 1)
                {
                    bulletAmmount[currentWeapon]--;
                    // Aplica imprecisão no tiro único (Rifle)
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

    // Função matemática para desviar levemente o tiro
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
            hit.collider.GetComponent<EnemyBase>()?.onHit(dano);
        }

        // VISUAL: Agora nós criamos um rastro independente para CADA raio
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

    // Coroutine que cuida do rastro individual e depois destrói o objeto para não poluir a cena
    IEnumerator AnimarERestruirRastro(LineRenderer linha, Vector3 inicio, Vector3 fim)
    {
        linha.positionCount = 2;
        linha.SetPosition(0, inicio);
        linha.SetPosition(1, fim);
        linha.enabled = true;

        yield return new WaitForSeconds(0.1f);

        // Destrói o objeto do rastro após o tempo passar
        Destroy(linha.gameObject);
    }

    // Cuidar da troca de armas
    private void handleSwap()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentWeapon < lenghtArsenal)
            {
                currentWeapon++;
                UpdateWeaponAppearance();
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (currentWeapon > 0)
            {
                currentWeapon--;
                UpdateWeaponAppearance();
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
}