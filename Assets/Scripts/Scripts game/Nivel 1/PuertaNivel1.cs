using UnityEngine;
using UnityEngine.SceneManagement;

public class PuertaNivel1 : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string escenaSiguiente = "Nivel2Baño"; // cámbialo al nombre real de tu escena
    [SerializeField] private string mensajeAbrir = "Presiona E para abrir la puerta";

    private bool jugadorCerca = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            NarracionManager.Instance?.Narrar(mensajeAbrir);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            NarracionManager.Instance?.OcultarMensaje();
        }
    }

    private void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            AbrirPuerta();
        }
    }

    private void AbrirPuerta()
    {
        SceneManager.LoadScene(escenaSiguiente);
    }
}
