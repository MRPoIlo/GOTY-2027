    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// GOTY — Director del Nivel 6 (Sótano).
    /// Maneja fragmentos de llave, estado emocional de la linterna,
    /// captura del jugador y transición final (bueno o malo).
    /// </summary>
    public class SotanoManager : MonoBehaviour
    {
        public static SotanoManager Instance { get; private set; }

        [Header("Fade")]
        [SerializeField] private CanvasGroup pantallaFade;
        [SerializeField] private float duracionFade = 1.5f;

        [Header("Jumpscare")]
        [SerializeField] private GameObject panelJumpscare;
        [SerializeField] private AudioClip sonidoJumpscare;
        [SerializeField] private float duracionJumpscare = 1f;

        [Header("Referencias")]
        [SerializeField] private PadrePatrullador padre;
        [SerializeField] private PuertaMetalica puertaMetalica;
        [SerializeField] private Transform spawnJugador;
        [SerializeField] private Transform posicionInicialPadre;

        [Header("Linterna emocional")]
        [SerializeField] private Light luzLinterna;

        [Header("Fragmentos")]
        [SerializeField] private FragmentoLlave[] fragmentos;
        private int fragmentosRecogidos = 0;
        public int FragmentosRecogidos => fragmentosRecogidos;

        [Header("Finales")]
        [SerializeField] private string escenaFinalBueno = "FinalBueno";
        [SerializeField] private string escenaFinalMalo = "FinalMalo";

        private bool finalBueno = false;

        // Estado
        private bool capturado = false;
        private bool nivelTerminado = false;
        private bool reiniciando = false;
        private AudioSource audioSource;
        private PlayerController player;

        // ─────────────────────────────────────────────
        // Awake
        // ─────────────────────────────────────────────
        private void Awake()
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

        // ─────────────────────────────────────────────
        // Start
        // ─────────────────────────────────────────────
        private IEnumerator Start()
        {
            player?.SetBloqueado(true);

            if (pantallaFade != null)
                pantallaFade.alpha = 1f;

            if (panelJumpscare != null)
                panelJumpscare.SetActive(false);

            // Verificar si el jugador desbloqueó el final bueno
            if (GameManager2.Instance != null)
            {
                finalBueno = GameManager2.Instance.DesbloqueaFinalBueno();

                Debug.Log(
                    $"[SotanoManager] Recuerdos acumulados: " +
                    $"{GameManager2.Instance.objetosBuenosRecogidos}/" +
                    $"{GameManager2.Instance.minimoFinalBueno}"
                );

                Debug.Log($"[SotanoManager] ¿Final bueno desbloqueado?: {finalBueno}");
            }
            else
            {
                Debug.LogWarning("[SotanoManager] No se encontró GameManager2.");
                finalBueno = false;
            }

            // Linterna en estado de miedo
            if (luzLinterna != null)
            {
                luzLinterna.color = new Color(0.6f, 0.1f, 0.05f);
                luzLinterna.intensity = 0.4f;
                StartCoroutine(ParpadeLinterna());
            }

            yield return StartCoroutine(Fade(0f, duracionFade));

            // Narración inicial
            if (finalBueno)
            {
                NarracionManager.Instance?.Narrar(new string[]
                {
                    "El sótano.",
                    "Nunca bajaba aquí solo.",
                    "Nunca.",
                    "Pero me siento diferente.",
                    "Recordar mis juguetes me ayudó."
                });
            }
            else
            {
                NarracionManager.Instance?.Narrar(new string[]
                {
                    "El sótano.",
                    "Nunca bajaba aquí solo.",
                    "Nunca."
                });
            }

            yield return StartCoroutine(EsperarNarracion(15f));

            player?.SetBloqueado(false);
        }

        // ─────────────────────────────────────────────
        // Parpadeo linterna
        // ─────────────────────────────────────────────
        private IEnumerator ParpadeLinterna()
        {
            while (!nivelTerminado)
            {
                if (luzLinterna != null && !capturado)
                {
                    float intensidadBase = fragmentosRecogidos > 0
                        ? 0.4f + fragmentosRecogidos * 0.15f
                        : 0.4f;

                    luzLinterna.intensity = intensidadBase *
                        (0.6f + 0.4f * Mathf.PerlinNoise(Time.time * 3f, 0f));
                }

                yield return null;
            }
        }

        // ─────────────────────────────────────────────
        // Fragmentos
        // ─────────────────────────────────────────────
        public void RegistrarFragmento(FragmentoLlave fragmento)
        {
            fragmentosRecogidos++;
            Debug.Log($"[SotanoManager] Fragmentos: {fragmentosRecogidos}/3");

            if (luzLinterna != null)
            {
                luzLinterna.color = Color.Lerp(
                    new Color(0.6f, 0.1f, 0.05f),
                    Color.white,
                    fragmentosRecogidos / 3f
                );
            }

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

        // ─────────────────────────────────────────────
        // Puerta activada
        // ─────────────────────────────────────────────
        public void OnPuertaActivada()
        {
            if (padre != null && puertaMetalica != null)
            {
                var agent = padre.GetComponent<UnityEngine.AI.NavMeshAgent>();

                if (agent != null)
                {
                    agent.speed = 5f;
                    agent.SetDestination(puertaMetalica.transform.position);
                }
            }

            NarracionManager.Instance?.Narrar("Viene hacia acá. Rápido.");
        }

        // ─────────────────────────────────────────────
        // Jugador escapó
        // ─────────────────────────────────────────────
        public void OnJugadorEscapo()
        {
            if (nivelTerminado)
                return;

            nivelTerminado = true;
            StartCoroutine(TerminarNivel());
        }

        private IEnumerator TerminarNivel()
        {
            padre?.Detener();
            player?.SetBloqueado(true);

            if (luzLinterna != null)
            {
                luzLinterna.color = Color.white;
                luzLinterna.intensity = 2f;
            }

            yield return StartCoroutine(EsperarNarracion(12f));

            yield return StartCoroutine(FadeBlanco(duracionFade));

            string escenaDestino = finalBueno
                ? escenaFinalBueno
                : escenaFinalMalo;

            Debug.Log($"[SotanoManager] Cargando escena final: {escenaDestino}");

            SceneManager.LoadScene(escenaDestino);
        }

        // ─────────────────────────────────────────────
        // Jugador capturado
        // ─────────────────────────────────────────────
        public void OnJugadorCapturado()
        {
            if (capturado || reiniciando || nivelTerminado)
                return;

            StartCoroutine(SecuenciaCaptura());
        }

        private IEnumerator SecuenciaCaptura()
        {
            capturado = true;
            padre?.Detener();
            player?.SetBloqueado(true);

            if (sonidoJumpscare != null)
                audioSource.PlayOneShot(sonidoJumpscare);

            if (panelJumpscare != null)
                panelJumpscare.SetActive(true);

            yield return new WaitForSeconds(duracionJumpscare);

            if (panelJumpscare != null)
                panelJumpscare.SetActive(false);

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

            if (spawnJugador != null && player != null)
            {
                var cc = player.GetComponent<CharacterController>();

                if (cc != null)
                    cc.enabled = false;

                player.transform.position = spawnJugador.position;

                if (cc != null)
                    cc.enabled = true;
            }

            if (padre != null && posicionInicialPadre != null)
                padre.Reiniciar(posicionInicialPadre.position);

            if (luzLinterna != null)
            {
                luzLinterna.color = Color.Lerp(
                    new Color(0.6f, 0.1f, 0.05f),
                    Color.white,
                    fragmentosRecogidos / 3f
                );

                luzLinterna.intensity = 0.4f + fragmentosRecogidos * 0.15f;
            }

            yield return StartCoroutine(Fade(0f, duracionFade));

            capturado = false;
            reiniciando = false;
            player?.SetBloqueado(false);
        }

        // ─────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────
        private IEnumerator EsperarNarracion(float timeout)
        {
            yield return new WaitForSeconds(0.2f);

            float t = 0f;

            while (t < timeout)
            {
                if (NarracionManager.Instance == null ||
                    !NarracionManager.Instance.EstaActivo())
                    yield break;

                t += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator Fade(float objetivo, float duracion)
        {
            if (pantallaFade == null)
                yield break;

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
            float t = 0f;

            while (t < duracion)
            {
                t += Time.deltaTime;

                if (luzLinterna != null)
                    luzLinterna.intensity = Mathf.Lerp(2f, 20f, t / duracion);

                yield return null;
            }

            yield return StartCoroutine(Fade(1f, 0.3f));
        }

        // Agrega este método dentro de SotanoManager

    /// <summary>
    /// Se llama la primera vez que el jugador examina la puerta.
    /// Aquí puedes activar fragmentos y comenzar la patrulla del padre.
    /// </summary>
    public void OnPuertaExaminada()
    {
        Debug.Log("[SotanoManager] La puerta fue examinada.");

        // Activar todos los fragmentos
        foreach (var fragmento in fragmentos)
        {
            if (fragmento != null)
                fragmento.gameObject.SetActive(true);
        }

        // Iniciar patrulla del padre si tu script lo permite
        if (padre != null)
            padre.gameObject.SetActive(true);

        NarracionManager.Instance?.Narrar(
            "Escuché pasos. No estoy solo."
        );
    }
    }