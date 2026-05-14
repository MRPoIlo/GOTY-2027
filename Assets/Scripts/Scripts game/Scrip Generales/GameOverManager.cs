using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Panel Game Over")]
    [SerializeField] private CanvasGroup panelGameOver;

    [Header("Reintentar")]
    [SerializeField] private string escenaActual = "";

    [Header("Salir al menú")]
    [SerializeField] private string escenaMenuPrincipal = "MenuPrincipal";

    [Header("Música Game Over")]
    [SerializeField] private AudioSource musicaGameOver;

    public bool gameOverActivado = false;

    private PlayerController player;
    private PausaManager pausaManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        player = FindFirstObjectByType<PlayerController>();
        pausaManager = FindFirstObjectByType<PausaManager>();
    }

    void Start()
    {
        if (panelGameOver != null)
        {
            panelGameOver.gameObject.SetActive(true);

            panelGameOver.alpha = 0f;
            panelGameOver.interactable = false;
            panelGameOver.blocksRaycasts = false;
        }

        if (musicaGameOver != null)
        {
            musicaGameOver.playOnAwake = false;
            musicaGameOver.loop = true;

            // 🔥 IMPORTANTE
            musicaGameOver.ignoreListenerPause = true;
        }
    }

    public void ActivarGameOver()
    {
        if (gameOverActivado)
            return;

        gameOverActivado = true;

        // ─────────────────────────────
        // UI
        // ─────────────────────────────

        if (panelGameOver != null)
        {
            panelGameOver.gameObject.SetActive(true);

            panelGameOver.alpha = 1f;
            panelGameOver.interactable = true;
            panelGameOver.blocksRaycasts = true;
        }

        // ─────────────────────────────
        // Player
        // ─────────────────────────────

        if (player != null)
            player.SetBloqueado(true);

        if (pausaManager != null)
            pausaManager.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // ─────────────────────────────
        // Detener música normal
        // ─────────────────────────────

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.DetenerTodaMusica();
        }

        // ─────────────────────────────
        // Música Game Over
        // ─────────────────────────────

        if (musicaGameOver != null)
        {
            Debug.Log("REPRODUCIENDO MUSICA GAME OVER");

            musicaGameOver.gameObject.SetActive(true);

            musicaGameOver.ignoreListenerPause = true;

            musicaGameOver.volume = 1f;

            musicaGameOver.Stop();

            musicaGameOver.Play();
        }
        else
        {
            Debug.LogError("NO HAY AUDIO SOURCE DE GAME OVER");
        }

        // ─────────────────────────────
        // Pausar juego
        // ─────────────────────────────

        Time.timeScale = 0f;
    }

    public void Reintentar()
    {
        Time.timeScale = 1f;

        gameOverActivado = false;

        if (musicaGameOver != null)
        {
            musicaGameOver.Stop();
        }

        string escena =
            string.IsNullOrEmpty(escenaActual)
            ? SceneManager.GetActiveScene().name
            : escenaActual;

        SceneManager.LoadScene(escena);
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f;

        gameOverActivado = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (musicaGameOver != null)
        {
            musicaGameOver.Stop();
        }

        SceneManager.LoadScene(escenaMenuPrincipal);
    }
}