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

    public bool gameOverActivado = false;

    private PlayerController player;
    private PausaManager pausaManager;

    void Awake()
    {
        // Evita que existan dos GameOverManager
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GameOverManager duplicado destruido: " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Debug.Log("GameOverManager inicializado en: " + gameObject.name);

        player = FindFirstObjectByType<PlayerController>();
        pausaManager = FindFirstObjectByType<PausaManager>();
    }

    void Start()
    {
        if (panelGameOver != null)
        {
            // Mantener el objeto activo, pero invisible
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
        Debug.Log("GAME OVER ACTIVADO");
        Debug.Log("Objeto actual (GameOverManager): " + gameObject.name);

        // Evitar activarlo más de una vez
        if (gameOverActivado)
        {
            Debug.Log("Game Over ya estaba activado.");
            return;
        }

        gameOverActivado = true;

        // Validar referencia al CanvasGroup
        if (panelGameOver == null)
        {
            Debug.LogError("panelGameOver es NULL. Asigna el CanvasGroup en el Inspector.");
            return;
        }

        Debug.Log("Panel asignado: " + panelGameOver.gameObject.name);

        // Forzar activación del objeto que contiene el CanvasGroup
        panelGameOver.gameObject.SetActive(true);

        // Mostrar panel
        panelGameOver.alpha = 1f;
        panelGameOver.interactable = true;
        panelGameOver.blocksRaycasts = true;

        Debug.Log("Alpha actual: " + panelGameOver.alpha);
        Debug.Log("Interactable: " + panelGameOver.interactable);
        Debug.Log("BlocksRaycasts: " + panelGameOver.blocksRaycasts);
        Debug.Log("Objeto activo: " + panelGameOver.gameObject.activeSelf);

        // Bloquear jugador
        if (player != null)
        {
            player.SetBloqueado(true);
            Debug.Log("Jugador bloqueado.");
        }
        else
        {
            Debug.LogWarning("PlayerController no encontrado.");
        }

        // Desactivar pausa
        if (pausaManager != null)
        {
            pausaManager.enabled = false;
            Debug.Log("PausaManager desactivado.");
        }

        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pausar el juego
        Time.timeScale = 0f;

        Debug.Log("Game Over mostrado correctamente.");
    }

    // Llamado desde el botón "Reintentar"
    public void Reintentar()
    {
        Time.timeScale = 1f;
        gameOverActivado = false;

        string escena = string.IsNullOrEmpty(escenaActual)
            ? SceneManager.GetActiveScene().name
            : escenaActual;

        SceneManager.LoadScene(escena);
    }

    // Llamado desde el botón "Salir"
    public void SalirAlMenu()
    {
        Time.timeScale = 1f;
        gameOverActivado = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(escenaMenuPrincipal);
    }
}