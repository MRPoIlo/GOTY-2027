using UnityEngine;

public class GameManager2 : MonoBehaviour
{
    public static GameManager2 Instance { get; private set; }

    public bool tieneLinterna = false;
    public bool prologoCompletado = false;

    // NUEVO: flag para el destornillador
    public bool tieneDestornillador = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
