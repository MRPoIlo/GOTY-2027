using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SalaCocinaManager : MonoBehaviour
{
    public static SalaCocinaManager Instance { get; private set; }

    [Header("Fade")]
    [SerializeField] private CanvasGroup pantallaFade;
    [SerializeField] private float duracionFade = 1.5f;

    [Header("Cajas")]
    [SerializeField] private List<GameObject> cajasBloqueando;

    [Header("Spawn")]
    [SerializeField] private Transform spawnJugador;

    [Header("Mensajes")]
    [TextArea]
    [SerializeField] private List<string> mensajesCajas;

    [Header("Escena")]
    [SerializeField] private string escenaSiguiente = "Nivel6Sotano";

    // 🔹 NUEVO: Evento para sincronizar inicialización del enemigo
    [Header("Eventos")]
    public UnityEvent OnJugadorListo;

    private PlayerController player;
    private bool nivelTerminado = false;

    void Awake()
    {
        Instance = this;
        player = FindObjectOfType<PlayerController>();
    }

    IEnumerator Start()
    {
        // 🔹 Bloquear jugador durante intro
        player?.SetBloqueado(true);

        // 🔹 Fade inicial
        if (pantallaFade != null) pantallaFade.alpha = 1f;
        yield return StartCoroutine(Fade(0f, duracionFade));

        // 🔹 Narración inicial más profunda e inmersiva
        NarracionManager.Instance?.Narrar(new string[]
        {
            "El frío de la cocina me atraviesa...",
            "Las sombras se mueven en silencio.",
            "La única salida es el sótano, pero está bloqueada.",
            "Debo mover esas cajas... sin que me escuche."
        });

        // 🔹 Esperar que termine la narración
        yield return new WaitForSeconds(8f);

        // 🔹 Desbloquear jugador
        player?.SetBloqueado(false);

        // 🔹 CRÍTICO: Notificar que el jugador está listo
        Debug.Log("✅ Jugador desbloqueado - Notificando a enemigo");
        OnJugadorListo?.Invoke();
    }

    // ───── CAJAS ─────
    public void MoverCaja(GameObject caja)
    {
        if (!cajasBloqueando.Contains(caja)) return;

        int index = cajasBloqueando.IndexOf(caja);
        cajasBloqueando.Remove(caja);
        caja.SetActive(false);

        // 🔹 Mensaje educativo más inmersivo
        if (index < mensajesCajas.Count)
        {
            NarracionManager.Instance?.Narrar(new string[]
            {
                mensajesCajas[index]
            });
        }

        // 🔹 Ruido → enemigo investiga
        EnemyNivel5 enemy = FindObjectOfType<EnemyNivel5>();
        if (enemy != null)
        {
            enemy.Investigar(caja.transform.position);
        }

        // 🔹 Última caja - narración más profunda
        if (cajasBloqueando.Count == 0)
        {
            NarracionManager.Instance?.Narrar(new string[]
            {
                "El camino está libre...",
                "Pero él sigue ahí afuera.",
                "Debo moverme con cuidado."
            });
        }
    }

    // ───── GAME OVER ─────
    public void GameOver()
    {
        if (nivelTerminado) return;
        StartCoroutine(ReiniciarNivel());
    }

    IEnumerator ReiniciarNivel()
    {
        player?.SetBloqueado(true);

        yield return StartCoroutine(Fade(1f, 1f));

        // 🔹 Narración de muerte más impactante
        NarracionManager.Instance?.Narrar(new string[]
        {
            "Sus pasos resonaron detrás de mí...",
            "No fui lo suficientemente sigiloso.",
            "El miedo me paraliza.",
            "Debo intentarlo de nuevo."
        });

        yield return new WaitForSeconds(6f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ───── SALIDA ─────
    public void OnJugadorEscapo()
    {
        if (nivelTerminado) return;
        nivelTerminado = true;
        StartCoroutine(TerminarNivel());
    }

    IEnumerator TerminarNivel()
    {
        player?.SetBloqueado(true);

        // 🔹 Narración de victoria más profunda
        NarracionManager.Instance?.Narrar(new string[]
        {
            "Lo logré... el camino al sótano está abierto.",
            "Puedo sentir su respiración a lo lejos.",
            "No hay tiempo que perder.",
            "Debo descender antes de que me encuentre."
        });

        yield return new WaitForSeconds(7f);

        yield return StartCoroutine(Fade(1f, duracionFade));

        SceneManager.LoadScene(escenaSiguiente);
    }

    // ───── FADE ─────
    IEnumerator Fade(float objetivo, float duracion)
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