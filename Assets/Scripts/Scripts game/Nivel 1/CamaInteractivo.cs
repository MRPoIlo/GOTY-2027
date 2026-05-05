using UnityEngine;

public class CamaInteractivo : MonoBehaviour, IInteractuable
{
    [Header("Texto acción")]
    [SerializeField] private string textoAccion = "Mirar la cama";

    private bool yaInteractuado = false;

    public void Interactuar()
    {
        if (yaInteractuado) return;

        string[] narracion = {
            "Las sábanas guardan la forma de ausencias…",
            "Aquí aprendí a esconderme del ruido."
        };

        NarracionManager.Instance?.Narrar(narracion);
        NivelManager1 manager = FindFirstObjectByType<NivelManager1>();
        manager?.RegistrarInteraccion();

        yaInteractuado = true;
    }

    public string ObtenerTextoAccion() => textoAccion;
    public bool EstaActivo() => !yaInteractuado;
}
