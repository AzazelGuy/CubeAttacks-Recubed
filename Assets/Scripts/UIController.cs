using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    
    [SerializeField] private PlayerData playerData;
    [SerializeField] private TextMeshProUGUI DebugBullets;
    [SerializeField] private TextMeshProUGUI lifeAmnt;
    [SerializeField] private Image HealthBarFill;
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
        }
    }
    private void Update()
    {

        DebugBullets.text = $"Arma: {weaponName}\n {bulletscurr}/{bulletsMax}";
        HealthBarFill.fillAmount = (playerData.playerHealthCurrent/playerData.playerHealthMax);
        lifeAmnt.text = (Mathf.Ceil(playerData.playerHealthCurrent)).ToString();
        foreach (var item in hurtImage)
        {
            Color itemColor = item.color;
            itemColor.a = (0.5f - (HealthBarFill.fillAmount * 0.5f));
            item.color = itemColor;
        }
    }

    public void getWeaponInfo(string name, int bulletMax, int bulletcurr)
    {
        weaponName = name;
        bulletsMax = bulletMax;
        bulletscurr = bulletcurr;
    }
}
