using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIControllerInGame : MonoBehaviour
{
    public static UIControllerInGame Instance;
    
    [SerializeField] private TextMeshProUGUI BulletAmount;
    [SerializeField] private Image fillImageBullet;
    [SerializeField] private Image fillImageBulletGhost;
    [SerializeField] private float fillGhostHoldDuration = 0.5f;
    [SerializeField] private float fillGhostCatchupSpeed = 1f;
    [SerializeField] private TextMeshProUGUI WeaponName;
    [SerializeField] private Image fillImageHealth;
    
    [SerializeField] private TextMeshProUGUI TexthealthAmnt;
    private Image fillImageHealthGhost;
    private float healthFillGhostTimer;
    private float bulletFillGhostTimer;
    private float lastHealthTargetFill;
    private float lastAmmoTargetFill;
    private bool healthTargetInitialized;
    private bool ammoTargetInitialized;
    [SerializeField] private CanvasGroup hurtImage;
    [Tooltip("A ordem do array afeta sim o sistema todo, fique atento")]
    [SerializeField] private RectTransform[] WeaponPos;
    [SerializeField] private Image[] WeaponImages;
    [SerializeField] private Sprite FallBackNoWeapon;

    private int weaponSlot = 0;
    private string weaponName;
    private int bulletsMax;
    private int bulletscurr;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        fillImageHealthGhost = CreateFillGhost(fillImageHealth);
        fillImageBulletGhost = CreateFillGhost(fillImageBullet, fillImageBulletGhost);

        foreach(RectTransform s in WeaponPos)
        {
            WeaponImages[weaponSlot].rectTransform.position = s.position;
            weaponSlot ++;
        }
        weaponSlot = 0;
    }
    private void Update()
    {
        BulletAmount.text = $"{bulletscurr}/{bulletsMax}";
        WeaponName.text = $"{weaponName}";
        UpdateFillGhost(fillImageHealth, fillImageHealthGhost, ref healthFillGhostTimer);
        UpdateFillGhost(fillImageBullet, fillImageBulletGhost, ref bulletFillGhostTimer);
        if (SceneManager.GetActiveScene().name == "HubWorld" || SceneManager.GetActiveScene().name == "MainMenu")
        {
            Instance = null;
            Destroy(gameObject);
        }

        if (WeaponPos != null && WeaponImages != null)
        {
           for(int i = 0; i < WeaponPos.Length; i++)
            {
            WeaponImages[i].rectTransform.position = Vector2.Lerp(WeaponImages[i].rectTransform.position,WeaponPos[(i - weaponSlot + WeaponPos.Length) % WeaponPos.Length].position,0.4f); //O 0.4 ainda é placeholder, depois coloco em um slider limitado ajustavel
            } 
        }
        
        
    }
    public void SetHealthEffect(float currentAmnt, float maxAmnt)
    {
        float targetFill = maxAmnt > 0f ? Mathf.Clamp01(currentAmnt / maxAmnt) : 0f;

        if (healthTargetInitialized)
        {
            if (targetFill < lastHealthTargetFill)
            {
                HoldFillGhost(fillImageHealth, fillImageHealthGhost, ref healthFillGhostTimer);
            }
            else if (targetFill > lastHealthTargetFill)
            {
                ClearFillGhost(fillImageHealth, fillImageHealthGhost, ref healthFillGhostTimer);
            }
        }

        lastHealthTargetFill = targetFill;
        healthTargetInitialized = true;

        hurtImage.alpha = 1f - targetFill;
        
    }    
    public void SetHealthAmount(float currentAmount)
    {
        if (TexthealthAmnt == null) return;

        TexthealthAmnt.text = $"{Mathf.CeilToInt(currentAmount)}";
    }

    public void getWeaponInfo(string name = "None", int bulletMax = 0, int bulletcurr = 0)
    {
        float targetFill = bulletMax > 0 ? Mathf.Clamp01((float)bulletcurr / bulletMax) : 0f;
        bool weaponChanged = name != weaponName || bulletMax != bulletsMax;

        if (ammoTargetInitialized && !weaponChanged)
        {
            if (targetFill < lastAmmoTargetFill)
            {
                HoldFillGhost(fillImageBullet, fillImageBulletGhost, ref bulletFillGhostTimer);
            }
            else if (targetFill > lastAmmoTargetFill)
            {
                ClearFillGhost(fillImageBullet, fillImageBulletGhost, ref bulletFillGhostTimer);
            }
        }
        else
        {
            ClearFillGhost(fillImageBullet, fillImageBulletGhost, ref bulletFillGhostTimer);
        }

        lastAmmoTargetFill = targetFill;
        ammoTargetInitialized = true;
        weaponName = name;
        bulletsMax = bulletMax;
        bulletscurr = bulletcurr;
        if (fillImageBullet != null)
        {
            fillImageBullet.fillAmount = Mathf.Lerp(fillImageBullet.fillAmount, targetFill, 0.5f);
        }
    }

    private Image CreateFillGhost(Image source, Image ghost = null)
    {
        if (source == null) return null;

        if (ghost == null)
        {
            ghost = Instantiate(source, source.transform.parent);
            ghost.gameObject.name = $"{source.gameObject.name}Ghost";
        }

        if (ghost.transform.parent == source.transform.parent)
        {
            ghost.transform.SetSiblingIndex(source.transform.GetSiblingIndex());
        }

        ghost.fillAmount = source.fillAmount;
        ghost.raycastTarget = false;
        Color ghostColor = ghost.color;
        ghostColor.a *= 0.5f;
        ghost.color = ghostColor;
        return ghost;
    }

    private void HoldFillGhost(Image fill, Image ghost, ref float timer)
    {
        if (fill == null || ghost == null) return;

        ghost.fillAmount = Mathf.Max(ghost.fillAmount, fill.fillAmount);
        timer = Mathf.Max(0f, fillGhostHoldDuration);
    }

    private void UpdateFillGhost(Image fill, Image ghost, ref float timer)
    {
        if (fill == null || ghost == null) return;

        if (timer > 0f)
        {
            timer = Mathf.Max(0f, timer - Time.deltaTime);
            return;
        }

        ghost.fillAmount = Mathf.MoveTowards(ghost.fillAmount, fill.fillAmount, fillGhostCatchupSpeed * Time.deltaTime);
    }

    private void ClearFillGhost(Image fill, Image ghost, ref float timer)
    {
        timer = 0f;
        if (fill != null && ghost != null) ghost.fillAmount = fill.fillAmount;
    }

    public void SpinWheel(int index, int size)
    {
        weaponSlot = index % size;

    }

    public void UpdateImages(Sprite desiredImage = null, int index = 0)
    {
        if (desiredImage == null)
        {
            WeaponImages[index].sprite = FallBackNoWeapon;
            return;
        }
        WeaponImages[index].sprite = desiredImage;
        return;
    }
    public Image Healthfill => fillImageHealth;

    public TextMeshProUGUI TextHealth => TexthealthAmnt;
}
