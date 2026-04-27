using UnityEngine;

public class CajaInteractiva : MonoBehaviour
{
    private bool usada = false;

    private void OnTriggerEnter(Collider other)
    {
        if (usada) return;

        if (other.CompareTag("Player"))
        {
            usada = true;
            SalaCocinaManager.Instance.MoverCaja(gameObject);
        }
    }
}