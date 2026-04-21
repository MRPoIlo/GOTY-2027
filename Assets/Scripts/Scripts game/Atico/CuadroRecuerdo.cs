using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// GOTY — Nivel 3 (Ático)
/// Representa un cuadro familiar roto. Al examinarlo muestra un fragmento
/// de recuerdo de Lena y se registra en el AticoManager.
/// </summary>
public class CuadroRecuerdo : MonoBehaviour, IInteractuable
{
    [Header("Identificación")]
    [Tooltip("Número del recuerdo en la historia real (1=más antiguo, 4=más reciente)")]
    public int ordenCronologico = 1;

    [Tooltip("Descripción corta para el menú de ordenamiento")]
    public string descripcionCorta = "Recuerdo 1";

    [Header("Narración al examinar")]
    [SerializeField, TextArea(2, 5)]
    private string[] lineasNarracion;

    [Header("Visual")]
    [Tooltip("Material del cuadro roto (estado inicial)")]
    public Material materialRoto;
    [Tooltip("Material del cuadro completo (estado resuelto)")]
    public Material materialCompleto;

    [Header("Evento")]
    public UnityEvent OnExaminado;

    // Estado
    private bool examinado = false;
    private bool resuelto = false;
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (materialRoto != null)
            rend.material = materialRoto;
    }

    // ─── IInteractuable ───────────────────────────────────────────────────────

    public void Interactuar()
    {
        if (resuelto) return;

        examinado = true;

        NarracionManager.Instance?.Narrar(lineasNarracion);

        // Registrar en el manager para habilitar el menú de ordenamiento
        AticoManager.Instance?.RegistrarCuadroExaminado(this);

        OnExaminado?.Invoke();
    }

    public string ObtenerTextoAccion() => resuelto ? "Observar" : "Examinar";
    public bool EstaActivo() => true;

    // ─── API pública ──────────────────────────────────────────────────────────

    public bool FueExaminado() => examinado;
    public bool EstaResuelto() => resuelto;
    public string GetDescripcion() => descripcionCorta;

    /// <summary>El AticoManager llama esto cuando el orden fue correcto.</summary>
    public void MarcarResuelto()
    {
        resuelto = true;

        // Cambiar material al cuadro completo
        if (materialCompleto != null && rend != null)
            rend.material = materialCompleto;
    }
}