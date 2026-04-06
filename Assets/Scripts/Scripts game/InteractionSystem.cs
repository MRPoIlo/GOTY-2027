using UnityEngine;

/// <summary>
/// GOTY - Sistema de interacción contextual.
/// Lanza un raycast desde la cámara; si golpea un IInteractuable,
/// muestra el ícono y permite presionar E para activarlo.
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float alcanceInteraccion = 2.5f;
    [SerializeField] private LayerMask capasInteractuables;
    [SerializeField] private Transform camaraTransform;

    [Header("UI Referencia")]
    [SerializeField] private GameObject iconoInteraccion;   // Canvas World o Screen
    [SerializeField] private TMPro.TextMeshProUGUI textoAccion; // "Examinar" / "Recoger"

    private IInteractuable objetoActual = null;
    private bool puedeInteractuar = false;

    void Update()
    {
        // Bloquear interacción mientras hay narración activa
        if (NarracionManager.Instance != null && NarracionManager.Instance.EstaActivo())
        {
            MostrarIcono(false, "");
            return;
        }

        BuscarObjetoInteractuable();

        if (puedeInteractuar && Input.GetKeyDown(KeyCode.E))
        {
            objetoActual?.Interactuar();
        }
    }

    private void BuscarObjetoInteractuable()
    {
        Ray rayo = new Ray(camaraTransform.position, camaraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(rayo, out hit, alcanceInteraccion, capasInteractuables))
        {
            IInteractuable interactuable = hit.collider.GetComponent<IInteractuable>();

            if (interactuable != null && interactuable.EstaActivo())
            {
                // Nuevo objeto detectado
                if (interactuable != objetoActual)
                {
                    objetoActual = interactuable;
                    MostrarIcono(true, interactuable.ObtenerTextoAccion());
                }

                puedeInteractuar = true;
                return;
            }
        }

        // Sin objeto válido
        if (objetoActual != null)
        {
            objetoActual = null;
            MostrarIcono(false, "");
        }

        puedeInteractuar = false;
    }

    private void MostrarIcono(bool mostrar, string texto)
    {
        if (iconoInteraccion != null)
            iconoInteraccion.SetActive(mostrar);

        if (textoAccion != null)
            textoAccion.text = texto;
    }

    // Visualización del alcance en editor
    void OnDrawGizmosSelected()
    {
        if (camaraTransform == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(camaraTransform.position,
                       camaraTransform.forward * alcanceInteraccion);
    }
}