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
    [SerializeField] private string escenaSiguiente = "Nivel3";

    private PausaManager pausaManager;
    private NarracionManager narracionManager;

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
    }

    public void IntentarAbrirRejilla()
    {
        if (pausaManager != null && !pausaManager.tieneDestornillador)
        {
            narracionManager?.Narrar("Está cerrada... necesito algo para abrirla.");
        }
        else
        {
            narracionManager?.Narrar("Debo quitar los tornillos para abrir la rejilla.");
            // Aquí activas tu mini‑juego
        }
    }

    public void CompletarMiniJuego()
    {
        StartCoroutine(FinalizarNivel());
    }

    private IEnumerator FinalizarNivel()
    {
        string[] narracionFinal = {
            "La rejilla se abre.",
            "El aire frío me envuelve mientras avanzo."
        };
        narracionManager?.Narrar(narracionFinal);
        yield return new WaitUntil(() => narracionManager == null || !narracionManager.EstaActivo());

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
