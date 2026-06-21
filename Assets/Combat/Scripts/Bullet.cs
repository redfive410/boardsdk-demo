using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int ownerIndex;
    public float gravityScale = 9.81f;

    [SerializeField] private GameObject impactEffectPrefab;

    private Vector2 velocity;

    public void Launch(Vector2 dir, float speed)
    {
        velocity = dir.normalized * speed;
        RotateToVelocity();
        Destroy(gameObject, 4f);
    }

    private void RotateToVelocity()
    {
        if (velocity.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(velocity.x, velocity.y) * Mathf.Rad2Deg);
    }

    private void Update()
    {
        velocity += Vector2.down * gravityScale * Time.deltaTime;
        transform.Translate((Vector3)velocity * Time.deltaTime, Space.World);
        RotateToVelocity();

        Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
        if (vp.x < 0 || vp.x > 1 || vp.y < 0 || vp.y > 1)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Bullet] hit {other.gameObject.name} tag={other.tag}");
        var health = other.GetComponentInParent<ShipHealth>();
        if (health == null || health.playerIndex == ownerIndex) return;

        if (other.CompareTag("Shield"))
            health.HitShield();
        else if (other.CompareTag("Ship"))
            health.HitShip();

        if (impactEffectPrefab != null)
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
