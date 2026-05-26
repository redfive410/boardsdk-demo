using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ShieldRingRenderer : MonoBehaviour
{
    [SerializeField] private float radius = 1.5f;
    [SerializeField] private int segments = 48;
    [SerializeField] private Color color = new Color(0.2f, 0.6f, 1f, 0.8f);
    [SerializeField] private float width = 0.05f;

    private LineRenderer lr;

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = segments;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.startColor = color;
        lr.endColor = color;
        lr.material = new Material(Shader.Find("Sprites/Default"));

        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }

    public void SetColor(Color c)
    {
        color = c;
        if (lr != null) { lr.startColor = c; lr.endColor = c; }
    }
}
