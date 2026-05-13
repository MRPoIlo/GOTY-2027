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
    [SerializeField] private GameObject canvasMiniJuegoRejilla;
    [SerializeField] private GameObject objetoDestornillador;

    [Header("Mensaje entrada rejilla")]
    [SerializeField] private GameObject mensajePresionaE;

    [Header("Timer")]
    [SerializeField] private TimerBaño timerBaño;
    [SerializeField] private float tiempoTotal = 90f;

    [Header("Objeto Jumpscare (Game Over)")]
    [SerializeField] private GameObject jumpscareObject;

    [Header("Paneles externos")]
    [SerializeField] private GameObject panelMenuPausa;
    [SerializeField] private GameObject panelOpcionesPausa;
    [SerializeField] private Canvas canvasNarracion;

    [Header("Audio Golpes")]
    [SerializeField] private AudioSource audioGolpes;

    [Header("Audio Reloj")]
    [SerializeField] private AudioSource audioReloj;
    [SerializeField] private float duracionLoopReloj = 2f;

    private PausaManager pausaManager;
    private NarracionManager narracionManager;
    private PlayerController playerController;

    private bool miniJuegoActivo = false;
    private bool rejillaAbierta = false;
    private bool esperandoEntrada = false;

    public bool enGameOver = false;

    private float tiempoRestante;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        pausaManager = FindFirstObjectByType<PausaManager>();
        narracionManager = FindFirstObjectByType<NarracionManager>();
        playerController = FindFirstObjectByType<PlayerController>();
    }

    private void Start()
    {
        tiempoRestante = tiempoTotal;

        if (canvasMiniJuegoRejilla != null)
            canvasMiniJuegoRejilla.SetActive(false);

        if (mensajePresionaE != null)
            mensajePresionaE.SetActive(false);

        timerBaño?.IniciarTimer();

        if (audioReloj != null)
            StartCoroutine(ReproducirRelojLoop());

        if (audioGolpes != null)
            StartCoroutine(ReproducirGolpes());
    }

    private void Update()
    {
        if (enGameOver)
            return;

        tiempoRestante -= Time.deltaTime;

        // 🎥 FOV dinámico
        float progreso = 1f - (tiempoRestante / tiempoTotal);

        if (Camera.main != null)
        {
            Camera.main.fieldOfView =
                Mathf.Lerp(60f, 75f, progreso);
        }

        // Entrar a rejilla
        if (esperandoEntrada &&
            Input.GetKeyDown(KeyCode.E))
        {
            esperandoEntrada = false;

            if (mensajePresionaE != null)
                mensajePresionaE.SetActive(false);

            StartCoroutine(FinalizarNivel());
        }
    }

    // ─────────────────────────────────────
    // GOLPES
    // ─────────────────────────────────────

    private IEnumerator ReproducirGolpes()
    {
        float intervalo = 15f;

        while (!enGameOver &&
               tiempoRestante > 0f)
        {
            yield return new WaitForSeconds(intervalo);

            if (audioGolpes != null)
            {
                float intensidad = 0.2f;
                float duracion = 0.15f;

                if (tiempoRestante <= 60f)
                {
                    intensidad = 0.35f;
                    duracion = 0.25f;
                    intervalo = 7f;
                }

                if (tiempoRestante <= 30f)
                {
                    intensidad = 0.6f;
                    duracion = 0.4f;
                }

                if (tiempoRestante <= 15f)
                {
                    intensidad = 0.8f;
                    duracion = 0.6f;

                    audioGolpes.pitch = 1.3f;
                }

                audioGolpes.Play();

                // ✅ FIX CÁMARA
                CameraShake.Instance?.StartShake(
                    duracion,
                    intensidad
                );

                // ✅ FIX SPAM MENSAJES
                if (Random.value < 0.2f &&
                    CameraShake.Instance != null &&
                    CameraShake.Instance.PuedeNarrar())
                {
                    narracionManager?.Narrar(
                        "¿Qué fue eso...?"
                    );

                    CameraShake.Instance
                        .ActivarCooldownNarracion();
                }
            }
        }
    }

    // ─────────────────────────────────────
    // RELOJ
    // ─────────────────────────────────────

    private IEnumerator ReproducirRelojLoop()
    {
        while (!enGameOver)
        {
            if (audioReloj != null)
                audioReloj.Play();

            yield return new WaitForSeconds(
                duracionLoopReloj
            );
        }
    }

    // ─────────────────────────────────────
    // REJILLA
    // ─────────────────────────────────────

    public void IntentarAbrirRejilla()
    {
        if (miniJuegoActivo || rejillaAbierta)
            return;

        if (pausaManager != null &&
            !pausaManager.tieneDestornillador)
        {
            narracionManager?.Narrar(
                "Está cerrada... necesito algo para abrirla."
            );

            objetoDestornillador
                ?.GetComponent<DestornilladorItem>()
                ?.HabilitarRecogida();
        }
        else
        {
            AbrirMiniJuego();
        }
    }

    private void AbrirMiniJuego()
    {
        miniJuegoActivo = true;

        playerController?.SetBloqueado(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        canvasMiniJuegoRejilla?.SetActive(true);
    }

    public void CompletarMiniJuego()
    {
        miniJuegoActivo = false;
        rejillaAbierta = true;

        canvasMiniJuegoRejilla?.SetActive(false);

        playerController?.SetBloqueado(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        narracionManager?.Narrar(
            "Ya está abierta, puedo entrar."
        );

        StartCoroutine(
            MostrarMensajeEntradaTrasNarracion()
        );
    }

    private IEnumerator MostrarMensajeEntradaTrasNarracion()
    {
        yield return new WaitUntil(() =>
            narracionManager == null ||
            !narracionManager.EstaActivo()
        );

        mensajePresionaE?.SetActive(true);

        esperandoEntrada = true;
    }

    // ─────────────────────────────────────
    // FINALIZAR NIVEL
    // ─────────────────────────────────────

    private IEnumerator FinalizarNivel()
    {
        playerController?.SetBloqueado(true);

        yield return StartCoroutine(
            Fade(1f, duracionFade)
        );

        SceneManager.LoadScene(escenaSiguiente);
    }

    private IEnumerator Fade(
        float objetivo,
        float duracion)
    {
        if (pantallaFade == null)
            yield break;

        float inicio = pantallaFade.alpha;

        float t = 0f;

        while (t < duracion)
        {
            t += Time.deltaTime;

            pantallaFade.alpha =
                Mathf.Lerp(
                    inicio,
                    objetivo,
                    t / duracion
                );

            yield return null;
        }

        pantallaFade.alpha = objetivo;
    }

    // ─────────────────────────────────────
    // GAME OVER
    // ─────────────────────────────────────

    public void GameOverPorTiempo()
    {
        enGameOver = true;

        audioReloj?.Stop();
        audioGolpes?.Stop();

        playerController?.SetBloqueado(true);

        narracionManager?.Narrar(
            "El tiempo se acabó... no pude escapar."
        );

        StartCoroutine(GameOverTrasNarracion());
    }

    private IEnumerator GameOverTrasNarracion()
    {
        yield return new WaitUntil(() =>
            narracionManager == null ||
            !narracionManager.EstaActivo()
        );

        panelMenuPausa?.SetActive(false);
        panelOpcionesPausa?.SetActive(false);

        if (canvasNarracion != null)
            canvasNarracion.enabled = false;

        canvasMiniJuegoRejilla?.SetActive(false);
        mensajePresionaE?.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        GameOverManager.Instance?.ActivarGameOver();
    }

    // ─────────────────────────────────────
    // BOTONES
    // ─────────────────────────────────────

    public void ReintentarNivel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f;

        audioReloj?.Stop();
        audioGolpes?.Stop();

        SceneManager.LoadScene("MainMenu");
    }
}