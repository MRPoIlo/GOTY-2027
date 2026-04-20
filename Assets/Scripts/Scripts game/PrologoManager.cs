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
    // ── Narración de apertura (al despertar) ─────────────────────────────────
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

    // ── Narración al intentar la puerta de salida ────────────────────────────
    private static readonly string[] NarracionPuertaBloqueada = new[]
    {
        "No puedo salir así.",
        "Hay algo que todavía no me visto."
    };

    [Header("Referencia a UI Fade")]
    [SerializeField] private CanvasGroup pantallaFade;
    [SerializeField] private float duracionFade = 2.5f;

    [Header("Siguiente escena")]
    [SerializeField] private string escenaSiguiente = "Nivel1"; // ← aquí está el cambio

    [Header("Condición de avance")]
    [Tooltip("Cuántos objetos debe examinar el jugador antes de poder salir")]
    [SerializeField] private int objetosRequeridosParaSalir = 3;

    // Estado interno
    private int objetosExaminados = 0;
    private bool puertaDesbloqueada = false;
    private bool finalizando = false;

    private PlayerController player;

    void Awake()
    {
        player = FindObjectOfType<PlayerController>();
        player?.SetBloqueado(true);

        if (pantallaFade != null) pantallaFade.alpha = 1f;
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(Fade(0f, duracionFade));

        yield return new WaitForSeconds(0.5f);

        if (NarracionManager.Instance != null)
        {
            NarracionManager.Instance.Narrar(NarracionDespiertar);
            yield return new WaitUntil(() => NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());
        }

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

        if (NarracionManager.Instance != null)
        {
            string[] narracionFinal = new[]
            {
                "La puerta cede.",
                "Quizás ya estoy listo para ver el resto."
            };
            NarracionManager.Instance.Narrar(narracionFinal);
            yield return new WaitUntil(() => NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());
        }

        yield return StartCoroutine(Fade(1f, duracionFade));
        SceneManager.LoadScene("Nivel 1"); // ← aquí carga Nivel1
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
