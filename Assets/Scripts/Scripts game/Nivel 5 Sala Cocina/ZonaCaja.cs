using UnityEngine;

public class ZonaCaja : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Caja"))
        {
            if (SalaCocinaManager.Instance != null &&
                SalaCocinaManager.Instance.GetFaseActual() == SalaCocinaManager.FaseJuego.Cajas)
            {
                Debug.Log("[ZonaCaja] Caja " + other.name + " entró en trigger en fase Cajas");
                SalaCocinaManager.Instance.MoverCaja(other.gameObject);
            }
            else
            {
                Debug.Log("[ZonaCaja] Caja " + other.name + " ignorada porque fase actual es "
                          + SalaCocinaManager.Instance?.GetFaseActual());
            }
        }
    }
}
