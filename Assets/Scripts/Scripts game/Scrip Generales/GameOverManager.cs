using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Panel Game Over")]
    [SerializeField] private CanvasGroup panelGameOver;

    [Header("Reintentar")]
    [SerializeField] private string escenaActual = ""; // dejar vacío = recarga automática

    [Header("Salir al menú")]
    [SerializeField] private string escenaMenuPrincipal = "MenuPrincipal";

    [Header("Música Game Over")]
    [SerializeField] private AudioSource musicaGameOver; // 🎵 arrastra aquí tu AudioSource

    public bool gameOverActivado = false;

    private PlayerController player;
    private PausaManager pausaManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GameOverManager duplicado destruido: " + gameObject.name);
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
        else
        {
            Debug.LogError("[GameOverManager] panelGameOver no está asignado en el inspector.");
        }

        if (musicaGameOver != null)
        {
            musicaGameOver.playOnAwake = false; // evitar que suene antes de tiempo
        }
    }

    public void ActivarGameOver()
    {
        if (gameOverActivado) return;
        gameOverActivado = true;

        if (panelGameOver == null)
        {
            Debug.LogError("panelGameOver es NULL. Asigna el CanvasGroup en el Inspector.");
            return;
        }

        panelGameOver.gameObject.SetActive(true);
        panelGameOver.alpha = 1f;
        panelGameOver.interactable = true;
        panelGameOver.blocksRaycasts = true;

        if (player != null) player.SetBloqueado(true);
        if (pausaManager != null) pausaManager.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        // 🎵 Reproducir música de Game Over
        if (musicaGameOver != null)
        {
            musicaGameOver.Play();
        }
    }

    public void Reintentar()
    {
        Time.timeScale = 1f;
        gameOverActivado = false;

        if (musicaGameOver != null)
        {
            musicaGameOver.Stop();
        }

        string escena = string.IsNullOrEmpty(escenaActual)
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
