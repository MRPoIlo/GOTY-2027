using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// GOTY - Zona de trigger para activar narración u otros eventos
/// al entrar a un área específica.
///
/// Uso: Crea un GameObject vacío, añade BoxCollider (Is Trigger = true)
/// y este script. Conecta el evento en el Inspector.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TriggerZona : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool usoUnico = true;
    [SerializeField] private string tagJugador = "Player";

    [Header("Narración opcional")]
    [Tooltip("Si tiene contenido, se muestra automáticamente al entrar")]
    [SerializeField, TextArea(2, 5)]
    private string[] narracionAlEntrar;

    [Header("Evento")]
    public UnityEvent OnZonaActivada;

    private bool activado = false;

    void Awake()
    {
        // Asegura que el collider sea trigger
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider otro)
    {
        if (usoUnico && activado) return;
        if (!otro.CompareTag(tagJugador)) return;

        activado = true;

        // Narración automática
        if (narracionAlEntrar != null && narracionAlEntrar.Length > 0)
            NarracionManager.Instance?.Narrar(narracionAlEntrar);

        OnZonaActivada?.Invoke();
    }

    public void Reactivar() => activado = false;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.25f);
        var col = GetComponent<BoxCollider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.center, col.size);
        }
    }
}