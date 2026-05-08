using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Panel Game Over")]
    [SerializeField] private CanvasGroup panelGameOver;

    [Header("Reintentar")]
    [SerializeField] private string escenaActual = ""; // dejar vacío = recarga automática

    public bool gameOverActivado = false;

    private PlayerController player;
    private PausaManager pausaManager;

    void Awake()
    {
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
    }

    public void ActivarGameOver()
    {
        if (gameOverActivado) return;
        gameOverActivado = true;

        // Asegurar que el tiempo esté activo
        Time.timeScale = 1f;

        // Bloquear jugador
        if (player != null) player.SetBloqueado(true);

        // Desactivar pausa
        if (pausaManager != null) pausaManager.enabled = false;

        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Mostrar panel de inmediato
        if (panelGameOver != null)
        {
            panelGameOver.alpha = 1f;
            panelGameOver.interactable = true;
            panelGameOver.blocksRaycasts = true;
        }
    }

    // Llamado desde el botón "Reintentar" del panel
    public void Reintentar()
    {
        Time.timeScale = 1f;
        gameOverActivado = false;

        string escena = string.IsNullOrEmpty(escenaActual)
            ? SceneManager.GetActiveScene().name
            : escenaActual;

        SceneManager.LoadScene(escena);
    }
}
