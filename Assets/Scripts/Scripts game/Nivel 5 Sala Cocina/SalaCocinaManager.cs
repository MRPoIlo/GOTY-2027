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

    [Header("Mensajes")]
    [TextArea][SerializeField] private List<string> mensajesCajas;

    [Header("Escena")]
    [SerializeField] private string escenaSiguiente = "Nivel6Sotano";

    [Header("Eventos")]
    public UnityEvent OnJugadorListo;

    private PlayerController player;
    private bool nivelTerminado = false;

    // 🔹 Estados de progreso
    private enum FaseJuego { TV, PuertaCocina, Cajas, PuertaSotano }
    private FaseJuego faseActual = FaseJuego.TV;

    void Awake()
    {
        Instance = this;
        player = FindObjectOfType<PlayerController>();
    }

    IEnumerator Start()
    {
        // Bloquear jugador durante intro
        player?.SetBloqueado(true);

        // Fade inicial
        if (pantallaFade != null) pantallaFade.alpha = 1f;
        yield return StartCoroutine(Fade(0f, duracionFade));

        // Narración inicial
        NarracionManager.Instance?.Narrar(new string[]
        {
            "El frío de la cocina me atraviesa...",
            "Las sombras se mueven en silencio.",
            "Debo encontrar una salida."
        });

        yield return new WaitForSeconds(6f);

        // Desbloquear jugador
        player?.SetBloqueado(false);

        // Notificar que el jugador está listo
        OnJugadorListo?.Invoke();
    }

    // ───── INTERACCIONES ─────
    public void InteractuarTV()
    {
        if (faseActual != FaseJuego.TV) return;

        NarracionManager.Instance?.Narrar(new string[]
        {
            "El televisor parpadea con un mensaje extraño...",
            "Debo revisar la puerta de la cocina."
        });

        faseActual = FaseJuego.PuertaCocina;
    }

    public void InteractuarPuertaCocina()
    {
        if (faseActual != FaseJuego.PuertaCocina) return;

        NarracionManager.Instance?.Narrar(new string[]
        {
            "La puerta de la cocina está cerrada.",
            "La única salida es el sótano, pero está bloqueado por cajas.",
            "Debo moverlas para poder abrir la puerta."
        });

        faseActual = FaseJuego.Cajas;
    }

    public void MoverCaja(GameObject caja)
    {
        if (faseActual != FaseJuego.Cajas)
        {
            NarracionManager.Instance?.Narrar(new string[]
            {
                "Aún no puedo mover las cajas...",
                "Debo revisar primero la puerta de la cocina."
            });
            return;
        }

        if (!cajasBloqueando.Contains(caja)) return;

        int index = cajasBloqueando.IndexOf(caja);
        cajasBloqueando.Remove(caja);
        caja.SetActive(false);

        if (index < mensajesCajas.Count)
        {
            NarracionManager.Instance?.Narrar(new string[]
            {
                mensajesCajas[index]
            });
        }

        // Ruido → enemigo investiga
        EnemyNivel5 enemy = FindObjectOfType<EnemyNivel5>();
        if (enemy != null)
        {
            enemy.Investigar(caja.transform.position);
        }

        if (cajasBloqueando.Count == 0)
        {
            NarracionManager.Instance?.Narrar(new string[]
            {
                "El camino al sótano está libre...",
                "Debo abrir la puerta y escapar."
            });
            faseActual = FaseJuego.PuertaSotano;
        }
    }

    public bool PuedeEscapar()
    {
        return faseActual == FaseJuego.PuertaSotano;
    }

    public void OnJugadorEscapo()
    {
        if (!PuedeEscapar() || nivelTerminado)
        {
            NarracionManager.Instance?.Narrar(new string[]
            {
                "La puerta aún está bloqueada...",
                "Debo mover todas las cajas primero."
            });
            return;
        }

        nivelTerminado = true;
        StartCoroutine(TerminarNivel());
    }

    IEnumerator TerminarNivel()
    {
        player?.SetBloqueado(true);

        NarracionManager.Instance?.Narrar(new string[]
        {
            "Lo logré... el camino al sótano está abierto.",
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

    // ───── Helpers para fases ─────
    public bool EstaEnFaseTV() => faseActual == FaseJuego.TV;
    public bool EstaEnFasePuertaCocina() => faseActual == FaseJuego.PuertaCocina;
    public bool EstaEnFaseCajas() => faseActual == FaseJuego.Cajas;
    public bool EstaEnFasePuertaSotano() => faseActual == FaseJuego.PuertaSotano;
}
