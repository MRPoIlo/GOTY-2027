using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrologoManager : MonoBehaviour
{
    public static PrologoManager Instance { get; private set; }

    private static readonly string[] NarracionDespiertar = new[]
    {
        "Esta habitación…",
        "La recuerdo.",
        "Hay algo que no me deja salir."
    };

    private static readonly string[] NarracionPasillo = new[]
    {
        "El pasillo siempre olía a madera vieja.",
        "Aquí aprendí a caminar sin hacer ruido."
    };

    private static readonly string[] NarracionPasillo2 = new[]
    {
        "Aqui no habian unas escaleras?",
        "Debo buscar el modo de salir de acá"
    };

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
    [SerializeField] private GameObject puertaSalida; // 🔥 ahora sí puedes arrastrar cualquier objeto

    private int objetosExaminados = 0;
    private bool puertaDesbloqueada = false;
    private bool finalizando = false;

    private PlayerController player;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        player = FindObjectOfType<PlayerController>();
        player?.SetBloqueado(true);

        if (pantallaFade != null)
            pantallaFade.alpha = 1f;

        if (puertaSalida == null)
            Debug.LogError("No asignaste la puerta en el Inspector");
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

    public void EntrarAlPasillo()
    {
        NarracionManager.Instance?.Narrar(NarracionPasillo);
    }

    public void EntrarAlPasillo2()
    {
        NarracionManager.Instance?.Narrar(NarracionPasillo2);
    }

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

        if (puertaSalida != null)
        {
            // 🔥 Aquí decides qué hace la puerta:
            puertaSalida.SetActive(true); // ejemplo: aparece
            // o puedes hacer:
            // puertaSalida.GetComponent<Animator>().SetTrigger("Open");
        }

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