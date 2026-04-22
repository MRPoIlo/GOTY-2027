using UnityEngine;

public class TriggerRejillaVista : MonoBehaviour
{
    private bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activado) return;
        if (other.CompareTag("Player"))
        {
            activado = true;
            NarracionManager.Instance.Narrar("Veo una rejilla arriba... debería alcanzarla, pero no llego.");
            CajaPickup.rejillaVista = true; // habilita interacción con las cajas
        }
    }
}
