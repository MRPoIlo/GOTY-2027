using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Logros 100 % locales (sin servidor).
/// - Se crea solo al arrancar el juego: NO hace falta ponerlo en ninguna escena.
/// - Guarda los logros en PlayerPrefs y nunca repite uno ya obtenido.
/// - Muestra una notificación breve con estilo de terror al desbloquear.
/// - Modo exposición: muestra el resumen al final y reinicia todo el progreso.
/// - Atajo oculto en MainMenu: Ctrl + Shift + R reinicia todo.
/// </summary>
public class LogrosManager : MonoBehaviour
{
    public static LogrosManager Instance { get; private set; }

    // ─────────────── Claves de PlayerPrefs del juego ───────────────
    private const string KeyLista = "Logros_Lista";          // ids separados por '|'
    private const string KeyPrefijoDesc = "Logro_Desc_";      // descripción de cada logro
    public const string KeyLinterna = "LinternaDesbloqueada"; // usada por Flashlight
    public const string KeyEscenaGuardada = "EscenaGuardada"; // usada por SaveManager

    [Header("Notificación")]
    [SerializeField] private float duracionNotificacion = 3f;
    [SerializeField] private TMP_FontAsset fuente; // opcional; si está vacío se usa la fuente de la narración

    [Header("Modo exposición")]
    [SerializeField] private string escenaMenu = "MainMenu";

    private readonly List<string> ids = new List<string>();
    private readonly Queue<string> pendientes = new Queue<string>();
    private bool mostrando;

    // UI creada por código
    private Canvas canvas;
    private CanvasGroup grupoNotificacion;
    private TextMeshProUGUI textoTitulo;
    private TextMeshProUGUI textoLogro;
    private CanvasGroup grupoResumen;
    private TextMeshProUGUI textoResumen;

    private static readonly Color RojoSangre = new Color(0.78f, 0.05f, 0.04f, 1f);
    private static readonly Color Hueso = new Color(0.86f, 0.82f, 0.74f, 1f);

    // ───────────────────────── Arranque automático ─────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CrearAutomaticamente()
    {
        if (Instance != null || FindAnyObjectByType<LogrosManager>() != null) return;
        new GameObject("LogrosManager").AddComponent<LogrosManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CargarLogros();
        CrearUI();
    }

    private void Update()
    {
        // Atajo oculto: Ctrl + Shift + R en el menú principal
        if (SceneManager.GetActiveScene().name != escenaMenu) return;

        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (ctrl && shift && Input.GetKeyDown(KeyCode.R))
        {
            ReiniciarTodo();
            Notificar("PROGRESO BORRADO", "Todo vuelve a empezar...");
        }
    }

    // ───────────────────────────── API pública ─────────────────────────────

    /// <summary>Desbloquea un logro. Si ya se tenía, no hace nada y devuelve false.</summary>
    public bool Desbloquear(string id, string descripcion)
    {
        if (string.IsNullOrEmpty(id) || ids.Contains(id)) return false;

        ids.Add(id);
        PlayerPrefs.SetString(KeyLista, string.Join("|", ids));
        PlayerPrefs.SetString(KeyPrefijoDesc + id, descripcion ?? id);
        PlayerPrefs.Save();

        Debug.Log($"[Logros] Desbloqueado: {id} ({descripcion})");
        Notificar("LOGRO DESBLOQUEADO", descripcion);
        return true;
    }

    public bool Tiene(string id) => ids.Contains(id);

    /// <summary>Descripciones de los logros obtenidos, en el orden en que se consiguieron.</summary>
    public List<string> ObtenerLogros()
    {
        var lista = new List<string>();
        foreach (var id in ids)
            lista.Add(PlayerPrefs.GetString(KeyPrefijoDesc + id, id));
        return lista;
    }

    private List<string> resumenFinal; // copia de los logros tomada al llegar al final

    /// <summary>
    /// Modo exposición, paso 1 (al ENTRAR a un final): desbloquea el logro del final,
    /// guarda una copia de la lista y borra todo el progreso al instante. Así, aunque
    /// alguien salga en mitad de la cinemática, el siguiente jugador empieza desde cero.
    /// </summary>
    public void IniciarFinal(string idFinal, string descripcionFinal)
    {
        Desbloquear(idFinal, descripcionFinal);
        resumenFinal = ObtenerLogros();
        ReiniciarTodo();
    }

    /// <summary>
    /// Modo exposición, paso 2 (al TERMINAR la cinemática): muestra los logros conseguidos.
    /// </summary>
    public void MostrarResumenFinal()
    {
        var obtenidos = resumenFinal ?? ObtenerLogros();
        resumenFinal = null;

        string cuerpo = obtenidos.Count == 0
            ? "<i>Ningún recuerdo quedó contigo...</i>"
            : "• " + string.Join("\n• ", obtenidos);

        textoResumen.text =
            "<size=150%><color=#C80D0A>LO QUE LOGRASTE RECORDAR</color></size>\n\n" +
            cuerpo +
            $"\n\n<size=80%><color=#7A736A>{obtenidos.Count} logro(s)</color></size>";

        StartCoroutine(FadeResumen(1f, 1.5f));
    }

    public void OcultarResumen()
    {
        if (grupoResumen != null) grupoResumen.alpha = 0f;
    }

    /// <summary>Borra logros, partida guardada, linterna y el progreso en memoria.</summary>
    public void ReiniciarTodo()
    {
        foreach (var id in ids)
            PlayerPrefs.DeleteKey(KeyPrefijoDesc + id);
        ids.Clear();

        PlayerPrefs.DeleteKey(KeyLista);
        PlayerPrefs.DeleteKey(KeyLinterna);
        PlayerPrefs.DeleteKey(KeyEscenaGuardada);
        PlayerPrefs.Save();

        // Progreso en memoria (GameManager2 sobrevive entre escenas)
        if (GameManager2.Instance != null)
        {
            GameManager2.Instance.tieneLinterna = false;
            GameManager2.Instance.prologoCompletado = false;
            GameManager2.Instance.tieneDestornillador = false;
            GameManager2.Instance.objetosBuenosRecogidos = 0;
        }

        Debug.Log("[Logros] Progreso reiniciado: logros, partida, linterna y PlayerPrefs del juego.");
    }

    // ───────────────────────────── Interno ─────────────────────────────

    private void CargarLogros()
    {
        ids.Clear();
        string guardado = PlayerPrefs.GetString(KeyLista, "");
        if (string.IsNullOrEmpty(guardado)) return;

        foreach (var id in guardado.Split('|'))
            if (!string.IsNullOrEmpty(id) && !ids.Contains(id))
                ids.Add(id);
    }

    private void Notificar(string titulo, string texto)
    {
        pendientes.Enqueue(titulo + "\n" + texto);
        if (!mostrando) StartCoroutine(MostrarPendientes());
    }

    private IEnumerator MostrarPendientes()
    {
        mostrando = true;
        AplicarFuente();

        while (pendientes.Count > 0)
        {
            string[] partes = pendientes.Dequeue().Split(new[] { '\n' }, 2);
            textoTitulo.text = partes[0];
            textoLogro.text = partes.Length > 1 ? partes[1] : "";

            // Entrada con parpadeo, como una luz que falla
            float[] parpadeo = { 0.6f, 0f, 0.9f, 0.2f, 1f };
            foreach (float a in parpadeo)
            {
                grupoNotificacion.alpha = a;
                yield return new WaitForSecondsRealtime(0.06f);
            }

            // Permanece con un temblor leve
            float t = 0f;
            float visible = Mathf.Max(0.5f, duracionNotificacion - 1f);
            Vector2 basePos = textoLogro.rectTransform.anchoredPosition;
            while (t < visible)
            {
                t += Time.unscaledDeltaTime;
                textoLogro.rectTransform.anchoredPosition = basePos + Random.insideUnitCircle * 1.2f;
                yield return null;
            }
            textoLogro.rectTransform.anchoredPosition = basePos;

            // Se desvanece
            t = 0f;
            while (t < 0.7f)
            {
                t += Time.unscaledDeltaTime;
                grupoNotificacion.alpha = Mathf.Lerp(1f, 0f, t / 0.7f);
                yield return null;
            }
            grupoNotificacion.alpha = 0f;
        }

        mostrando = false;
    }

    private IEnumerator FadeResumen(float objetivo, float duracion)
    {
        AplicarFuente();
        float inicio = grupoResumen.alpha;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            grupoResumen.alpha = Mathf.Lerp(inicio, objetivo, t / duracion);
            yield return null;
        }
        grupoResumen.alpha = objetivo;
    }

    /// <summary>Usa la fuente asignada o, si no hay, la de la narración del juego.</summary>
    private void AplicarFuente()
    {
        TMP_FontAsset f = fuente;
        if (f == null && NarracionManager.Instance != null)
        {
            var textoNarracion = NarracionManager.Instance.GetComponentInChildren<TextMeshProUGUI>(true);
            if (textoNarracion != null) f = textoNarracion.font;
        }
        if (f == null) return;

        fuente = f; // se recuerda para escenas sin narración (finales, menú)
        textoTitulo.font = f;
        textoLogro.font = f;
        textoResumen.font = f;
    }

    private void CrearUI()
    {
        var goCanvas = new GameObject("LogrosCanvas");
        goCanvas.transform.SetParent(transform, false);
        canvas = goCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000; // por encima de toda la UI del juego
        var scaler = goCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // ── Notificación (arriba al centro) ──
        var notif = CrearPanel(goCanvas.transform, "Notificacion",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(760f, 150f),
            new Color(0.01f, 0f, 0f, 0.96f));
        grupoNotificacion = notif.AddComponent<CanvasGroup>();
        grupoNotificacion.alpha = 0f;
        grupoNotificacion.blocksRaycasts = false;
        grupoNotificacion.interactable = false;

        // Línea roja fina en la base, como una herida
        CrearPanel(notif.transform, "Linea", new Vector2(0f, 0f), new Vector2(1f, 0f),
            Vector2.zero, new Vector2(0f, 3f), RojoSangre);

        textoTitulo = CrearTexto(notif.transform, "Titulo", 26, RojoSangre,
            new Vector2(0f, 0.55f), new Vector2(1f, 1f));
        textoTitulo.characterSpacing = 12f;
        textoTitulo.fontStyle = FontStyles.Bold;

        textoLogro = CrearTexto(notif.transform, "Logro", 40, Hueso,
            new Vector2(0f, 0f), new Vector2(1f, 0.6f));

        // ── Resumen final (pantalla completa, sin bloquear el botón de salir) ──
        var resumen = CrearPanel(goCanvas.transform, "ResumenFinal",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(1100f, 700f),
            new Color(0.01f, 0.005f, 0.005f, 1f));
        grupoResumen = resumen.AddComponent<CanvasGroup>();

        CrearPanel(resumen.transform, "Linea", new Vector2(0f, 0f), new Vector2(1f, 0f),
            Vector2.zero, new Vector2(0f, 3f), RojoSangre);
        grupoResumen.alpha = 0f;
        grupoResumen.blocksRaycasts = false;
        grupoResumen.interactable = false;

        textoResumen = CrearTexto(resumen.transform, "Texto", 38, Hueso,
            new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.94f));
        textoResumen.lineSpacing = 12f;

        // Al cambiar de escena se oculta el resumen y se toma la fuente de la narración (si hay)
        SceneManager.sceneLoaded += (s, m) => { OcultarResumen(); AplicarFuente(); };
        AplicarFuente();
    }

    private static GameObject CrearPanel(Transform padre, string nombre, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pos, Vector2 tam, Color color)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(padre, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, anchorMin.y);
        rt.anchoredPosition = pos;
        rt.sizeDelta = tam;
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return go;
    }

    private static TextMeshProUGUI CrearTexto(Transform padre, string nombre, float tam, Color color,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = tam;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return tmp;
    }
}
