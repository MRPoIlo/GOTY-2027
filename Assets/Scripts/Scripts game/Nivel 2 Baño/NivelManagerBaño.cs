using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NivelManagerBaño : MonoBehaviour
{
    public static NivelManagerBaño Instance { get; private set; }

    [Header("Referencia a UI Fade")]
    [SerializeField] private CanvasGroup pantallaFade;
    [SerializeField] private float duracionFade = 2.5f;

    [Header("Siguiente escena")]
    [SerializeField] private string escenaSiguiente = "Nivel3Atico";

    [Header("Mini-juego Rejilla")]
    [SerializeField] private GameObject canvasMiniJuegoRejilla; // Canvas con los 4 tornillos
    [SerializeField] private GameObject objetoDestornillador;   // GameObject del destornillador en la escena

    [Header("Mensaje entrada rejilla")]
    [SerializeField] private GameObject mensajePresionaE;       // UI con "Presione E para entrar"

    private PausaManager pausaManager;
    private NarracionManager narracionManager;
    private PlayerController playerController;

    // Estados internos
    private bool miniJuegoActivo = false;
    private bool rejillaAbierta = false;
    private bool esperandoEntrada = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        pausaManager = FindObjectOfType<PausaManager>();
        narracionManager = FindObjectOfType<NarracionManager>();
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Start()
    {
        if (canvasMiniJuegoRejilla != null) canvasMiniJuegoRejilla.SetActive(false);
        if (mensajePresionaE != null) mensajePresionaE.SetActive(false);

        // 🔹 El destornillador ahora siempre existe en escena, no lo desactivamos
    }

    private void Update()
    {
        // Cuando la rejilla está abierta y el jugador presiona E, se carga la siguiente escena
        if (esperandoEntrada && Input.GetKeyDown(KeyCode.E))
        {
            esperandoEntrada = false;
            if (mensajePresionaE != null) mensajePresionaE.SetActive(false);
            StartCoroutine(FinalizarNivel());
        }
    }

    // Llamado desde el trigger/interacción con la rejilla
    public void IntentarAbrirRejilla()
    {
        if (miniJuegoActivo || rejillaAbierta) return;

        if (pausaManager != null && !pausaManager.tieneDestornillador)
        {
            narracionManager?.Narrar("Está cerrada... necesito algo para abrirla.");
            objetoDestornillador.GetComponent<DestornilladorItem>()?.HabilitarRecogida();
        }
        else
        {
            AbrirMiniJuego();
        }
    }

    // Activa el Canvas del mini-juego y bloquea al jugador
    private void AbrirMiniJuego()
    {
        miniJuegoActivo = true;
        playerController?.SetBloqueado(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (canvasMiniJuegoRejilla != null)
            canvasMiniJuegoRejilla.SetActive(true);
    }

    // Llamado por MiniJuegoRejilla cuando los 4 tornillos han sido retirados
    public void CompletarMiniJuego()
    {
        miniJuegoActivo = false;
        rejillaAbierta = true;

        if (canvasMiniJuegoRejilla != null)
            canvasMiniJuegoRejilla.SetActive(false);

        // 🔹 Mantener al jugador bloqueado hasta que presione E para salir
        playerController?.SetBloqueado(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        narracionManager?.Narrar("Ya está abierta, puedo entrar.");
        StartCoroutine(MostrarMensajeEntradaTrasNarracion());
    }

    // Paso 5: mostrar "Presione E para entrar" cuando la narración termine
    private IEnumerator MostrarMensajeEntradaTrasNarracion()
    {
        yield return new WaitUntil(() => narracionManager == null || !narracionManager.EstaActivo());

        if (mensajePresionaE != null)
            mensajePresionaE.SetActive(true);

        esperandoEntrada = true;
    }

    // Fade y carga de escena
    private IEnumerator FinalizarNivel()
    {
        playerController?.SetBloqueado(true);
        yield return StartCoroutine(Fade(1f, duracionFade));
        SceneManager.LoadScene(escenaSiguiente);
    }

    private IEnumerator Fade(float objetivo, float duracion)
    {
        if (pantallaFade == null) yield break;
        float inicio = pantallaFade.alpha;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            pantallaFade.alpha = Mathf.Lerp(inicio, objetivo, t / duracion);
            yield return null;
        }
        pantallaFade.alpha = objetivo;
    }
}
