using UnityEngine;

public class DiarioInteractivo : MonoBehaviour, IInteractuable
{
    [Header("Texto acción")]
    [SerializeField] private string textoAccion = "Leer el diario";

    private bool yaInteractuado = false;

    public void Interactuar()
    {
        if (yaInteractuado) return;

        string[] narracion = {
            "Las páginas hablan en silencio…",
            "Palabras que nunca se dijeron en voz alta."
        };

        NarracionManager.Instance?.Narrar(narracion);
        NivelManager1 manager = FindFirstObjectByType<NivelManager1>();
        manager?.RegistrarInteraccion();

        yaInteractuado = true;
    }

    public string ObtenerTextoAccion() => textoAccion;
    public bool EstaActivo() => !yaInteractuado;
}
