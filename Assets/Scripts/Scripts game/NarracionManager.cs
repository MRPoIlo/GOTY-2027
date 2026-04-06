using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// GOTY - Gestor de narración interna de Lena.
/// Muestra texto con efecto typewriter y puede encadenar líneas en secuencia.
/// Otros sistemas lo invocan vía NarracionManager.Instance.Narrar(...)
/// </summary>
public class NarracionManager : MonoBehaviour
{
    public static NarracionManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panelNarracion;
    [Tooltip("El ícono de interacción (se oculta mientras narra)")]
    [SerializeField] private GameObject iconoInteraccion;
    [SerializeField] private TextMeshProUGUI textoNarracion;
    [SerializeField] private TextMeshProUGUI indicadorContinuar; // "— continúa —"

    [Header("Tiempos")]
    [SerializeField] private float velocidadTypewriter = 0.04f; // seg por caracter
    [SerializeField] private float tiempoAutoAvance    = 0f;    // 0 = espera input del jugador

    // Estado
    private Queue<string> colaTextos = new Queue<string>();
    private Coroutine corutinaNarracion;
    private bool narrando = false;
    private bool esperandoInput = false;

    // Referencia al PlayerController para bloquearlo durante narración
    private PlayerController playerController;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        playerController = FindObjectOfType<PlayerController>();
        OcultarPanel();
    }

    void Update()
    {
        // El jugador presiona E o Space para avanzar al siguiente texto
        if (esperandoInput && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
        {
            esperandoInput = false;
        }
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Muestra una sola línea de narración.
    /// </summary>
    public void Narrar(string texto)
    {
        Narrar(new string[] { texto });
    }

    /// <summary>
    /// Muestra una secuencia de líneas de narración en orden.
    /// Si ya hay narración activa, espera a que termine antes de iniciar.
    /// El jugador presiona E / Space para avanzar entre ellas.
    /// </summary>
    public void Narrar(string[] textos)
    {
        // Si ya está narrando, encolar para después en vez de interrumpir
        if (narrando)
        {
            StartCoroutine(EsperarYNarrar(textos));
            return;
        }

        colaTextos.Clear();
        foreach (var t in textos)
            colaTextos.Enqueue(t);

        if (corutinaNarracion != null)
            StopCoroutine(corutinaNarracion);

        corutinaNarracion = StartCoroutine(MostrarSecuencia());
    }

    /// <summary>Espera a que termine la narración actual y luego muestra la siguiente.</summary>
    private IEnumerator EsperarYNarrar(string[] textos)
    {
        yield return new WaitUntil(() => !narrando);
        Narrar(textos);
    }

    /// <summary>Detiene la narración inmediatamente.</summary>
    public void Detener()
    {
        if (corutinaNarracion != null)
            StopCoroutine(corutinaNarracion);

        colaTextos.Clear();
        narrando = false;
        esperandoInput = false;
        OcultarPanel();
        playerController?.SetBloqueado(false);
    }

    public bool EstaActivo() => narrando;

    // ─── Corrutinas ───────────────────────────────────────────────────────────

    private IEnumerator MostrarSecuencia()
    {
        narrando = true;
        playerController?.SetBloqueado(true);
        panelNarracion?.SetActive(true);
        iconoInteraccion?.SetActive(false);

        while (colaTextos.Count > 0)
        {
            string linea = colaTextos.Dequeue();
            yield return StartCoroutine(TypewriterEfecto(linea));

            // Mostrar indicador de continuar
            bool esUltima = colaTextos.Count == 0;
            if (indicadorContinuar != null)
                indicadorContinuar.gameObject.SetActive(!esUltima);

            // Esperar input o auto-avanzar
            if (tiempoAutoAvance > 0f)
                yield return new WaitForSeconds(tiempoAutoAvance);
            else
            {
                esperandoInput = true;
                yield return new WaitUntil(() => !esperandoInput);
            }
        }

        narrando = false;
        OcultarPanel();
        iconoInteraccion?.SetActive(true);
        playerController?.SetBloqueado(false);
    }

    private IEnumerator TypewriterEfecto(string linea)
    {
        textoNarracion.text = "";
        if (indicadorContinuar != null)
            indicadorContinuar.gameObject.SetActive(false);

        foreach (char c in linea)
        {
            // Si el jugador presiona E mientras escribe, completa el texto de golpe
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                textoNarracion.text = linea;
                yield break;
            }
            textoNarracion.text += c;
            yield return new WaitForSeconds(velocidadTypewriter);
        }
    }

    private void OcultarPanel()
    {
        panelNarracion?.SetActive(false);
        if (textoNarracion != null) textoNarracion.text = "";
        if (indicadorContinuar != null) indicadorContinuar.gameObject.SetActive(false);
    }
}