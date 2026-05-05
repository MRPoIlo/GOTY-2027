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

    [Header("Cajas")]
    [SerializeField] private List<GameObject> cajasBloqueando;

    [Header("Mensajes")]
    [TextArea][SerializeField] private List<string> mensajesCajas;

    [Header("Escena")]
    [SerializeField] private string escenaSiguiente = "Nivel6Sotano";

    [Header("Televisión")]
    [SerializeField] private Sprite partidoSprite;
    [SerializeField] private Sprite pantallaRotaSprite;
    [SerializeField] private SpriteRenderer tvRenderer;
    [SerializeField] private AudioSource tvAudioSource;
    [SerializeField] private AudioClip sonidoPantallaRota;

    private PlayerController player;
    private EnemyNivel5 enemy;
    private bool nivelTerminado = false;

    public enum FaseJuego { TV, PuertaCocina, Cajas, PuertaSotano }
    private FaseJuego faseActual = FaseJuego.TV;

    void Awake()
    {
        Instance = this;
        player = FindFirstObjectByType<PlayerController>();
        enemy = FindFirstObjectByType<EnemyNivel5>();
    }

    IEnumerator Start()
    {
        BloquearJuego(true);

        if (pantallaFade != null) pantallaFade.alpha = 1f;
        yield return StartCoroutine(Fade(0f, duracionFade));

        NarracionManager.Instance?.Narrar(new string[]
        {
            "La televisión está encendida... transmite un partido de fútbol.",
            "La normalidad de esas imágenes contrasta con la tensión que siento.",
            "Debo acercarme, aunque temo lo que pueda mostrarme."
        });

        NarracionManager.Instance.OnNarracionTerminada.AddListener(() => BloquearJuego(false));
        CambiarFase(FaseJuego.TV);
    }

    public void InteractuarTV()
    {
        if (faseActual != FaseJuego.TV) return;

        BloquearJuego(true);

        if (tvRenderer != null && partidoSprite != null)
            tvRenderer.sprite = partidoSprite;

        NarracionManager.Instance?.Narrar(new string[]
        {
            "El televisor muestra imágenes confusas, el partido se distorsiona...",
            "No puedo quedarme aquí... debo revisar la puerta de la cocina."
        });

        StartCoroutine(CambiarPantallaTrasNarracion());
        CambiarFase(FaseJuego.PuertaCocina);
    }

    IEnumerator CambiarPantallaTrasNarracion()
    {
        yield return new WaitUntil(() => !NarracionManager.Instance.EsNarrando);

        if (tvRenderer != null && pantallaRotaSprite != null)
        {
            Color c = tvRenderer.color;
            for (float t = 0; t < 1f; t += Time.deltaTime * 4f)
            {
                c.a = 1f - t;
                tvRenderer.color = c;
                yield return null;
            }
            tvRenderer.sprite = pantallaRotaSprite;
            c.a = 1f;
            tvRenderer.color = c;
        }

        if (tvAudioSource != null && sonidoPantallaRota != null)
            tvAudioSource.PlayOneShot(sonidoPantallaRota);

        NarracionManager.Instance?.Narrar(new string[]
        {
            "La pantalla se rompe... ya no hay partido, solo un reflejo de mi propia fractura.",
            "Lo que parecía entretenimiento ahora es un recordatorio de lo que debo enfrentar."
        });
    }

    public void InteractuarPuertaCocina()
    {
        if (faseActual == FaseJuego.TV)
        {
            BloquearJuego(true);
            NarracionManager.Instance?.Narrar(new string[]
            {
                "No puedo abrir la puerta todavía...",
                "Primero debo enfrentar lo que la televisión intenta mostrarme."
            });
            NarracionManager.Instance.OnNarracionTerminada.AddListener(() => BloquearJuego(false));
        }
        else if (faseActual == FaseJuego.PuertaCocina)
        {
            BloquearJuego(true);

            NarracionManager.Instance?.Narrar(new string[]
            {
                "La puerta de la cocina está cerrada, como si quisiera protegerme de lo que hay detrás.",
                "Pero sé que no es protección... es un muro que me obliga a enfrentar lo que escondo.",
                "Si quiero seguir, debo quitar las cajas que bloquean el sótano."
            });

            NarracionManager.Instance.OnNarracionTerminada.AddListener(() =>
            {
                BloquearJuego(false);
                if (enemy != null) enemy.SetBloqueado(false);
                CambiarFase(FaseJuego.Cajas);
            });
        }
    }

    public void MoverCaja(GameObject caja)
    {
        if (faseActual != FaseJuego.Cajas)
        {
            BloquearJuego(true);
            NarracionManager.Instance?.Narrar(new string[]
            {
                "Aún no puedo mover las cajas...",
                "Primero debo enfrentar la puerta de la cocina."
            });
            NarracionManager.Instance.OnNarracionTerminada.AddListener(() => BloquearJuego(false));
            return;
        }

        if (!cajasBloqueando.Contains(caja)) return;

        int index = cajasBloqueando.IndexOf(caja);
        cajasBloqueando.Remove(caja);

        BloquearJuego(true);
        StartCoroutine(NarrarMensajeCaja(index));

        // 🔴 Si ya no quedan cajas, cambiar fase ANTES de narrar
        if (cajasBloqueando.Count == 0)
        {
            CambiarFase(FaseJuego.PuertaSotano);

            NarracionManager.Instance?.Narrar(new string[]
            {
                "El camino al sótano está libre.",
                "Descender no es escapar... es enfrentar la oscuridad y lo que me espera en ella.",
                "Es hora de bajar."
            });

            NarracionManager.Instance.OnNarracionTerminada.AddListener(() =>
            {
                BloquearJuego(false);
                if (enemy != null) enemy.SetBloqueado(false);
            });
        }
    }

    IEnumerator NarrarMensajeCaja(int index)
    {
        if (index < mensajesCajas.Count && !string.IsNullOrEmpty(mensajesCajas[index]))
        {
            string mensaje = mensajesCajas[index];
            mensajesCajas[index] = "";

            NarracionManager.Instance?.Narrar(new string[] { mensaje });
            yield return new WaitUntil(() => !NarracionManager.Instance.EsNarrando);
        }

        BloquearJuego(false);
        if (enemy != null) enemy.SetBloqueado(false);
    }

    public bool PuedeEscapar() => faseActual == FaseJuego.PuertaSotano;

    public void OnJugadorEscapo()
    {
        if (!PuedeEscapar() || nivelTerminado)
        {
            BloquearJuego(true);
            NarracionManager.Instance?.Narrar(new string[]
            {
                "La puerta aún está bloqueada..."
            });
            NarracionManager.Instance.OnNarracionTerminada.AddListener(() => BloquearJuego(false));
            return;
        }

        nivelTerminado = true;
        StartCoroutine(TerminarNivel());
    }

    IEnumerator TerminarNivel()
    {
        BloquearJuego(true);

        NarracionManager.Instance?.Narrar(new string[]
        {
            "Lo logré... el camino al sótano está abierto.",
            "Pero sé que al descender no encontraré paz inmediata.",
            "Cada escalón será un recordatorio de lo que cargo conmigo.",
            "Y aun así... debo bajar."
        });

        yield return new WaitUntil(() => !NarracionManager.Instance.EsNarrando);

        yield return StartCoroutine(Fade(1f, duracionFade));
        SceneManager.LoadScene(escenaSiguiente);
    }

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

    public FaseJuego GetFaseActual() => faseActual;

    private void CambiarFase(FaseJuego nuevaFase)
    {
        faseActual = nuevaFase;
        Debug.Log("[SalaCocinaManager] Fase cambiada a: " + faseActual);
    }

    private void BloquearJuego(bool bloqueado)
    {
        player?.SetBloqueado(bloqueado);
        enemy?.SetBloqueado(bloqueado);
    }
}
