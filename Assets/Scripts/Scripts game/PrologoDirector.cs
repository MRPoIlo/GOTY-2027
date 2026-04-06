using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GOTY - Director de la Escena 1 (Prólogo).
///
/// Controla la secuencia narrativa de inicio:
///  1. Fade in desde negro
///  2. Narración inicial de Lena
///  3. Activar movimiento del jugador
///  4. Trigger de zona para narración ambiental
///  5. (Futuro) Carga de Escena 2 al resolver la mecánica de salida
/// </summary>
public class PrologoDirector : MonoBehaviour
{
    [Header("UI Fade")]
    [SerializeField] private CanvasGroup pantallaFade;  // Image negra en Canvas
    [SerializeField] private float duracionFade = 2.5f;

    [Header("Narración de apertura")]
    [SerializeField, TextArea(2, 6)]
    private string[] narracionApertura = new string[]
    {
        "Esta casa… la recuerdo.",
        "Hay algo que no me deja salir.",
        "Necesito encontrar la puerta."
    };

    [Header("Narración al llegar al pasillo")]
    [SerializeField, TextArea(2, 6)]
    private string[] narracionPasillo = new string[]
    {
        "El pasillo siempre olía a madera vieja.",
        "Aquí aprendí a caminar en silencio."
    };

    [Header("Escena siguiente")]
    [SerializeField] private string nombreEscenaSiguiente = "Escena2_Sala";

    // Referencias
    private PlayerController player;
    private bool proloGoTerminado = false;

    void Awake()
    {
        player = FindObjectOfType<PlayerController>();
        player?.SetBloqueado(true);

        // Empezar en negro
        if (pantallaFade != null)
            pantallaFade.alpha = 1f;
    }

    IEnumerator Start()
    {
        // Paso 1: Fade in
        yield return StartCoroutine(FadeHacia(0f, duracionFade));

        // Paso 2: Narración de apertura
        NarracionManager.Instance?.Narrar(narracionApertura);

        // Esperar que la narración termine antes de liberar al jugador
        yield return new WaitUntil(() => !NarracionManager.Instance.EstaActivo());

        // Paso 3: Liberar movimiento
        player?.SetBloqueado(false);
    }

    // ─── Triggers de zona (arrastra estos GameObjects vacíos a la escena) ─────

    /// <summary>
    /// Llama este método desde un TriggerZona ubicado en el pasillo.
    /// Ejemplo: GetComponent<TriggerZona>().OnZonaActivada.AddListener(...)
    /// </summary>
    public void ActivarNarracionPasillo()
    {
        NarracionManager.Instance?.Narrar(narracionPasillo);
    }

    /// <summary>
    /// Llamar cuando el jugador resuelve la mecánica de salida del prólogo.
    /// </summary>
    public void TerminarPrologo()
    {
        if (proloGoTerminado) return;
        proloGoTerminado = true;
        StartCoroutine(TransicionHaciaEscena2());
    }

    private IEnumerator TransicionHaciaEscena2()
    {
        player?.SetBloqueado(true);
        yield return StartCoroutine(FadeHacia(1f, duracionFade));
        SceneManager.LoadScene(nombreEscenaSiguiente);
    }

    // ─── Utilidad Fade ────────────────────────────────────────────────────────

    private IEnumerator FadeHacia(float alphaObjetivo, float duracion)
    {
        if (pantallaFade == null) yield break;

        float alphaInicial = pantallaFade.alpha;
        float t = 0f;

        while (t < duracion)
        {
            t += Time.deltaTime;
            pantallaFade.alpha = Mathf.Lerp(alphaInicial, alphaObjetivo, t / duracion);
            yield return null;
        }

        pantallaFade.alpha = alphaObjetivo;
    }
}