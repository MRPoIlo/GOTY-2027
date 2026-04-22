using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CuadroRecuerdo : MonoBehaviour, IInteractuable
{
    [Header("Identificación")]
    public int ordenCronologico = 1;
    public string descripcionCorta = "Recuerdo 1";

    [Header("Imagen del cuadro")]
    public Sprite imagenCuadro;

    [Header("Narración al examinar")]
    [SerializeField, TextArea(2, 5)]
    private string[] lineasNarracion;

    [Header("Visual")]
    public Material materialRoto;
    public Material materialCompleto;

    [Header("Evento")]
    public UnityEvent OnExaminado;

    private bool examinado = false;
    private bool resuelto  = false;
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (materialRoto != null)
            rend.material = materialRoto;
    }

    public void Interactuar()
    {
        if (resuelto || examinado) return;
        examinado = true;
        StartCoroutine(SecuenciaInteraccion());
    }

    private IEnumerator SecuenciaInteraccion()
    {
        // 1. Mostrar imagen
        VisualizadorCuadro.Instance?.Mostrar(imagenCuadro, descripcionCorta);

        // 2. Esperar que el jugador cierre la imagen
        yield return new WaitUntil(() =>
            VisualizadorCuadro.Instance == null ||
            !VisualizadorCuadro.Instance.EstaMostrando());

        // 3. Mostrar narración del cuadro
        if (lineasNarracion != null && lineasNarracion.Length > 0)
        {
            NarracionManager.Instance?.Narrar(lineasNarracion);

            // 4. Esperar que termine la narración
            yield return new WaitUntil(() =>
                NarracionManager.Instance == null ||
                !NarracionManager.Instance.EstaActivo());
        }

        // 5. SOLO DESPUÉS de todo lo anterior, registrar en el manager
        // Así el menú del puzzle nunca interrumpe la narración del cuadro
        AticoManager.Instance?.RegistrarCuadroExaminado(this);

        OnExaminado?.Invoke();
    }

    public string ObtenerTextoAccion() => resuelto ? "Observar" : "Examinar";
    public bool EstaActivo() => !examinado && !resuelto;

    public bool FueExaminado()     => examinado;
    public bool EstaResuelto()     => resuelto;
    public string GetDescripcion() => descripcionCorta;

    public void MarcarResuelto()
    {
        resuelto = true;
        if (materialCompleto != null && rend != null)
            rend.material = materialCompleto;
    }
}