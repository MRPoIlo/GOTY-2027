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

    [Header("Objeto Jumpscare (Game Over)")]
    [SerializeField] private GameObject jumpscareObject;

    [Header("Paneles externos (arrastrar desde inspector)")]
    [SerializeField] private GameObject panelMenuPausa;
    [SerializeField] private GameObject panelOpcionesPausa;
    [SerializeField] private Canvas canvasNarracion;

    private PausaManager pausaManager;
    private NarracionManager narracionManager;
    private PlayerController playerController;

    private bool miniJuegoActivo = false;
    private bool rejillaAbierta = false;
    private bool esperandoEntrada = false;

    // 🔹 Bandera de Game Over
    public bool enGameOver = false;

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
        if (canvasMiniJuegoRejilla != null) canvasMiniJuegoRejilla.SetActive(false);
        if (mensajePresionaE != null) mensajePresionaE.SetActive(false);

        if (timerBaño != null) timerBaño.IniciarTimer();

        if (jumpscareObject != null) jumpscareObject.SetActive(false);
    }

    private void Update()
    {
        if (esperandoEntrada && Input.GetKeyDown(KeyCode.E))
        {
            esperandoEntrada = false;
            if (mensajePresionaE != null) mensajePresionaE.SetActive(false);
            StartCoroutine(FinalizarNivel());
        }
    }

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

    private void AbrirMiniJuego()
    {
        miniJuegoActivo = true;
        playerController?.SetBloqueado(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (canvasMiniJuegoRejilla != null)
            canvasMiniJuegoRejilla.SetActive(true);
    }

    public void CompletarMiniJuego()
    {
        miniJuegoActivo = false;
        rejillaAbierta = true;

        if (canvasMiniJuegoRejilla != null)
            canvasMiniJuegoRejilla.SetActive(false);

        playerController?.SetBloqueado(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        narracionManager?.Narrar("Ya está abierta, puedo entrar.");
        StartCoroutine(MostrarMensajeEntradaTrasNarracion());
    }

    private IEnumerator MostrarMensajeEntradaTrasNarracion()
    {
        yield return new WaitUntil(() => narracionManager == null || !narracionManager.EstaActivo());

        if (mensajePresionaE != null)
            mensajePresionaE.SetActive(true);

        esperandoEntrada = true;
    }

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

    // 🔹 Llamado por el Timer cuando se acaba el tiempo
    public void GameOverPorTiempo()
    {
        enGameOver = true;

        playerController?.SetBloqueado(true);
        narracionManager?.Narrar("El tiempo se acabó... no pude escapar.");
        StartCoroutine(GameOverTrasNarracion());
    }

    private IEnumerator GameOverTrasNarracion()
    {
        yield return new WaitUntil(() => narracionManager == null || !narracionManager.EstaActivo());

        if (panelMenuPausa != null) panelMenuPausa.SetActive(false);
        if (panelOpcionesPausa != null) panelOpcionesPausa.SetActive(false);
        if (canvasNarracion != null) canvasNarracion.enabled = false;
        if (canvasMiniJuegoRejilla != null) canvasMiniJuegoRejilla.SetActive(false);
        if (mensajePresionaE != null) mensajePresionaE.SetActive(false);

        playerController?.SetBloqueado(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 🔹 Pausar el juego
        Time.timeScale = 0f;

        if (jumpscareObject != null) jumpscareObject.SetActive(true);
    }

    public void ReintentarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
    