using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VestibuloManager : MonoBehaviour
{
    public static VestibuloManager Instance { get; private set; }

    [Header("Fade")]
    [SerializeField] private CanvasGroup pantallaFade;
    [SerializeField] private float duracionFade = 1.5f;

    [Header("Jumpscare")]
    [SerializeField] private GameObject panelJumpscare;
    [SerializeField] private AudioClip sonidoJumpscare;
    [SerializeField] private AudioClip sonidoPasos;
    [SerializeField] private float duracionJumpscare = 1f;
    [SerializeField] private float tiempoLlegarPadre = 3f;

    [Header("Referencias")]
    [SerializeField] private List<ObjetoSonoro> objetosSonoros;
    [SerializeField] private Transform spawnJugador;

    [Header("Linterna")]
    [SerializeField] private Light luzLinterna;

    [Header("Siguiente escena")]
    [SerializeField] private string escenaSiguiente = "Nivel5";

    // Estado
    private bool nivelTerminado = false;
    private bool padreActivo    = false;
    private bool reiniciando    = false;
    private AudioSource audioSource;
    private PlayerController player;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        player = FindObjectOfType<PlayerController>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    IEnumerator Start()
    {
        player?.SetBloqueado(true);

        if (pantallaFade != null) pantallaFade.alpha = 1f;
        if (panelJumpscare != null) panelJumpscare.SetActive(false);

        // Linterna tensión baja
        if (luzLinterna != null)
        {
            luzLinterna.color     = new Color(1f, 0.95f, 0.7f);
            luzLinterna.intensity = 0.85f;
        }

        yield return StartCoroutine(Fade(0f, duracionFade));

        // Narración de apertura
        if (NarracionManager.Instance != null)
        {
            NarracionManager.Instance.Narrar(new string[]
            {
                "El vestíbulo.",
                "Aprendí a cruzarlo sin respirar.",
                "Sin existir."
            });
            yield return StartCoroutine(EsperarNarracion(10f));
        }

        player?.SetBloqueado(false);
    }

    // ─── Recibir ruido ────────────────────────────────────────────────────────

    public void RecibirRuido(int nivel)
    {
        if (nivelTerminado || reiniciando) return;

        Debug.Log($"[Vestíbulo] Ruido nivel {nivel} recibido");

        // Narración contextual
        if (NarracionManager.Instance != null)
        {
            if (nivel == 1)      NarracionManager.Instance.Narrar("Cuidado.");
            else if (nivel == 2) NarracionManager.Instance.Narrar("Demasiado ruido.");
            else                 NarracionManager.Instance.Narrar("Lo escuché.");
        }

        // Activar secuencia del padre si no está activo
        if (!padreActivo)
        {
            padreActivo = true;
            float velocidad = nivel == 1 ? tiempoLlegarPadre :
                              nivel == 2 ? tiempoLlegarPadre * 0.6f :
                                          tiempoLlegarPadre * 0.3f;
            StartCoroutine(SecuenciaPadre(velocidad));
        }
    }

    // ─── Secuencia del padre ──────────────────────────────────────────────────

    private IEnumerator SecuenciaPadre(float tiempoLlegar)
    {
        Debug.Log($"[Vestíbulo] Padre activado, llega en {tiempoLlegar}s");

        // Pasos acercándose
        if (sonidoPasos != null)
        {
            audioSource.clip   = sonidoPasos;
            audioSource.loop   = true;
            audioSource.volume = 0.8f;
            audioSource.Play();
        }

        // Esperar el tiempo según nivel de ruido
        yield return new WaitForSeconds(tiempoLlegar);

        if (nivelTerminado || reiniciando) yield break;

        // Detener pasos
        audioSource.Stop();

        // Jumpscare
        yield return StartCoroutine(TriggerJumpscare());
    }

    private IEnumerator TriggerJumpscare()
{
    Debug.Log("[Vestíbulo] JUMPSCARE ACTIVADO");

    // Parar el fade primero para que no tape el jumpscare
    if (pantallaFade != null) pantallaFade.alpha = 0f;

    if (sonidoJumpscare != null)
        audioSource.PlayOneShot(sonidoJumpscare);

    panelJumpscare?.SetActive(true);
    Debug.Log("[Vestíbulo] Panel activo: " + panelJumpscare?.activeSelf);

    // Esperar sin hacer nada mas
    yield return new WaitForSeconds(duracionJumpscare);

    panelJumpscare?.SetActive(false);

    yield return new WaitForSeconds(0.5f);

    yield return StartCoroutine(ReiniciarNivel());
}

    // ─── Reinicio ─────────────────────────────────────────────────────────────

    private IEnumerator ReiniciarNivel()
    {
        reiniciando = true;
        padreActivo = false;
        player?.SetBloqueado(true);
        audioSource.Stop();

        yield return StartCoroutine(Fade(1f, duracionFade * 0.5f));

        // Narración breve
        if (NarracionManager.Instance != null)
        {
            NarracionManager.Instance.Narrar(new string[] { "Sus pasos.", "Otra vez." });
            yield return StartCoroutine(EsperarNarracion(5f));
        }

        // Mover jugador al spawn
        if (spawnJugador != null && player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = spawnJugador.position;
            if (cc != null) cc.enabled = true;
        }

        // Reiniciar objetos sonoros
        foreach (var obj in objetosSonoros)
            if (obj != null) { obj.gameObject.SetActive(false); obj.gameObject.SetActive(true); }

        yield return StartCoroutine(Fade(0f, duracionFade));

        reiniciando = false;
        player?.SetBloqueado(false);
    }

    // ─── Zona de salida ───────────────────────────────────────────────────────

    public void OnJugadorEscapo()
    {
        if (nivelTerminado) return;
        nivelTerminado = true;
        StartCoroutine(TerminarNivel());
    }

    private IEnumerator TerminarNivel()
    {
        player?.SetBloqueado(true);
        audioSource.Stop();

        // Linterna resolución
        if (luzLinterna != null)
        {
            luzLinterna.color     = Color.white;
            luzLinterna.intensity = 1f;
        }

        if (NarracionManager.Instance != null)
        {
            NarracionManager.Instance.Narrar(new string[]
            {
                "Llegué al otro lado.",
                "Hoy fue uno de los días buenos."
            });
            yield return StartCoroutine(EsperarNarracion(10f));
        }

        yield return StartCoroutine(Fade(1f, duracionFade));
        SceneManager.LoadScene(escenaSiguiente);
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
}