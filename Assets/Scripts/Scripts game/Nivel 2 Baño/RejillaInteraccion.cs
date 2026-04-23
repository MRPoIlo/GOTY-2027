using UnityEngine;

public class RejillaInteraccion : MonoBehaviour
{
    [Header("Rango de interacción")]
    [SerializeField] private float rangoInteraccion = 2.5f;

    [Header("UI opcional: mensaje 'Pulsa E para interactuar'")]
    [SerializeField] private GameObject mensajeInteraccion;

    private Transform jugador;
    private bool jugadorEnRango = false;

    private void Start()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) jugador = go.transform;

        if (mensajeInteraccion != null) mensajeInteraccion.SetActive(false);
    }

    private void Update()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);
        jugadorEnRango = distancia <= rangoInteraccion;

        if (mensajeInteraccion != null)
            mensajeInteraccion.SetActive(jugadorEnRango);

        if (jugadorEnRango && Input.GetKeyDown(KeyCode.E))
        {
            NivelManagerBaño.Instance?.IntentarAbrirRejilla();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangoInteraccion);
    }
}
