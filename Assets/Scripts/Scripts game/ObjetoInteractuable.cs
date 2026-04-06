using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// GOTY - Componente genérico para objetos interactuables de la Escena 1.
/// Arrastra este componente a cualquier objeto de la casa (fotografía,
/// cajón, teléfono, etc.) y configura la narración desde el Inspector.
///
/// Implementa IInteractuable para ser detectado por InteractionSystem.
/// </summary>
public class ObjetoInteractuable : MonoBehaviour, IInteractuable
{
    [Header("Texto")]
    [Tooltip("Texto que aparece en el ícono de interacción (ej: 'Examinar')")]
    [SerializeField] private string textoAccion = "Examinar";

    [Header("Narración")]
    [Tooltip("Líneas de voz interna que se muestran al interactuar. Una por elemento.")]
    [SerializeField, TextArea(2, 5)]
    private string[] lineasNarracion;

    [Header("Comportamiento")]
    [Tooltip("Si es true, solo puede interactuarse una vez")]
    [SerializeField] private bool usoUnico = true;
    [Tooltip("Si es true, el objeto se desactiva visualmente después de interactuar")]
    [SerializeField] private bool ocultarAlInteractuar = false;

    [Header("Evento opcional")]
    [Tooltip("Se invoca después de la narración. Útil para abrir puertas, activar luces, etc.")]
    public UnityEvent OnInteractuado;

    // Estado
    private bool activo = true;
    private bool yaInteractuado = false;

    // ─── IInteractuable ───────────────────────────────────────────────────────

    public void Interactuar()
    {
        if (!activo || (usoUnico && yaInteractuado)) return;

        yaInteractuado = true;

        // Mostrar narración si hay líneas definidas
        if (lineasNarracion != null && lineasNarracion.Length > 0)
            NarracionManager.Instance?.Narrar(lineasNarracion);

        // Disparar evento Unity (conectar en Inspector)
        OnInteractuado?.Invoke();

        // Ocultar objeto si se configuró
        if (ocultarAlInteractuar)
            gameObject.SetActive(false);

        // Desactivar interacción si es de uso único
        if (usoUnico)
            activo = false;
    }

    public string ObtenerTextoAccion() => textoAccion;

    public bool EstaActivo() => activo && !(usoUnico && yaInteractuado);

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>Reactiva el objeto (útil para puzzles de reinicio).</summary>
    public void Reactivar()
    {
        activo = true;
        yaInteractuado = false;
        gameObject.SetActive(true);
    }

    public void Desactivar() => activo = false;

    // Indicador visual en editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, 0.15f);
    }
}