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

    [Header("Padre")]
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

    [Header("Música general")]
    [SerializeField] private AudioSource musicaGeneral;

    // Estado
    private bool nivelTerminado = false;
    private bool padreActivo = false;
    private bool reiniciando = false;

    private AudioSource audioSource;
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

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    IEnumerator Start()
    {
        player?.SetBloqueado(true);

        if (pantallaFade != null)
            pantallaFade.alpha = 1f;

        // Linterna tensión baja
        if (luzLinterna != null)
        {
            luzLinterna.color = new Color(1f, 0.95f, 0.7f);
            luzLinterna.intensity = 0.85f;
        }

        yield return StartCoroutine(Fade(0f, duracionFade));

        // Narración inicio
        if (NarracionManager.Instance != null)
        {
            NarracionManager.Instance.Narrar(new string[]
            {
                "El vestíbulo.",
                "Aprendí a cruzarlo sin respirar.",
                "Sin existir."
            });

            yield return StartCoroutine(
                EsperarNarracion(10f));
        }

        // Música general
        if (musicaGeneral != null)
        {
            musicaGeneral.loop = true;
            musicaGeneral.Play();
        }

        player?.SetBloqueado(false);
    }

    // ─────────────────────────────────────
    // RECIBIR RUIDO
    // ─────────────────────────────────────

    public void RecibirRuido(int nivel)
    {
        if (nivelTerminado || reiniciando)
            return;

        Debug.Log($"[Vestíbulo] Ruido nivel {nivel}");

        // Narración contextual
        if (NarracionManager.Instance != null)
        {
            if (nivel == 1)
                NarracionManager.Instance.Narrar(
                    "Cuidado.");
            else if (nivel == 2)
                NarracionManager.Instance.Narrar(
                    "Demasiado ruido.");
            else
                NarracionManager.Instance.Narrar(
                    "Lo escuché.");
        }

        // Activar padre
        if (!padreActivo)
        {
            padreActivo = true;

            float velocidad =
                nivel == 1 ? tiempoLlegarPadre :
                nivel == 2 ? tiempoLlegarPadre * 0.6f :
                             tiempoLlegarPadre * 0.3f;

            StartCoroutine(
                SecuenciaPadre(velocidad));
        }
    }

    // ─────────────────────────────────────
    // SECUENCIA PADRE
    // ─────────────────────────────────────

    private IEnumerator SecuenciaPadre(float tiempoLlegar)
    {
        Debug.Log(
            $"[Vestíbulo] Padre llega en {tiempoLlegar}s");

        // Pasos
        if (sonidoPasos != null)
        {
            audioSource.clip = sonidoPasos;
            audioSource.loop = true;
            audioSource.volume = 0.8f;

            audioSource.Play();
        }

        // Esperar llegada
        yield return new WaitForSeconds(
            tiempoLlegar);

        if (nivelTerminado || reiniciando)
            yield break;

        // Detener pasos
        audioSource.Stop();

        // Activar Game Over
        yield return StartCoroutine(
            TriggerJumpscare());
    }

    // ─────────────────────────────────────
    // GAME OVER
    // ─────────────────────────────────────

    private IEnumerator TriggerJumpscare()
    {
        Debug.Log(
            "[Vestíbulo] GAME OVER");

        if (pantallaFade != null)
            pantallaFade.alpha = 0f;

        yield return new WaitForSeconds(
            duracionJumpscare);

        // Detener música
        if (musicaGeneral != null &&
            musicaGeneral.isPlaying)
        {
            musicaGeneral.Stop();
        }

        // Activar Game Over
        if (GameOverManager.Instance != null)
        {
            Debug.Log(
                "[Vestíbulo] Activando GameOver");

            GameOverManager.Instance
                .ActivarGameOver();
        }
        else
        {
            Debug.LogError(
                "[Vestíbulo] GameOverManager NULL");
        }
    }

    // ─────────────────────────────────────
    // REINICIO
    // ─────────────────────────────────────

    private IEnumerator ReiniciarNivel()
    {
        reiniciando = true;
        padreActivo = false;

        player?.SetBloqueado(true);

        audioSource.Stop();

        yield return StartCoroutine(
            Fade(1f, duracionFade * 0.5f));

        if (NarracionManager.Instance != null)
        {
            NarracionManager.Instance.Narrar(
                new string[]
                {
                    "Sus pasos.",
                    "Otra vez."
                });

            yield return StartCoroutine(
                EsperarNarracion(5f));
        }

        if (spawnJugador != null &&
            player != null)
        {
            CharacterController cc =
                player.GetComponent<CharacterController>();

            if (cc != null)
                cc.enabled = false;

            player.transform.position =
                spawnJugador.position;

            if (cc != null)
                cc.enabled = true;
        }

        foreach (var obj in objetosSonoros)
        {
            if (obj != null)
            {
                obj.gameObject.SetActive(false);
                obj.gameObject.SetActive(true);
            }
        }

        yield return StartCoroutine(
            Fade(0f, duracionFade));

        reiniciando = false;

        player?.SetBloqueado(false);
    }

    // ─────────────────────────────────────
    // ESCAPAR
    // ─────────────────────────────────────

    public void OnJugadorEscapo()
    {
        if (nivelTerminado)
            return;

        nivelTerminado = true;

        StartCoroutine(TerminarNivel());
    }

    private IEnumerator TerminarNivel()
    {
        player?.SetBloqueado(true);

        audioSource.Stop();

        if (luzLinterna != null)
        {
            luzLinterna.color = Color.white;
            luzLinterna.intensity = 1f;
        }

        if (NarracionManager.Instance != null)
        {
            NarracionManager.Instance.Narrar(
                new string[]
                {
                    "Llegué al otro lado.",
                    "Hoy fue uno de los días buenos."
                });

            yield return StartCoroutine(
                EsperarNarracion(10f));
        }

        yield return StartCoroutine(
            Fade(1f, duracionFade));

        SceneManager.LoadScene(
            escenaSiguiente);
    }

    // ─────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────

    private IEnumerator EsperarNarracion(
        float timeout)
    {
        yield return new WaitForSeconds(0.2f);

        float t = 0f;

        while (t < timeout)
        {
            if (NarracionManager.Instance == null ||
                !NarracionManager.Instance.EstaActivo())
            {
                yield break;
            }

            t += Time.deltaTime;

            yield return null;
        }
    }

    private IEnumerator Fade(
        float objetivo,
        float duracion)
    {
        if (pantallaFade == null)
            yield break;

        float inicio = pantallaFade.alpha;

        float t = 0f;

        while (t < duracion)
        {
            t += Time.deltaTime;

            pantallaFade.alpha =
                Mathf.Lerp(
                    inicio,
                    objetivo,
                    t / duracion);

            yield return null;
        }

        pantallaFade.alpha = objetivo;
    }
}