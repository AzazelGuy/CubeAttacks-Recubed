using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UIControllerInGame : MonoBehaviour
{
    public static UIControllerInGame Instance;
    
    [SerializeField] private TextMeshProUGUI BulletAmount;
    [SerializeField] private TextMeshProUGUI WeaponName;
    [SerializeField] private Image fillImageHealth;
    private Material waveMaterialHealth;
    [SerializeField] private Image fillImageShield;
    private Material waveMaterialShield;
    [SerializeField] private Image[] hurtImage;

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

        waveMaterialHealth = fillImageHealth.material;

        waveMaterialShield = fillImageShield.material;
    }
    private void Update()
    {

        BulletAmount.text = $"{bulletscurr}/{bulletsMax}";
        WeaponName.text = $"{weaponName}";

        SetFillHealth(PlayerInGameDataHolder.Instance.Health, PlayerInGameDataHolder.Instance.MaxHealth); //Health

        

        SetFillShield(PlayerInGameDataHolder.Instance.Shield, PlayerInGameDataHolder.Instance.MaxShield); //Shield
        
    }

    public void SetFillHealth(float currentAmnt, float maxAmnt)
    {
        float targetFill = currentAmnt / maxAmnt;

        foreach (var item in hurtImage)
        {

            Color itemColor = item.color;
            itemColor.a = 0.5f - (targetFill * 0.5f);
            item.color = itemColor;
        }

        waveMaterialHealth.SetFloat("_FillAmount", targetFill);
        
    }

    public void SetFillShield(float currentAmnt, float maxAmnt)
    {
        float targetFill = currentAmnt / maxAmnt;

        waveMaterialShield.SetFloat("_FillAmount", targetFill);
        
    }
    public void getWeaponInfo(string name, int bulletMax, int bulletcurr)
    {
        weaponName = name;
        bulletsMax = bulletMax;
        bulletscurr = bulletcurr;
    }

    public Image Healthfill => fillImageHealth;
    public Image Shieldfill => fillImageShield;
}
