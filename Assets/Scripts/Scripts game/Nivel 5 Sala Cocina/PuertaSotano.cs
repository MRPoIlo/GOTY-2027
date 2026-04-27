using UnityEngine;

public class PuertaSotano : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SalaCocinaManager.Instance.OnJugadorEscapo();
        }
    }
}