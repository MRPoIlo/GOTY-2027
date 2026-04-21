using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GOTY — Director del Nivel 3 (Ático).
/// Orquesta: entrada, exploración de cuadros, menú de ordenamiento,
/// cuadro familiar, secuencia de la sombra mamá y salida.
/// </summary>
public class AticoManager : MonoBehaviour
{
    public static AticoManager Instance { get; private set; }

    [Header("Fade")]
    [SerializeField] private CanvasGroup pantallaFade;
    [SerializeField] private float duracionFade = 1.8f;

    [Header("Cuadros del nivel")]
    [SerializeField] private List<CuadroRecuerdo> cuadros;

    [Header("Cuadro familiar completo")]
    [Tooltip("El cuadro grande que aparece en la pared al ordenar correctamente")]
    [SerializeField] private GameObject cuadroFamiliarCompleto;

    [Header("Sombra del padre (bloqueando la puerta)")]
    [SerializeField] private GameObject sombraPadre;

    [Header("Sombra de la mamá")]
    [SerializeField] private SombraMama sombraMama;

    [Header("Siguiente escena")]
    [SerializeField] private string escenaSiguiente = "Nivel4";

    // Estado
    private List<CuadroRecuerdo> cuadrosExaminados = new List<CuadroRecuerdo>();
    private bool menuAbierto = false;
    private bool nivelResuelto = false;

    private PlayerController player;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        player = FindObjectOfType<PlayerController>();
    }

    IEnumerator Start()
    {
        player?.SetBloqueado(true);

        if (pantallaFade != null) pantallaFade.alpha = 1f;

        yield return StartCoroutine(Fade(0f, duracionFade));

        // Narración de entrada al ático
        NarracionManager.Instance?.Narrar(new string[]
        {
            "El ático.",
            "Aquí guardaban todo lo que no querían recordar.",
            "Y todo lo que no podían olvidar."
        });

        yield return new WaitUntil(() =>
            NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());

        player?.SetBloqueado(false);
    }

    // ─── Registro de cuadros examinados ──────────────────────────────────────

    /// <summary>
    /// CuadroRecuerdo lo llama al ser examinado.
    /// Cuando los 4 están examinados, abre el menú de ordenamiento.
    /// </summary>
    public void RegistrarCuadroExaminado(CuadroRecuerdo cuadro)
    {
        if (!cuadrosExaminados.Contains(cuadro))
            cuadrosExaminados.Add(cuadro);

        Debug.Log($"[Ático] Cuadros examinados: {cuadrosExaminados.Count}/{cuadros.Count}");

        // Cuando el jugador examina todos, abrir el menú
        if (cuadrosExaminados.Count >= cuadros.Count && !menuAbierto)
        {
            menuAbierto = true;
            StartCoroutine(AbrirMenuConDelay());
        }
    }

    private IEnumerator AbrirMenuConDelay()
    {
        // Esperar que termine la narración del último cuadro
        yield return new WaitUntil(() =>
            NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());

        yield return new WaitForSeconds(0.8f);

        NarracionManager.Instance?.Narrar(
            "Recuerdo estos momentos. Pero no sé en qué orden pasaron.");

        yield return new WaitUntil(() =>
            NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());

        MenuOrdenamiento.Instance?.AbrirMenu(cuadrosExaminados);
    }

    // ─── Resultado del menú de ordenamiento ──────────────────────────────────

    /// <summary>MenuOrdenamiento llama esto cuando el orden es correcto.</summary>
    public void OnOrdenCorrecto()
    {
        StartCoroutine(SecuenciaOrdenCorrecto());
    }

    private IEnumerator SecuenciaOrdenCorrecto()
    {
        // Marcar todos los cuadros como resueltos
        foreach (var cuadro in cuadros)
            cuadro.MarcarResuelto();

        // Narración de comprensión
        NarracionManager.Instance?.Narrar(new string[]
        {
            "Sí. Así fue.",
            "Lo recuerdo ahora."
        });

        yield return new WaitUntil(() =>
            NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());

        // Mostrar cuadro familiar completo
        if (cuadroFamiliarCompleto != null)
        {
            cuadroFamiliarCompleto.SetActive(true);

            NarracionManager.Instance?.Narrar(new string[]
            {
                "Éramos una familia normal. Por fuera."
            });

            yield return new WaitUntil(() =>
                NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());
        }

        yield return new WaitForSeconds(1f);

        // Iniciar secuencia de la sombra mamá
        if (sombraMama != null)
        {
            sombraMama.gameObject.SetActive(true);
            sombraMama.IniciarSecuencia();
        }
    }

    /// <summary>SombraMama llama esto cuando termina su secuencia.</summary>
    public void OnSecuenciaMamaTerminada()
    {
        if (!nivelResuelto)
        {
            nivelResuelto = true;
            StartCoroutine(TerminarNivel());
        }
    }

    // ─── Transición final ─────────────────────────────────────────────────────

    private IEnumerator TerminarNivel()
    {
        player?.SetBloqueado(true);

        NarracionManager.Instance?.Narrar(new string[]
        {
            "La puerta está abierta.",
            "Puedo seguir."
        });

        yield return new WaitUntil(() =>
            NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());

        // Guardar progreso en GameManager
        if (GameManager2.Instance != null)
            GameManager2.Instance.prologoCompletado = true; // Reutilizar o agregar nivel3Completado

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