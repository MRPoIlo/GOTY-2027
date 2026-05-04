using UnityEngine;

public class TVInteractivo : MonoBehaviour, IInteractuable
{
    [Header("Texto acción")]
    [SerializeField] private string textoAccion = "Encender TV";

    private bool yaInteractuado = false;

    public void Interactuar()
    {
        if (yaInteractuado) return;

        SalaCocinaManager.Instance.InteractuarTV();
        yaInteractuado = true;
    }

    public string ObtenerTextoAccion() => textoAccion;

    public bool EstaActivo() => !yaInteractuado;
}
