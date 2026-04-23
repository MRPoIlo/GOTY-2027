using UnityEngine;
using UnityEngine.SceneManagement;

public class PuertaNivel1 : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string escenaSiguiente = "Nivel2Baño";
    [SerializeField] private string mensajeAbrir = "Presiona E para abrir la puerta";

    [Header("Interacción")]
    [SerializeField] private float rangoInteraccion = 2.5f; // 🔹 distancia máxima para interactuar

    private Transform jugador;
    private bool mostrandoMensaje = false;

    void Start()
    {
        // Buscar al jugador por tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            jugador = playerObj.transform;
    }

    void Update()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= rangoInteraccion)
        {
            if (!mostrandoMensaje)
            {
                NarracionManager.Instance?.Narrar(mensajeAbrir);
                mostrandoMensaje = true;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                AbrirPuerta();
            }
        }
        else
        {
            if (mostrandoMensaje)
            {
                NarracionManager.Instance?.OcultarMensaje();
                mostrandoMensaje = false;
            }
        }
    }

    private void AbrirPuerta()
    {
        SceneManager.LoadScene(escenaSiguiente);
    }
}
