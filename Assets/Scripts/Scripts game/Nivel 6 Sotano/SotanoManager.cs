using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GOTY — Director del Nivel 6 (Sótano).
/// Maneja fragmentos de llave, estado emocional de la linterna,
/// captura del jugador y transición final.
/// </summary>
public class SotanoManager : MonoBehaviour
{
    public static SotanoManager Instance { get; private set; }

    [Header("Fade")]
    [SerializeField] private CanvasGroup pantallaFade;
    [SerializeField] private float duracionFade = 1.5f;

    [Header("Jumpscare")]
    [SerializeField] private GameObject panelJumpscare;
    [SerializeField] private AudioClip  sonidoJumpscare;
    [SerializeField] private float      duracionJumpscare = 1f;

    [Header("Referencias")]
    [SerializeField] private PadrePatrullador padre;
    [SerializeField] private PuertaMetalica   puertaMetalica;
    [SerializeField] private Transform        spawnJugador;
    [SerializeField] private Transform        posicionInicialPadre;

    [Header("Linterna emocional")]
    [SerializeField] private Light luzLinterna;

    [Header("Fragmentos")]
    [SerializeField] private FragmentoLlave[] fragmentos; // los 3 en la escena
    private int fragmentosRecogidos = 0;

    [Header("Siguiente escena")]
    [SerializeField] private string escenaSiguiente = "CinematicaFinal";

    // Estado
    private bool capturado      = false;
    private bool nivelTerminado = false;
    private bool reiniciando    = false;
    private AudioSource audioSource;
    private PlayerController player;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        player      = FindObjectOfType<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    IEnumerator Start()
    {
        player?.SetBloqueado(true);

        if (pantallaFade    != null) pantallaFade.alpha = 1f;
        if (panelJumpscare  != null) panelJumpscare.SetActive(false);

        // Linterna en estado Miedo — rojo oscuro, parpadeo (según GDD)
        if (luzLinterna != null)
        {
            luzLinterna.color     = new Color(0.6f, 0.1f, 0.05f);
            luzLinterna.intensity = 0.4f;
            StartCoroutine(ParpadeLinterna());
        }

        yield return StartCoroutine(Fade(0f, duracionFade));

        NarracionManager.Instance?.Narrar(new string[]
        {
            "El sótano.",
            "Nunca bajaba aquí solo.",
            "Nunca."
        });

        yield return StartCoroutine(EsperarNarracion(10f));

        player?.SetBloqueado(false);
    }

    // ─── Parpadeo linterna ────────────────────────────────────────────────────

    private IEnumerator ParpadeLinterna()
    {
        while (!nivelTerminado)
        {
            if (luzLinterna != null && !capturado)
            {
                float base_intensity = fragmentosRecogidos > 0
                    ? 0.4f + fragmentosRecogidos * 0.15f // aumenta al recoger fragmentos
                    : 0.4f;

                luzLinterna.intensity = base_intensity *
                    (0.6f + 0.4f * Mathf.PerlinNoise(Time.time * 3f, 0f));
            }
            yield return null;
        }
    }

    // ─── Fragmentos de llave ──────────────────────────────────────────────────

    public void RegistrarFragmento(FragmentoLlave fragmento)
    {
        fragmentosRecogidos++;
        Debug.Log($"[Sótano] Fragmentos: {fragmentosRecogidos}/3");

        // La linterna se estabiliza un poco al recoger cada fragmento
        if (luzLinterna != null)
            luzLinterna.color = Color.Lerp(
                new Color(0.6f, 0.1f, 0.05f), // rojo
                Color.white,                    // blanco
                fragmentosRecogidos / 3f);

        if (fragmentosRecogidos >= 3)
            OnTodosLosFragmentosRecogidos();
    }

    private void OnTodosLosFragmentosRecogidos()
    {
        NarracionManager.Instance?.Narrar(new string[]
        {
            "Los tres fragmentos.",
            "La puerta metálica. Tengo que llegar."
        });

        puertaMetalica?.Habilitar();
    }

    // ─── Puerta activada ──────────────────────────────────────────────────────

    /// <summary>PuertaMetalica llama esto al iniciar la cuenta regresiva.</summary>
    public void OnPuertaActivada()
    {
        // El padre empieza a correr directo hacia la puerta
        if (padre != null && puertaMetalica != null)
        {
            var agent = padre.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = 5f; // velocidad máxima
                agent.SetDestination(puertaMetalica.transform.position);
            }
        }

        NarracionManager.Instance?.Narrar("Viene hacia acá. Rápido.");
    }

    // ─── Jugador escapó ───────────────────────────────────────────────────────

    public void OnJugadorEscapo()
    {
        if (nivelTerminado) return;
        nivelTerminado = true;
        StartCoroutine(TerminarNivel());
    }

    private IEnumerator TerminarNivel()
    {
        padre?.Detener();
        player?.SetBloqueado(true);

        // Linterna a blanco puro — resolución total
        if (luzLinterna != null)
        {
            luzLinterna.color     = Color.white;
            luzLinterna.intensity = 2f;
        }

        NarracionManager.Instance?.Narrar(new string[]
        {
            "La puerta cedió.",
            "Subí las escaleras sin mirar atrás.",
            "Por primera vez en años."
        });

        yield return StartCoroutine(EsperarNarracion(12f));

        // Fade a blanco — no negro — es un final de esperanza
        yield return StartCoroutine(FadeBlanco(duracionFade));

        SceneManager.LoadScene(escenaSiguiente);
    }

    // ─── Jugador capturado ────────────────────────────────────────────────────

    public void OnJugadorCapturado()
    {
        if (capturado || reiniciando || nivelTerminado) return;
        StartCoroutine(SecuenciaCaptura());
    }

    private IEnumerator SecuenciaCaptura()
    {
        capturado = true;
        padre?.Detener();
        player?.SetBloqueado(true);

        // Jumpscare
        if (sonidoJumpscare != null)
            audioSource.PlayOneShot(sonidoJumpscare);

        if (panelJumpscare != null)
            panelJumpscare.SetActive(true);

        yield return new WaitForSeconds(duracionJumpscare);

        if (panelJumpscare != null)
            panelJumpscare.SetActive(false);

        // Reiniciar
        yield return StartCoroutine(ReiniciarDesdeEntrada());
    }

    private IEnumerator ReiniciarDesdeEntrada()
    {
        reiniciando = true;

        yield return StartCoroutine(Fade(1f, duracionFade * 0.5f));

        NarracionManager.Instance?.Narrar(new string[]
        {
            "Me atrapó.",
            "Tengo que ser más cuidadoso."
        });

        yield return StartCoroutine(EsperarNarracion(6f));

        // Mover jugador al spawn
        if (spawnJugador != null && player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = spawnJugador.position;
            if (cc != null) cc.enabled = true;
        }

        // Reiniciar padre
        if (padre != null && posicionInicialPadre != null)
            padre.Reiniciar(posicionInicialPadre.position);

        // Reiniciar fragmentos recogidos NO — el jugador los conserva
        // Solo reinicia la posición

        // Linterna vuelve al estado de miedo
        if (luzLinterna != null)
        {
            luzLinterna.color     = Color.Lerp(
                new Color(0.6f, 0.1f, 0.05f),
                Color.white,
                fragmentosRecogidos / 3f);
            luzLinterna.intensity = 0.4f + fragmentosRecogidos * 0.15f;
        }

        yield return StartCoroutine(Fade(0f, duracionFade));

        capturado   = false;
        reiniciando = false;
        player?.SetBloqueado(false);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private IEnumerator EsperarNarracion(float timeout)
    {
        yield return new WaitForSeconds(0.2f);
        float t = 0f;
        while (t < timeout)
        {
            if (NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo())
                yield break;
            t += Time.deltaTime;
            yield return null;
        }
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

    private IEnumerator FadeBlanco(float duracion)
    {
        // Fade a blanco usando la luz de la linterna como fuente
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            if (luzLinterna != null)
                luzLinterna.intensity = Mathf.Lerp(2f, 20f, t / duracion);
            yield return null;
        }
        // Luego fade normal a negro para la transición de escena
        yield return StartCoroutine(Fade(1f, 0.3f));
    }
}