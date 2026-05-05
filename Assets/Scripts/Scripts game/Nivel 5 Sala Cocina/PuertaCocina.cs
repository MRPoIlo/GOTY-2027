using UnityEngine;

public class PuertaCocina : MonoBehaviour, IInteractuable
{
    [Header("Texto acción")]
    [SerializeField] private string textoAccion = "Abrir puerta";

    [Header("Narración")]
    [TextArea(2, 5)]
    [SerializeField] private string[] lineasNarracion;

    private bool yaInteractuada = false;

    public void Interactuar()
    {
        if (yaInteractuada) return;

        // Mostrar narración propia
        if (lineasNarracion != null && lineasNarracion.Length > 0)
            NarracionManager.Instance?.Narrar(lineasNarracion);

        // Avisar al manager para cambiar de fase y activar enemigo
        SalaCocinaManager.Instance.InteractuarPuertaCocina();

        yaInteractuada = true;
    }

    public string ObtenerTextoAccion() => textoAccion;

    public bool EstaActivo() => !yaInteractuada;
}
