using UnityEngine;

public class PuertaSotano : MonoBehaviour, IInteractuable
{
    [Header("Texto acción")]
    [SerializeField] private string textoAccion = "Abrir puerta sótano";

    public void Interactuar()
    {
        SalaCocinaManager.Instance.OnJugadorEscapo();
    }

    public string ObtenerTextoAccion() => textoAccion;

    public bool EstaActivo() => SalaCocinaManager.Instance.PuedeEscapar();
}
