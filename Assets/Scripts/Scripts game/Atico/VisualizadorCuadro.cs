using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GOTY — Nivel 3 (Ático)
/// Muestra la imagen de un cuadro en pantalla completa al examinarlo.
/// El jugador presiona E para cerrar y continuar.
/// </summary>
public class VisualizadorCuadro : MonoBehaviour
{
    public static VisualizadorCuadro Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject canvasVisualizacion;
    [SerializeField] private Image imagenCuadro;
    [SerializeField] private TextMeshProUGUI textoCuadro;
    [SerializeField] private TextMeshProUGUI textoCerrar;

    [Header("Animación")]
    [SerializeField] private float duracionFadeIn  = 0.4f;
    [SerializeField] private float duracionFadeOut = 0.3f;

    private CanvasGroup canvasGroup;
    private bool mostrando = false;
    private PlayerController player;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        canvasGroup = canvasVisualizacion.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = canvasVisualizacion.AddComponent<CanvasGroup>();

        canvasVisualizacion.SetActive(false);
        player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        // Cerrar al presionar E mientras se muestra
        if (mostrando && Input.GetKeyDown(KeyCode.E))
            StartCoroutine(Cerrar());
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Llamar desde CuadroRecuerdo al interactuar.
    /// </summary>
    public void Mostrar(Sprite imagen, string descripcion)
    {
        if (mostrando) return;
        StartCoroutine(MostrarCuadro(imagen, descripcion));
    }

    public bool EstaMostrando() => mostrando;

    // ─── Corrutinas ───────────────────────────────────────────────────────────

    private IEnumerator MostrarCuadro(Sprite imagen, string descripcion)
    {
        mostrando = true;
        // No bloqueamos al jugador — solo mostramos la imagen
        // El InteractionSystem se bloquea solo porque NarracionManager estará activo

        // Configurar contenido
        if (imagenCuadro != null)
        {
            imagenCuadro.sprite  = imagen;
            imagenCuadro.enabled = imagen != null;
        }

        if (textoCuadro != null)
            textoCuadro.text = descripcion;

        if (textoCerrar != null)
            textoCerrar.text = "Presiona E para cerrar";

        // Mostrar con fade in
        canvasVisualizacion.SetActive(true);
        canvasGroup.alpha = 0f;

        float t = 0f;
        while (t < duracionFadeIn)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / duracionFadeIn);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator Cerrar()
    {
        float t = 0f;
        while (t < duracionFadeOut)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / duracionFadeOut);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasVisualizacion.SetActive(false);
        mostrando = false;
        // El jugador se libera desde NarracionManager cuando termina la narración
    }
}