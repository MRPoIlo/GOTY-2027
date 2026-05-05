using UnityEngine;

public class CajaInteractuable : MonoBehaviour, IInteractuable
{
    [Header("Texto acción")]
    [SerializeField] private string textoAccion = "Coger caja";

    [Header("Referencia de mano")]
    [SerializeField] private Transform manoTransform;

    private bool enManos = false;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
    }

    public void Interactuar()
    {
        // 🔴 Validar fase antes de permitir coger/soltar
        if (SalaCocinaManager.Instance == null ||
            SalaCocinaManager.Instance.GetFaseActual() != SalaCocinaManager.FaseJuego.Cajas)
        {
            NarracionManager.Instance?.Narrar(new string[]
            {
                "Aún no puedo mover las cajas...",
                "Primero debo enfrentar la puerta de la cocina."
            });
            return; // ❌ No permitir coger/soltar
        }

        if (!enManos)
            CogerCaja();
        else
            SoltarCaja();
    }

    private void CogerCaja()
    {
        enManos = true;
        rb.isKinematic = true;
        transform.SetParent(manoTransform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        textoAccion = "Soltar caja";
    }

    private void SoltarCaja()
    {
        enManos = false;
        transform.SetParent(null);
        rb.isKinematic = false;
        textoAccion = "Coger caja";
    }

    public string ObtenerTextoAccion() => textoAccion;

    public bool EstaActivo() => true;
}
