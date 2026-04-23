using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GOTY — Gestor narrativo del Prólogo.
/// Controla el orden de eventos: fade-in → narración → exploración → salida.
/// Coloca este script en el GameObject "PrologoManager" de la escena.
/// </summary>
public class PrologoManager : MonoBehaviour
{
    public static PrologoManager Instance { get; private set; }

    // ── Narración de apertura ────────────────────────────────────────────────
    private static readonly string[] NarracionDespiertar = new[]
    {
        "Esta habitación…",
        "La recuerdo.",
        "Hay algo que no me deja salir."
    };

    // ── Narración al entrar al pasillo ───────────────────────────────────────
    private static readonly string[] NarracionPasillo = new[]
    {
        "El pasillo siempre olía a madera vieja.",
        "Aquí aprendí a caminar sin hacer ruido."
    };

    // ── Narración al intentar la puerta bloqueada ────────────────────────────
    private static readonly string[] NarracionPuertaBloqueada = new[]
    {
        "No puedo salir así.",
        "Hay algo que todavía no he visto."
    };

    [Header("Referencia a UI Fade")]
    [SerializeField] private CanvasGroup pantallaFade;
    [SerializeField] private float duracionFade = 2.5f;

    [Header("Siguiente escena")]
    [SerializeField] private string escenaSiguiente = "Nivel1";

    [Header("Condición de avance")]
    [SerializeField] private int objetosRequeridosParaSalir = 3;

    [Header("Puerta de salida")]
    [SerializeField] private PuertaNivel puertaSalida; // referencia directa a la puerta

    // Estado interno
    private int objetosExaminados = 0;
    private bool puertaDesbloqueada = false;
    private bool finalizando = false;

    private PlayerController player;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        player = FindObjectOfType<PlayerController>();
        player?.SetBloqueado(true);

        if (pantallaFade != null) pantallaFade.alpha = 1f;
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(Fade(0f, duracionFade));

        yield return new WaitForSeconds(0.5f);

        NarracionManager.Instance?.Narrar(NarracionDespiertar);
        yield return new WaitUntil(() =>
            NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());

        player?.SetBloqueado(false);
    }

    // ─── Llamados desde TriggerZona ──────────────────────────────────────────
    public void EntrarAlPasillo()
    {
        NarracionManager.Instance?.Narrar(NarracionPasillo);
    }

    // ─── Llamados desde ObjetoInteractuable.OnInteractuado ───────────────────
    public void RegistrarObjetoExaminado()
    {
        objetosExaminados++;
        Debug.Log($"[Prólogo] Objetos examinados: {objetosExaminados}/{objetosRequeridosParaSalir}");

        if (objetosExaminados >= objetosRequeridosParaSalir && !puertaDesbloqueada)
        {
            DesbloquearSalida();
        }
    }

    private void DesbloquearSalida()
    {
        puertaDesbloqueada = true;
        puertaSalida.HabilitarPuerta(); // 🔹 habilita la puerta
        NarracionManager.Instance?.Narrar("Algo se ha movido.");
        Debug.Log("[Prólogo] Salida desbloqueada.");
    }

    public void IntentarSalir()
    {
        if (finalizando) return;

        if (puertaDesbloqueada)
        {
            StartCoroutine(TerminarPrologo());
        }
        else
        {
            NarracionManager.Instance?.Narrar(NarracionPuertaBloqueada);
        }
    }

    // ─── Transición final ─────────────────────────────────────────────────────
    private IEnumerator TerminarPrologo()
    {
        finalizando = true;
        player?.SetBloqueado(true);

        NarracionManager.Instance?.Narrar(new string[]
        {
            "La puerta cede.",
            "Quizás ya estoy listo para ver el resto."
        });

        yield return new WaitUntil(() =>
            NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());

        yield return StartCoroutine(Fade(1f, duracionFade));
        SceneManager.LoadScene(escenaSiguiente);
    }

    // ─── Fade ─────────────────────────────────────────────────────────────────
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
