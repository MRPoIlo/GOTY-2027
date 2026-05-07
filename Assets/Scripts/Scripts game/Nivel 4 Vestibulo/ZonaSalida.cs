using UnityEngine;

/// <summary>
/// GOTY — Nivel 4 (Vestíbulo)
/// Detecta cuando el jugador llega a la zona de salida por distancia.
/// Compatible con CharacterController.
/// </summary>
public class ZonaSalida : MonoBehaviour
{
    [SerializeField] private float distanciaDeteccion = 2f;

    private Transform jugador;
    private bool activado = false;

    void Start()
    {
        var pc = FindObjectOfType<PlayerController>();
        if (pc != null) jugador = pc.transform;
    }

    void Update()
    {
        if (activado || jugador == null) return;

        float dist = Vector3.Distance(transform.position, jugador.position);
        if (dist <= distanciaDeteccion)
        {
            activado = true;
            Debug.Log("[ZonaSalida] Jugador llegó a la salida");
            VestibuloManager.Instance?.OnJugadorEscapo();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccion);
    }
}