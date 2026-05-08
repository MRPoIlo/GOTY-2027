using UnityEngine;

public class PuertaSotano : MonoBehaviour, IInteractuable
{
    [Header("Texto acción")]
    [SerializeField] private string textoAccion = "Abrir puerta sótano";

    private bool yaInteractuada = false;

    public void Interactuar()
    {
        if (yaInteractuada) return;

        // Delegar toda la lógica al manager
        SalaCocinaManager.Instance.InteractuarPuertaSotano();

        yaInteractuada = true;
    }

    public string ObtenerTextoAccion() => textoAccion;

    public bool EstaActivo()
    {
        // Solo activa si estamos en la fase del sótano y aún no se interactuó
        return SalaCocinaManager.Instance.GetFaseActual() == SalaCocinaManager.FaseJuego.PuertaSotano
               && !yaInteractuada;
    }
}
