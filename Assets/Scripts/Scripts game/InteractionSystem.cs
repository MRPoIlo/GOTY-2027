using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float alcanceInteraccion = 2.5f;
    [SerializeField] private Transform camaraTransform;

    [Header("UI Referencia")]
    [SerializeField] private GameObject iconoInteraccion;
    [SerializeField] private TMPro.TextMeshProUGUI textoAccion;

    private IInteractuable objetoActual = null;
    private bool puedeInteractuar = false;

    void Update()
    {
        if (NarracionManager.Instance != null && NarracionManager.Instance.EstaActivo())
        {
            MostrarIcono(false, "");
            return;
        }

        BuscarObjetoInteractuable();

        if (puedeInteractuar && Input.GetKeyDown(KeyCode.E))
            objetoActual?.Interactuar();
    }

    private void BuscarObjetoInteractuable()
    {
        Ray rayo = new Ray(camaraTransform.position, camaraTransform.forward);
        RaycastHit hit;

        // Raycast contra TODO, no solo interactuables
        if (Physics.Raycast(rayo, out hit, alcanceInteraccion))
        {
            IInteractuable interactuable = hit.collider.GetComponent<IInteractuable>();

            if (interactuable != null && interactuable.EstaActivo())
            {
                if (interactuable != objetoActual)
                {
                    objetoActual = interactuable;
                    MostrarIcono(true, interactuable.ObtenerTextoAccion());
                }

                puedeInteractuar = true;
                return;
            }
        }

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

    void OnDrawGizmosSelected()
    {
        if (camaraTransform == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(camaraTransform.position, camaraTransform.forward * alcanceInteraccion);
    }
}
