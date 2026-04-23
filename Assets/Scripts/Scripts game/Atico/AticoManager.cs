using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AticoManager : MonoBehaviour
{
    public static AticoManager Instance { get; private set; }

    [Header("Fade")]
    [SerializeField] private CanvasGroup pantallaFade;
    [SerializeField] private float duracionFade = 1.8f;

    [Header("Cuadros del nivel")]
    [SerializeField] private List<CuadroRecuerdo> cuadros;

    [Header("Cuadro familiar completo")]
    [SerializeField] private GameObject cuadroFamiliarCompleto;

    [Header("Sombra del padre (bloqueando la puerta)")]
    [SerializeField] private GameObject sombraPadre;

    [Header("Sombra de la mamá")]
    [SerializeField] private SombraMama sombraMama;

    [Header("Siguiente escena")]
    [SerializeField] private string escenaSiguiente = "Nivel4";

    // Estado interno
    private List<CuadroRecuerdo> cuadrosExaminados = new List<CuadroRecuerdo>();
    private bool menuAbierto = false;
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
    public void RegistrarCuadroExaminado(CuadroRecuerdo cuadro)
    {
        if (!cuadrosExaminados.Contains(cuadro))
            cuadrosExaminados.Add(cuadro);

        if (cuadrosExaminados.Count >= cuadros.Count && !menuAbierto)
        {
            menuAbierto = true;
            StartCoroutine(AbrirMenuConDelay());
        }
    }

    private IEnumerator AbrirMenuConDelay()
    {
        yield return new WaitUntil(() =>
            NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());

        NarracionManager.Instance?.Narrar(
            "Recuerdo estos momentos. Pero no sé en qué orden pasaron.");

        yield return new WaitUntil(() =>
            NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());

        MenuOrdenamiento.Instance?.AbrirMenu(cuadrosExaminados);
    }

    // ─── Resultado del menú de ordenamiento ──────────────────────────────────
    public void OnOrdenCorrecto()
    {
        StartCoroutine(SecuenciaOrdenCorrecto());
    }

    private IEnumerator SecuenciaOrdenCorrecto()
    {
        foreach (var cuadro in cuadros)
            cuadro.MarcarResuelto();

        NarracionManager.Instance?.Narrar(new string[]
        {
            "Sí. Así fue.",
            "Lo recuerdo ahora."
        });

        yield return new WaitUntil(() =>
            NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo());

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

        if (sombraMama != null)
        {
            sombraMama.gameObject.SetActive(true);
            sombraMama.IniciarSecuencia();
        }
    }

    /// <summary>SombraMama llama esto cuando termina su secuencia.</summary>
    public void OnSecuenciaMamaTerminada()
    {
        if (!puertaDesbloqueada)
        {
            puertaDesbloqueada = true;
            NarracionManager.Instance?.Narrar(new string[]
            {
                "La puerta está abierta.",
                "Puedo salir cuando quiera."
            });
            Debug.Log("[Ático] Puerta desbloqueada.");
        }
    }

    // ─── Llamado desde la puerta ─────────────────────────────────────────────
    public void IntentarSalir()
    {
        if (finalizando) return;

        if (puertaDesbloqueada)
        {
            StartCoroutine(TerminarNivel());
        }
        else
        {
            NarracionManager.Instance?.Narrar(new string[]
            {
                "Todavía no puedo salir.",
                "Algo me retiene aquí."
            });
        }
    }

    private IEnumerator TerminarNivel()
    {
        finalizando = true;
        player?.SetBloqueado(true);

        NarracionManager.Instance?.Narrar(new string[]
        {
            "La puerta cede.",
            "El silencio me acompaña hacia lo desconocido."
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
