using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager2 : MonoBehaviour
{
    public static GameManager2 Instance { get; private set; }

    [Header("Progreso del juego")]
    public bool tieneLinterna = false;
    public bool prologoCompletado = false;
    public bool tieneDestornillador = false;

    [Header("Finales")]
    public int objetosBuenosRecogidos = 0;
    public int minimoFinalBueno = 5;

    private void Awake()
    {
        // Patrón Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log(
            $"[GameManager2] Inicializado correctamente.\n" +
            $"Escena actual: {SceneManager.GetActiveScene().name}\n" +
            $"Recuerdos acumulados: {objetosBuenosRecogidos}/{minimoFinalBueno}"
        );
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(
            $"[GameManager2] Nueva escena cargada: {scene.name}\n" +
            $"Recuerdos acumulados: {objetosBuenosRecogidos}/{minimoFinalBueno}\n" +
            $"¿Final bueno desbloqueado?: {DesbloqueaFinalBueno()}"
        );
    }

    /// <summary>
    /// Llamar cada vez que el jugador interactúe con un objeto
    /// que tenga el tag "Recuerdo".
    /// </summary>
    public void RegistrarObjetoBueno()
    {
        objetosBuenosRecogidos++;

        Debug.Log(
            $"[GameManager2] Recuerdo registrado.\n" +
            $"Total actual: {objetosBuenosRecogidos}/{minimoFinalBueno}\n" +
            $"Faltan: {Mathf.Max(0, minimoFinalBueno - objetosBuenosRecogidos)}\n" +
            $"¿Final bueno desbloqueado?: {DesbloqueaFinalBueno()}"
        );
    }

    /// <summary>
    /// Retorna true si se alcanzó el mínimo requerido.
    /// </summary>
    public bool DesbloqueaFinalBueno()
    {
        return objetosBuenosRecogidos >= minimoFinalBueno;
    }

    /// <summary>
    /// Reinicia el contador manualmente (opcional).
    /// </summary>
    public void ReiniciarRecuerdos()
    {
        objetosBuenosRecogidos = 0;

        Debug.Log(
            $"[GameManager2] Contador reiniciado.\n" +
            $"Recuerdos actuales: {objetosBuenosRecogidos}/{minimoFinalBueno}"
        );
    }
}