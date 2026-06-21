using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ImpactEffect : MonoBehaviour
{
    [Tooltip("Sprites cycled through in order, one per frame interval.")]
    [SerializeField] private Sprite[] frames;

    [Tooltip("Seconds each sprite is shown before advancing.")]
    [SerializeField] private float frameInterval = 0.05f;

    [Tooltip("Sorting order applied at runtime so the impact draws above ships and shields.")]
    [SerializeField] private int sortingOrder = 100;

    private SpriteRenderer spriteRenderer;
    private int frameIndex;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        frameIndex = 0;
        timer = 0f;
        // Force the order on the live instance so the impact always renders on top,
        // regardless of the serialized prefab value.
        spriteRenderer.sortingOrder = sortingOrder;
        if (frames != null && frames.Length > 0)
            spriteRenderer.sprite = frames[0];
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        timer += Time.deltaTime;
        if (timer < frameInterval) return;

        timer -= frameInterval;
        frameIndex++;

        if (frameIndex >= frames.Length)
        {
            Destroy(gameObject);
            return;
        }

        spriteRenderer.sprite = frames[frameIndex];
    }
}
