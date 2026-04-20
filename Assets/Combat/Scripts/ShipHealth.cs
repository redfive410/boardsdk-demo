using UnityEngine;

public class ShipHealth : MonoBehaviour
{
    public int playerIndex;

    [SerializeField] private GameObject shieldRing;

    private int shieldHits = 0;
    private const int maxShieldHits = 3;
    private bool shieldActive = true;

    public void HitShield()
    {
        if (!shieldActive) return;

        shieldHits++;
        Debug.Log($"[ShipHealth {playerIndex}] shield hit {shieldHits}/{maxShieldHits}");
        if (shieldHits >= maxShieldHits)
        {
            shieldActive = false;
            if (shieldRing != null)
                shieldRing.SetActive(false);
            Debug.Log($"[ShipHealth {playerIndex}] shield destroyed");
        }
    }

    public void HitShip()
    {
        Debug.Log($"[ShipHealth {playerIndex}] HitShip called shieldActive={shieldActive} GameManager={GameManager.Instance != null}");
        if (shieldActive) return;
        GameManager.Instance.ShipDestroyed(playerIndex);
    }

    public bool ShieldActive => shieldActive;

    public void SetShipVisible(bool visible)
    {
        if (shieldRing != null && shieldActive)
            shieldRing.SetActive(visible);
    }
}
