using UnityEngine;

public class FotoInteractivo : MonoBehaviour, IInteractuable
{
    [Header("Texto acción")]
    [SerializeField] private string textoAccion = "Observar la foto";

    private bool yaInteractuado = false;

    public void Interactuar()
    {
        if (yaInteractuado) return;

        string[] narracion = {
            "La sonrisa congelada en la foto oculta gritos que nunca se ven."
        };

        NarracionManager.Instance?.Narrar(narracion);
        NivelManager1 manager = FindFirstObjectByType<NivelManager1>();
        manager?.RegistrarInteraccion();

        yaInteractuado = true;
    }

    public string ObtenerTextoAccion() => textoAccion;
    public bool EstaActivo() => !yaInteractuado;
}
