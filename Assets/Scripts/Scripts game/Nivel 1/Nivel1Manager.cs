using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NivelManager1 : MonoBehaviour
{
    private static readonly string[] NarracionInicio = {
        "La habitación de mis padres.",
        "Aquí todo parece detenido en el tiempo."
    };

    private static readonly string[] NarracionEnemigo = {
        "Escucho pasos...",
        "No debería estar aquí."
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
    [SerializeField] private int objetosRequeridosParaEnemigo = 3;

    private int interaccionesCompletadas = 0;
    private bool enemigoActivado = false;
    private bool finalizando = false;

    [Header("Referencia al enemigo")]
    public EnemyAI enemigo;

    [Header("Puerta narrativa")]
    [SerializeField] private GameObject puertaEntrada; // referencia al objeto puerta

    private PlayerController player;

    void Awake()
    {
        player = FindFirstObjectByType<PlayerController>();
        player?.SetBloqueado(true);

        if (pantallaFade != null) pantallaFade.alpha = 1f;
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(Fade(0f, duracionFade));
        yield return new WaitForSeconds(0.5f);

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

        if (puertaEntrada != null)
        {
            puertaEntrada.SetActive(false);
            Collider col = puertaEntrada.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            Debug.Log("[Nivel1] Puerta desactivada, el padre entra.");
        }

        // 🔹 Activar rutina del enemigo después de 5 segundos
        if (enemigo != null)
        {
            enemigo.Activar();
            Debug.Log("[Nivel1] Rutina del enemigo activada con delay.");
        }
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

    private IEnumerator TerminarNivel1()
    {
        finalizando = true;
        player?.SetBloqueado(true);

        if (NarracionManager.Instance != null)
        {
            string[] narracionFinal = {
                "La puerta del baño se abre.",
                "El silencio me acompaña hacia lo desconocido."
            };
            NarracionManager.Instance.Narrar(narracionFinal);
            yield return new WaitUntil(() => NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());
        }

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
