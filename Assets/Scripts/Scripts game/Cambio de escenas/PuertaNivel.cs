using UnityEngine;
using UnityEngine.SceneManagement;

public class PuertaNivel : MonoBehaviour, IInteractuable
{
    [SerializeField] private string textoAccion = "Abrir puerta";
    [SerializeField] private string nombreEscenaDestino = "Nivel1";

    private bool habilitada = false;

    // M�todo p�blico para habilitar la puerta (lo llamas desde el sistema de tu compa�ero)
    public void HabilitarPuerta()
    {
        habilitada = true;
    }

    // M�todo de interacci�n (cuando el jugador interact�a con la puerta)
    public void Interactuar()
    {
        if (!habilitada) return;

        Debug.Log("Puerta abierta, cargando " + nombreEscenaDestino);
        SceneManager.LoadScene(nombreEscenaDestino);
    }

    public string ObtenerTextoAccion() => textoAccion;

    public bool EstaActivo() => habilitada;
}