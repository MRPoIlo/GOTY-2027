using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Text;

public class NivelManager1 : MonoBehaviour
{
    private static readonly string[] NarracionInicio = {
        "La habitación de mis padres.",
        "Aquí todo parece detenido en el tiempo, como si el aire aún guardara discusiones que nunca se apagaron."
    };

    private static readonly string[] NarracionSombraMadre = {
        "Una silueta...",
        "No es un fantasma, es la memoria que me persigue."
    };

    private static readonly string[] NarracionPasos = {
        "Escucho pasos...",
        "Escóndete."
    };

    private static readonly string[] NarracionEscape = {
        "¡Me escucharon... corre!"
    };

    private static readonly string[] NarracionPuertaBloqueada = {
        "Todavía hay cosas que debo enfrentar.",
        "No puedo salir sin mirar atrás."
    };

    [Header("Referencia a UI Fade")]
    [SerializeField] private CanvasGroup pantallaFade;
    [SerializeField] private float duracionFade = 2.5f;

    [Header("Siguiente escena")]
    [SerializeField] private string escenaSiguiente = "Nivel2Baño";

    [Header("Condición de avance")]
    [SerializeField] private int objetosRequeridos = 4;

    private int interaccionesCompletadas = 0;
    private bool sombraActivada = false;
    private bool pasosActivados = false;
    private bool enemigoActivado = false;
    private bool finalizando = false;

    [Header("Referencia al enemigo")]
    [SerializeField] private EnemyAI enemigo;

    [Header("Referencia a la madre")]
    [SerializeField] private SombraMadreEvento sombraMadre;

    [Header("Puerta narrativa")]
    [SerializeField] private GameObject puertaEntrada;

    [Header("Paneles externos")]
    [SerializeField] private GameObject panelMenuPausa;
    [SerializeField] private GameObject panelOpcionesPausa;
    [SerializeField] private Canvas canvasNarracion;
    [SerializeField] private GameObject jumpscareObject;

    private PlayerController player;

    public bool enGameOver = false;

    // API
    private string url = "http://goty.somee.com/api/Logroes";

    void Awake()
    {
        player = FindFirstObjectByType<PlayerController>();

        if (enemigo == null)
            enemigo = FindFirstObjectByType<EnemyAI>();

        player?.SetBloqueado(true);

        if (pantallaFade != null)
            pantallaFade.alpha = 1f;
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(Fade(0f, duracionFade));

        yield return new WaitForSeconds(0.5f);

        NarracionManager.Instance?.Narrar(NarracionInicio);

        yield return new WaitUntil(() =>
            NarracionManager.Instance == null ||
            !NarracionManager.Instance.EstaActivo());

        player?.SetBloqueado(false);
    }

    public void RegistrarInteraccion()
    {
        interaccionesCompletadas++;

        Debug.Log("Interacciones: " +
            interaccionesCompletadas);

        if (interaccionesCompletadas >= objetosRequeridos &&
            !sombraActivada)
        {
            ActivarSombraMadre();
        }
    }

    private void ActivarSombraMadre()
    {
        if (sombraActivada)
            return;

        sombraActivada = true;

        player?.SetBloqueado(true);

        StartCoroutine(SecuenciaSombraMadre());
    }

    private IEnumerator SecuenciaSombraMadre()
    {
        if (sombraMadre != null)
            sombraMadre.ActivarSombra();

        yield return new WaitForSeconds(4f);

        NarracionManager.Instance?.
            Narrar(NarracionSombraMadre);

        yield return new WaitForSeconds(5f);

        if (sombraMadre != null)
            sombraMadre.DesactivarSombra();

        if (puertaEntrada != null)
        {
            puertaEntrada.SetActive(false);

            Collider col =
                puertaEntrada.GetComponent<Collider>();

            if (col != null)
                col.enabled = false;
        }

        enemigo?.SetBloqueado(false);

        enemigoActivado = true;

        player?.SetBloqueado(false);

        if (!pasosActivados)
        {
            pasosActivados = true;

            NarracionManager.Instance?.
                Narrar(NarracionPasos);
        }
    }

    public void IntentarSalir()
    {
        if (finalizando)
            return;

        if (enemigoActivado)
        {
            StartCoroutine(TerminarNivel1());
        }
        else
        {
            NarracionManager.Instance?.
                Narrar(NarracionPuertaBloqueada);
        }
    }

    private IEnumerator TerminarNivel1()
    {
        finalizando = true;

        player?.SetBloqueado(true);

        NarracionManager.Instance?.
            Narrar(NarracionEscape);

        yield return new WaitUntil(() =>
            NarracionManager.Instance == null ||
            !NarracionManager.Instance.EstaActivo());

        // 🔥 ENVIAR LOGRO
        yield return StartCoroutine(RegistrarLogro());

        // Esperar un poco para asegurar envío
        yield return new WaitForSeconds(1f);

        // Fade
        yield return StartCoroutine(Fade(1f, duracionFade));

        // Cargar escena
        SceneManager.LoadScene(escenaSiguiente);
    }

    private IEnumerator RegistrarLogro()
    {
        LogroData data = new LogroData();

        data.NombreJugador = "Lena";
        data.Descripcion = "Completó Nivel 1";
        data.idTema = 2;

        string json = JsonUtility.ToJson(data);

        Debug.Log("========== LOGRO NIVEL 1 ==========");
        Debug.Log(json);

        using (UnityWebRequest www =
            new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw =
                Encoding.UTF8.GetBytes(json);

            www.uploadHandler =
                new UploadHandlerRaw(bodyRaw);

            www.downloadHandler =
                new DownloadHandlerBuffer();

            www.SetRequestHeader(
                "Content-Type",
                "application/json");

            yield return www.SendWebRequest();

            if (www.result ==
                UnityWebRequest.Result.Success)
            {
                Debug.Log("🏆 NIVEL 1 REGISTRADO");
                Debug.Log(www.downloadHandler.text);
            }
            else
            {
                Debug.LogError("❌ ERROR API");

                Debug.LogError(www.error);

                Debug.LogError(www.downloadHandler.text);
            }
        }
    }

    // GAME OVER

    public void ActivarGameOver()
    {
        enGameOver = true;

        if (panelMenuPausa != null)
            panelMenuPausa.SetActive(false);

        if (panelOpcionesPausa != null)
            panelOpcionesPausa.SetActive(false);

        if (canvasNarracion != null)
            canvasNarracion.enabled = false;

        player?.SetBloqueado(true);

        Cursor.lockState = CursorLockMode.None;

        Cursor.visible = true;

        Time.timeScale = 0f;

        if (jumpscareObject != null)
            jumpscareObject.SetActive(true);
    }

    public void ReintentarNivel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
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

            pantallaFade.alpha =
                Mathf.Lerp(inicio, objetivo, t / duracion);

            yield return null;
        }

        pantallaFade.alpha = objetivo;
    }
}