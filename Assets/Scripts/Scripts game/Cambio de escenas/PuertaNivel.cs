using UnityEngine;
using UnityEngine.SceneManagement;

public class PuertaNivel : MonoBehaviour, IInteractuable
{
    [SerializeField] private string textoAccion = "Abrir puerta";
    [SerializeField] private string nombreEscenaDestino = "Nivel1";

    private bool habilitada = false;

    // Método público para habilitar la puerta (lo llamas desde el sistema de tu compañero)
    public void HabilitarPuerta()
    {
        habilitada = true;
    }

    // Método de interacción (cuando el jugador interactúa con la puerta)
    public void Interactuar()
    {
        if (!habilitada) return;

        Debug.Log("Puerta abierta, cargando " + nombreEscenaDestino);
        SceneManager.LoadScene(nombreEscenaDestino);
    }

    public string ObtenerTextoAccion() => textoAccion;

    public bool EstaActivo() => habilitada;
}
