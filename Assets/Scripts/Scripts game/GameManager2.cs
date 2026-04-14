using UnityEngine;

/// <summary>
/// GOTY — Gestor global del estado del juego.
/// Persiste entre escenas con DontDestroyOnLoad.
/// Guarda qué objetos ha recogido el jugador, qué escenas ha visitado, etc.
/// Acceso desde cualquier script: GameManager.Instance.tieneLinterna
/// </summary>
public class GameManager2 : MonoBehaviour
{
    public static GameManager2 Instance { get; private set; }

    [Header("Inventario del jugador")]
    public bool tieneLinterna = false;

    [Header("Progreso de escenas")]
    public bool prologoCompletado = false;

    void Awake()
    {
        // Singleton — si ya existe uno, destruye el duplicado
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // survives scene changes
    }
}