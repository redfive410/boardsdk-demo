using UnityEngine;
using Board.Input;

public class ShipController : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.grey;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 0.4f;
    [SerializeField] private PlayerDashboard dashboard;

    private float fireCooldown;
    private ShipHealth health;

    private void Awake()
    {
        health = GetComponent<ShipHealth>();
    }

    public void ApplyContact(BoardContact? contact)
    {
        if (!contact.HasValue || contact.Value.isNoneEndedOrCanceled)
        {
            spriteRenderer.color = inactiveColor;
            health?.SetShipVisible(false);
            return;
        }

        health?.SetShipVisible(true);

        var c = contact.Value;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(c.screenPosition.x, c.screenPosition.y, 10f)
        );
        transform.position = worldPos;
        transform.rotation = Quaternion.Euler(0f, 0f, c.orientation * Mathf.Rad2Deg);
        spriteRenderer.color = c.isTouched ? activeColor : inactiveColor;

        bool shouldFire = dashboard != null && dashboard.FirePressed;
        fireCooldown -= Time.deltaTime;
        if (shouldFire && fireCooldown <= 0f && bulletPrefab != null)
        {
            fireCooldown = fireRate;
            float speed = dashboard != null ? dashboard.BulletSpeed : 8f;
            var bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            var b = bullet.GetComponent<Bullet>();
            b.ownerIndex = playerIndex;
            b.Launch(-transform.up, speed);
        }
    }
}
