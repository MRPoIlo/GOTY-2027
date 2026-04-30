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

    [Header("Jugador a detectar")]
    [Tooltip("Arrastra aquí el GameObject del jugador")]
    [SerializeField] private GameObject jugador;

    [Header("Narración opcional")]
    [Tooltip("Si tiene contenido, se muestra automáticamente al entrar")]
    [SerializeField, TextArea(2, 5)]
    private string[] narracionAlEntrar;

    [Header("Evento")]
    public UnityEvent OnZonaActivada;

    [Header("Objeto asociado al mensaje")]
    [Tooltip("Arrastra aquí el objeto que aparecerá/desaparecerá")]
    [SerializeField] private GameObject objetoMensaje;

    private bool activado = false;

    void Awake()
    {
        // Asegura que el collider sea trigger
        GetComponent<Collider>().isTrigger = true;

        // Ocultar objeto al inicio
        if (objetoMensaje != null)
            objetoMensaje.SetActive(false);
    }

    void OnTriggerEnter(Collider otro)
    {
        if (usoUnico && activado) return;
        if (jugador != null && otro.gameObject != jugador) return;

        activado = true;

        // Narración automática
        if (narracionAlEntrar != null && narracionAlEntrar.Length > 0)
        {
            NarracionManager.Instance?.Narrar(narracionAlEntrar);

            // Mostrar objeto junto con el mensaje
            if (objetoMensaje != null)
                objetoMensaje.SetActive(true);
        }

        OnZonaActivada?.Invoke();
    }

    // Método para ocultar mensaje y objeto
    public void OcultarMensaje()
    {
        // 🔧 Usamos los métodos reales de NarracionManager
        NarracionManager.Instance?.Detener();
        // o si prefieres cerrar panel y desbloquear jugador:
        // NarracionManager.Instance?.OcultarMensaje();

        if (objetoMensaje != null)
            objetoMensaje.SetActive(false);
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
