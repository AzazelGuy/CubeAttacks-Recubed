using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UIControllerInGame : MonoBehaviour
{
    public static UIControllerInGame Instance;
    
    [SerializeField] private TextMeshProUGUI BulletAmount;
    [SerializeField] private TextMeshProUGUI WeaponName;
    [SerializeField] private Image fillImageHealth;
    [SerializeField] private Image fillImageShield;
    
    [SerializeField] private TextMeshProUGUI TexthealthAmnt;
    [SerializeField] private TextMeshProUGUI TextshieldAmnt;
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
    }
    private void Update()
    {

        BulletAmount.text = $"{bulletscurr}/{bulletsMax}";
        WeaponName.text = $"{weaponName}";
        
    }
    public void SetHealthEffect(float currentAmnt, float maxAmnt)
    {
        float targetFill = currentAmnt / maxAmnt;

        foreach (var item in hurtImage)
        {

            Color itemColor = item.color;
            itemColor.a = 0.5f - (targetFill * 0.5f);
            item.color = itemColor;
        }
        
    }    
    public void getWeaponInfo(string name, int bulletMax, int bulletcurr)
    {
        weaponName = name;
        bulletsMax = bulletMax;
        bulletscurr = bulletcurr;
    }

    public Image Healthfill => fillImageHealth;
    public Image Shieldfill => fillImageShield;

    public TextMeshProUGUI TextHealth => TexthealthAmnt;
    public TextMeshProUGUI TextShield => TextshieldAmnt;
}
