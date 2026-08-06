using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    public enum PickupType { RefillAll, RefillCurrentSlot, RefillSpecificIndex }
    
    [Header("Pickup Settings")]
    public PickupType type = PickupType.RefillAll;
    public int specificSlotIndex = 0;
    public int ammoAmount = 50; // Use a high number or leave for full refill

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        // Get the weapon controller on the player prefab
        WeaponControllerPlayer weaponController = other.GetComponentInChildren<WeaponControllerPlayer>();

        if (weaponController != null && PlayerInGameDataHolder.Instance != null)
        {
            // Pass the active Arsenal list directly from the player to the Data Holder!
            PlayerInGameDataHolder.Instance.RefillAllAmmo(PlayerInGameDataHolder.Instance.Arsenal);

            Destroy(gameObject);
        }
    }
}
}