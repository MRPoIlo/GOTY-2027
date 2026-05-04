using UnityEngine;

public class CajaInteractuable : MonoBehaviour, IInteractuable
{
    [Header("Texto acción")]
    [SerializeField] private string textoAccion = "Mover caja";

    private bool usada = false;

    public void Interactuar()
    {
        if (usada) return;

        // Solo permitir mover si el manager está en fase de cajas
        if (SalaCocinaManager.Instance != null && SalaCocinaManager.Instance.EstaEnFaseCajas())
        {
            usada = true;
            SalaCocinaManager.Instance.MoverCaja(gameObject);
        }
        else
        {
            NarracionManager.Instance?.Narrar(new string[]
            {
                "Aún no puedo mover las cajas...",
                "Debo revisar primero la puerta de la cocina."
            });
        }
    }

    public string ObtenerTextoAccion() => textoAccion;

    public bool EstaActivo() => !usada;
}
