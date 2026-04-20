using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NivelManager1 : MonoBehaviour
{
    // ── Narración de apertura del Nivel 1 ────────────────────────────────
    private static readonly string[] NarracionInicio = new[]
    {
        "La habitación de mis padres.",
        "Aquí todo parece detenido en el tiempo."
    };

    // ── Narración al completar las interacciones ─────────────────────────
    private static readonly string[] NarracionEnemigo = new[]
    {
        "Escucho pasos...",
        "No debería estar aquí."
    };

    // ── Narración al intentar salir antes de completar ───────────────────
    private static readonly string[] NarracionPuertaBloqueada = new[]
    {
        "Todavía hay cosas que debo enfrentar.",
        "No puedo salir sin mirar atrás."
    };

    [Header("Referencia a UI Fade")]
    [SerializeField] private CanvasGroup pantallaFade;
    [SerializeField] private float duracionFade = 2.5f;

    [Header("Siguiente escena")]
    [SerializeField] private string escenaSiguiente = "Nivel2";

    [Header("Condición de avance")]
    [Tooltip("Cuántos objetos debe examinar el jugador antes de activar al enemigo")]
    [SerializeField] private int objetosRequeridosParaEnemigo = 3;

    // Estado interno
    private int interaccionesCompletadas = 0;
    private bool enemigoActivado = false;
    private bool finalizando = false;

    [Header("Referencia al enemigo")]
    public EnemyAI enemigo;

    private PlayerController player;

    void Awake()
    {
        player = FindObjectOfType<PlayerController>();
        player?.SetBloqueado(true);

        if (pantallaFade != null) pantallaFade.alpha = 1f;
    }

    IEnumerator Start()
    {
        // Fade inicial
        yield return StartCoroutine(Fade(0f, duracionFade));
        yield return new WaitForSeconds(0.5f);

        // Narración inicial
        if (NarracionManager.Instance != null)
        {
            NarracionManager.Instance.Narrar(NarracionInicio);
            yield return new WaitUntil(() => NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());
        }

        player?.SetBloqueado(false);
    }

    // ─── Llamados desde ObjetoInteractuable.OnInteractuado ───────────────
    public void RegistrarInteraccion()
    {
        interaccionesCompletadas++;
        Debug.Log($"[Nivel1] Interacciones: {interaccionesCompletadas}/{objetosRequeridosParaEnemigo}");

        if (interaccionesCompletadas >= objetosRequeridosParaEnemigo && !enemigoActivado)
        {
            ActivarEnemigo();
        }
    }

    private void ActivarEnemigo()
    {
        enemigoActivado = true;
        NarracionManager.Instance?.Narrar(NarracionEnemigo);
        enemigo.gameObject.SetActive(true);
        Debug.Log("[Nivel1] Enemigo activado.");
    }

    public void IntentarSalir()
    {
        if (finalizando) return;

        if (enemigoActivado)
        {
            StartCoroutine(TerminarNivel1());
        }
        else
        {
            NarracionManager.Instance?.Narrar(NarracionPuertaBloqueada);
        }
    }

    // ─── Transición final ────────────────────────────────────────────────
    private IEnumerator TerminarNivel1()
    {
        finalizando = true;
        player?.SetBloqueado(true);

        if (NarracionManager.Instance != null)
        {
            string[] narracionFinal = new[]
            {
                "La puerta se abre.",
                "El silencio me acompaña hacia lo desconocido."
            };
            NarracionManager.Instance.Narrar(narracionFinal);
            yield return new WaitUntil(() => NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());
        }

        yield return StartCoroutine(Fade(1f, duracionFade));
        SceneManager.LoadScene(escenaSiguiente);
    }

    // ─── Fade ────────────────────────────────────────────────────────────
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
