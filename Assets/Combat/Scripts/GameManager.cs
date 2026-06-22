using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField, Range(0f, 1f)] private float explosionVolume = 1f;

    private void Awake()
    {
        Instance = this;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShipDestroyed(int losingPlayerIndex)
    {
        int winner = losingPlayerIndex == 0 ? 1 : 0;
        if (gameOverText != null)
            gameOverText.text = $"Player {winner + 1} Wins!";
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Play before freezing time. AudioSource playback runs on the DSP clock and is
        // unaffected by timeScale, so the explosion is still audible at the game-over freeze.
        if (explosionSound != null)
        {
            Vector3 pos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(explosionSound, pos, explosionVolume);
        }

        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
