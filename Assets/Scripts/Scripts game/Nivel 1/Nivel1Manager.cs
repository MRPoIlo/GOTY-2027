using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NivelManager1 : MonoBehaviour
{
    private static readonly string[] NarracionInicio = {
        "La habitación de mis padres.",
        "Aquí todo parece detenido en el tiempo, como si el aire aún guardara discusiones que nunca se apagaron."
    };

    private static readonly string[] NarracionSombraMadre = {
        "Una silueta oscura me observa...",
        "No es un fantasma, es la memoria que me persigue."
    };

    private static readonly string[] NarracionPasos = {
        "Escucho pasos...",
        "Escóndete."
    };

    private static readonly string[] NarracionEscape = {
        "¡Me escucharon... corre!"
    };

    private static readonly string[] NarracionPuertaBloqueada = {
        "Todavía hay cosas que debo enfrentar.",
        "No puedo salir sin mirar atrás."
    };

    [Header("Referencia a UI Fade")]
    [SerializeField] private CanvasGroup pantallaFade;
    [SerializeField] private float duracionFade = 2.5f;

    [Header("Siguiente escena")]
    [SerializeField] private string escenaSiguiente = "Nivel2Baño";

    [Header("Condición de avance")]
    [SerializeField] private int objetosRequeridos = 4; // cama, foto, diario, perfume

    private int interaccionesCompletadas = 0;
    private bool sombraActivada = false;
    private bool enemigoActivado = false;
    private bool finalizando = false;

    [Header("Referencia al enemigo")]
    [SerializeField] private EnemyAI enemigo;

    [Header("Puerta narrativa")]
    [SerializeField] private GameObject puertaEntrada;

    private PlayerController player;

    void Awake()
    {
        player = FindFirstObjectByType<PlayerController>();

        if (enemigo == null)
            enemigo = FindFirstObjectByType<EnemyAI>();

        player?.SetBloqueado(true);

        if (pantallaFade != null) pantallaFade.alpha = 1f;
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(Fade(0f, duracionFade));
        yield return new WaitForSeconds(0.5f);

        NarracionManager.Instance?.Narrar(NarracionInicio);
        yield return new WaitUntil(() => !NarracionManager.Instance.EstaActivo());

        player?.SetBloqueado(false);
    }

    // ─── INTERACCIONES ─────────────────────────
    public void RegistrarInteraccion()
    {
        interaccionesCompletadas++;
        Debug.Log($"[Nivel1] Interacciones: {interaccionesCompletadas}/{objetosRequeridos}");

        if (interaccionesCompletadas >= objetosRequeridos && !sombraActivada)
        {
            ActivarSombraMadre();
        }
    }

    private void ActivarSombraMadre()
    {
        sombraActivada = true;
        player?.SetBloqueado(true);

        NarracionManager.Instance?.Narrar(NarracionSombraMadre);
        NarracionManager.Instance.OnNarracionTerminada.AddListener(() =>
        {
            player?.SetBloqueado(false);
            IniciarSecuenciaPasos();
        });
    }

    private void IniciarSecuenciaPasos()
    {
        NarracionManager.Instance?.Narrar(NarracionPasos);

        // 🔴 Delay de 3 segundos antes de activar enemigo
        StartCoroutine(ActivarEnemigoConDelay(3f));
    }

    private IEnumerator ActivarEnemigoConDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ActivarEnemigo();
    }

    private void ActivarEnemigo()
    {
        enemigoActivado = true;

        if (puertaEntrada != null)
        {
            puertaEntrada.SetActive(false);
            Collider col = puertaEntrada.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        if (enemigo != null)
        {
            enemigo.Activar();
            Debug.Log("[Nivel1] Enemigo activado correctamente.");
        }
    }

    // ─── SALIDA ─────────────────────────
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

    private IEnumerator TerminarNivel1()
    {
        finalizando = true;
        player?.SetBloqueado(true);

        NarracionManager.Instance?.Narrar(NarracionEscape);
        yield return new WaitUntil(() => !NarracionManager.Instance.EstaActivo());

        yield return StartCoroutine(Fade(1f, duracionFade));
        SceneManager.LoadScene(escenaSiguiente);
    }

    // ─── FADE ─────────────────────────
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
