using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerDashboard : MonoBehaviour
{
    [SerializeField] private Button fireButton;
    [SerializeField] private Slider intensitySlider;

    [Tooltip("Min/max bullet speed mapped to slider 0-1")]
    public float minSpeed = 2f;
    public float maxSpeed = 15f;

    public bool FirePressed { get; private set; }
    public float BulletSpeed => Mathf.Lerp(minSpeed, maxSpeed, intensitySlider != null ? intensitySlider.value : 0.5f);

    private void Start()
    {
        var trigger = fireButton.gameObject.AddComponent<EventTrigger>();

        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener(_ => FirePressed = true);
        trigger.triggers.Add(down);

        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener(_ => FirePressed = false);
        trigger.triggers.Add(up);
    }
}
