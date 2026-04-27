using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SalaCocinaManager : MonoBehaviour
{
    public static SalaCocinaManager Instance { get; private set; }

    [Header("Fade")]
    [SerializeField] private CanvasGroup pantallaFade;
    [SerializeField] private float duracionFade = 1.5f;

    [Header("Referencias de escena")]
    [SerializeField] private GameObject televisorRoto;
    [SerializeField] private GameObject cocinaDesordenada;
    [SerializeField] private List<GameObject> cajasBloqueando;
    [SerializeField] private Transform spawnJugador;

    [Header("Padre")]
    [SerializeField] private AudioClip sonidoPasos;
    [SerializeField] private float tiempoLlegarPadre = 4f;
    private bool padreActivo = false;

    [Header("Siguiente escena")]
    [SerializeField] private string escenaSiguiente = "Nivel6Sotano";

    // Estado
    private bool nivelTerminado = false;
    private bool reiniciando = false;
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

        yield return StartCoroutine(Fade(0f, duracionFade));

        // Narración inicial
        NarracionManager.Instance?.Narrar(new string[]
        {
            "La sala y la cocina.",
            "El televisor encendido, pero roto.",
            "La cocina, un caos que habla por sí sola."
        });
        yield return StartCoroutine(EsperarNarracion(10f));

        player?.SetBloqueado(false);
    }

    // ─── Interacción con cajas ───────────────────────────────────────────────
    public void MoverCaja(GameObject caja)
    {
        if (cajasBloqueando.Contains(caja))
        {
            cajasBloqueando.Remove(caja);
            caja.SetActive(false);
            Debug.Log("[SalaCocina] Caja movida. Restantes: " + cajasBloqueando.Count);

            if (cajasBloqueando.Count == 0)
            {
                NarracionManager.Instance?.Narrar(new string[]
                {
                    "El camino al sótano está libre.",
                    "Pero debo tener cuidado con él."
                });
            }
        }
    }

    // ─── Activación del padre ────────────────────────────────────────────────
    public void EscucharRuido()
    {
        if (padreActivo || nivelTerminado || reiniciando) return;

        padreActivo = true;
        StartCoroutine(SecuenciaPadre());
    }

    private IEnumerator SecuenciaPadre()
    {
        Debug.Log("[SalaCocina] Padre activado, llega en " + tiempoLlegarPadre + "s");

        if (sonidoPasos != null)
        {
            audioSource.clip = sonidoPasos;
            audioSource.loop = true;
            audioSource.Play();
        }

        yield return new WaitForSeconds(tiempoLlegarPadre);

        if (nivelTerminado || reiniciando) yield break;

        audioSource.Stop();
        yield return StartCoroutine(ReiniciarNivel());
    }

    // ─── Reinicio ────────────────────────────────────────────────────────────
    private IEnumerator ReiniciarNivel()
    {
        reiniciando = true;
        padreActivo = false;
        player?.SetBloqueado(true);

        yield return StartCoroutine(Fade(1f, duracionFade * 0.5f));

        NarracionManager.Instance?.Narrar(new string[]
        {
            "Sus pasos retumban.",
            "Debo esconderme mejor."
        });
        yield return StartCoroutine(EsperarNarracion(5f));

        // Reset jugador
        if (spawnJugador != null && player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = spawnJugador.position;
            if (cc != null) cc.enabled = true;
        }

        // Reset cajas
        foreach (var caja in cajasBloqueando)
            if (caja != null) { caja.SetActive(true); }

        yield return StartCoroutine(Fade(0f, duracionFade));

        reiniciando = false;
        player?.SetBloqueado(false);
    }

    // ─── Salida al sótano ────────────────────────────────────────────────────
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

        NarracionManager.Instance?.Narrar(new string[]
        {
            "Abrí el camino al sótano.",
            "El maltrato está en cada rincón."
        });
        yield return StartCoroutine(EsperarNarracion(10f));

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
