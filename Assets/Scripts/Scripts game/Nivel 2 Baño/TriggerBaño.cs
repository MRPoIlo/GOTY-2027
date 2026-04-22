using UnityEngine;

public class TriggerBaño : MonoBehaviour
{
    private bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activado) return;
        if (other.CompareTag("Player"))
        {
            activado = true;

            // Narración inicial
            NarracionManager.Instance.Narrar("Busca una salida rápido antes que te alcance");

            // Iniciar contador
            FindFirstObjectByType<TimerBaño>().IniciarTimer();
        }
    }
}
