using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int ownerIndex;
    public float gravityScale = 9.81f;

    private Vector2 velocity;

    public void Launch(Vector2 dir, float speed)
    {
        velocity = dir.normalized * speed;
        Destroy(gameObject, 4f);
    }

    private void Update()
    {
        velocity += Vector2.down * gravityScale * Time.deltaTime;
        transform.Translate((Vector3)velocity * Time.deltaTime, Space.World);

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

        Destroy(gameObject);
    }
}
