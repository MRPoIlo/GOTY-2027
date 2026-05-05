using UnityEngine;

public class TazaCafeInteractivo : MonoBehaviour, IInteractuable
{
    [Header("Texto acción")]
    [SerializeField] private string textoAccion = "Tomar la taza de café";

    private bool yaInteractuado = false;

    public void Interactuar()
    {
        if (yaInteractuado) return;

        string[] narracion = {
            "El café aún conserva su aroma...",
            "No es solo bebida, es un recuerdo que se quedó impregnado en la habitación."
        };

        NarracionManager.Instance?.Narrar(narracion);
        NivelManager1 manager = FindFirstObjectByType<NivelManager1>();
        manager?.RegistrarInteraccion();

        yaInteractuado = true;
    }

    public string ObtenerTextoAccion() => textoAccion;
    public bool EstaActivo() => !yaInteractuado;
}
