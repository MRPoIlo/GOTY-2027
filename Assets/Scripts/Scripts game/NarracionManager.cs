using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NarracionManager : MonoBehaviour
{
    public static NarracionManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panelNarracion;
    [SerializeField] private GameObject iconoInteraccion;
    [SerializeField] private TextMeshProUGUI textoNarracion;
    [SerializeField] private TextMeshProUGUI indicadorContinuar;

    [Header("Tiempos")]
    [SerializeField] private float velocidadTypewriter = 0.04f;
    [SerializeField] private float tiempoAutoAvance = 0f;

    private Queue<string> colaTextos = new Queue<string>();
    private Coroutine corutinaNarracion;
    private bool narrando = false;
    private bool esperandoInput = false;

    private PlayerController playerController;
    private PausaManager pausaManager;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        playerController = FindFirstObjectByType<PlayerController>();
        pausaManager = FindObjectOfType<PausaManager>();
        OcultarPanel();
    }

    void Update()
    {
        if (esperandoInput && narrando && Time.timeScale > 0f)
        {
            if (pausaManager != null && pausaManager.EnOpciones)
                return;

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                esperandoInput = false;
            }
        }
    }

    public void Narrar(string texto) => Narrar(new string[] { texto });

    public void Narrar(string[] textos)
    {
        if (narrando)
        {
            StartCoroutine(EsperarYNarrar(textos));
            return;
        }

        colaTextos.Clear();
        foreach (var t in textos) colaTextos.Enqueue(t);

        if (corutinaNarracion != null) StopCoroutine(corutinaNarracion);
        corutinaNarracion = StartCoroutine(MostrarSecuencia());
    }

    private IEnumerator EsperarYNarrar(string[] textos)
    {
        yield return new WaitUntil(() => !narrando);
        Narrar(textos);
    }

    public void Detener()
    {
        if (corutinaNarracion != null) StopCoroutine(corutinaNarracion);

        colaTextos.Clear();
        narrando = false;
        esperandoInput = false;
        OcultarPanel();
        playerController?.SetBloqueado(false);
    }

    public bool EstaActivo() => narrando;

    public void OnPausaActivada()
    {
        if (narrando) panelNarracion?.SetActive(false);
    }

    public void OnPausaContinuada()
    {
        if (narrando) panelNarracion?.SetActive(true);
    }

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

            bool esUltima = colaTextos.Count == 0;
            if (indicadorContinuar != null)
                indicadorContinuar.gameObject.SetActive(!esUltima);

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
        if (indicadorContinuar != null) indicadorContinuar.gameObject.SetActive(false);

        foreach (char c in linea)
        {
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

    public void OcultarMensaje()
    {
        OcultarPanel();
        narrando = false;
        esperandoInput = false;
        playerController?.SetBloqueado(false);
    }
}
