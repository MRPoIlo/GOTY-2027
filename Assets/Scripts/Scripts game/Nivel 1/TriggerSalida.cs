using UnityEngine;

public class TriggerEscape : MonoBehaviour
{
    [SerializeField] private EnemyAI enemigo;
    [SerializeField] private AudioSource sonidoEscape;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Narración
            NarracionManager.Instance?.Narrar(new string[] { "¡Me escucharon... corre!" });

            // Sonido
            if (sonidoEscape != null) sonidoEscape.Play();

            // Activar persecución directa
            if (enemigo != null)
            {
                enemigo.ForzarPersecucion(); // usa la función del EnemyAI
            }
        }
    }
}
