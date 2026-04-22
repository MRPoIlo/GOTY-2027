using UnityEngine;

public class VentRejilla : MonoBehaviour, IInteractuable
{
    public bool EstaActivo() => true;

    public string ObtenerTextoAccion() => "Abrir rejilla [E]";

    public void Interactuar()
    {
        if (NivelManagerBaño.Instance != null)
        {
            NivelManagerBaño.Instance.IntentarAbrirRejilla();
        }
        else
        {
            Debug.LogError("No se encontró el NivelManagerBaño en la escena.");
        }
    }
}
