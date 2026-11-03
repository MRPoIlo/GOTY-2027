using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [SerializeField] private Light LuzLinterna;
    private bool linternaBloqueada = true;
    private bool encendida = false;

    private PausaManager pausaManager;

    /// <summary>True si la linterna ya se desbloqueó en esta partida (memoria o PlayerPrefs).</summary>
    public static bool LinternaDesbloqueada =>
        (GameManager2.Instance != null && GameManager2.Instance.tieneLinterna) ||
        PlayerPrefs.GetInt(LogrosManager.KeyLinterna, 0) == 1;

    void Awake()
    {
        // Si hay dos Flashlight controlando la MISMA luz (p. ej. en el personaje y en su cámara),
        // solo uno debe actuar; si no, la tecla F cambiaría la luz dos veces.
        foreach (var otro in GetComponentInParent<PlayerController>(true) != null
                     ? GetComponentInParent<PlayerController>(true).GetComponentsInChildren<Flashlight>(true)
                     : new Flashlight[0])
        {
            if (otro != this && otro.enabled && otro.LuzLinterna == LuzLinterna &&
                otro.GetInstanceID() < GetInstanceID())
            {
                enabled = false;
                return;
            }
        }
    }

    void Start()
    {
        if (LuzLinterna != null)
            LuzLinterna.enabled = false;

        if (LinternaDesbloqueada)
        {
            linternaBloqueada = false;
            if (GameManager2.Instance != null)
                GameManager2.Instance.tieneLinterna = true;
        }

        pausaManager = FindAnyObjectByType<PausaManager>();
    }

    void Update()
    {
        if (pausaManager != null)
        {
            if (pausaManager.juegoPausado || pausaManager.EnOpciones)
                return;
        }

        if (linternaBloqueada) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            encendida = !encendida;

            if (LuzLinterna != null)
                LuzLinterna.enabled = encendida;
        }
    }

    public void DesbloquearLinterna()
    {
        linternaBloqueada = false;

        if (LuzLinterna != null)
        {
            encendida = true;
            LuzLinterna.enabled = true;
        }

        if (GameManager2.Instance != null)
            GameManager2.Instance.tieneLinterna = true;

        PlayerPrefs.SetInt(LogrosManager.KeyLinterna, 1);
        PlayerPrefs.Save();
    }

    /// <summary>Desbloquea la linterna en todos los Flashlight activos de la escena.</summary>
    public static void DesbloquearTodas()
    {
        foreach (var f in FindObjectsByType<Flashlight>(FindObjectsSortMode.None))
            if (f.enabled)
                f.DesbloquearLinterna();
    }
}
