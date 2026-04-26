using UnityEngine;

/// <summary>
/// GOTY — Nivel 4 (Vestíbulo)
/// Trigger invisible al final del vestíbulo.
/// Cuando el jugador llega aquí, el nivel termina aunque el padre esté cerca.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ZonaSalida : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider otro)
    {
        if (!otro.CompareTag("Player")) return;
        VestibuloManager.Instance?.OnJugadorEscapo();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        var col = GetComponent<BoxCollider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.center, col.size);
        }
    }
}