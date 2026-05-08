using UnityEngine;

public class CajaInteractuable : MonoBehaviour, IInteractuable
{
    [Header("Texto acción")]
    [SerializeField] private string textoAccion = "Coger caja";

    [Header("Referencia de mano")]
    [SerializeField] private Transform manoTransform;

    private bool enManos = false;
    private bool yaSoltada = false;
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
        if (yaSoltada) return;

        if (!enManos)
            CogerCaja();
        else
            SoltarCaja();
    }

    private void CogerCaja()
    {
        // ✅ Solo mostrar advertencia si no es fase Cajas, sin llamar MoverCaja
        if (SalaCocinaManager.Instance == null ||
            SalaCocinaManager.Instance.GetFaseActual() != SalaCocinaManager.FaseJuego.Cajas)
        {
            NarracionManager.Instance?.Narrar(new string[]
            {
                "Aún no puedo mover las cajas...",
                "Primero debo enfrentar la puerta de la cocina."
            });
            return;
        }

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
        yaSoltada = true;
        transform.SetParent(null);
        rb.isKinematic = false;
        textoAccion = "Coger caja";

        // ✅ Avisar al manager solo al soltar
        SalaCocinaManager.Instance?.MoverCaja(gameObject);
    }

    public string ObtenerTextoAccion() => textoAccion;

    public bool EstaActivo()
    {
        if (yaSoltada) return false;
        if (SalaCocinaManager.Instance == null) return false;

        var fase = SalaCocinaManager.Instance.GetFaseActual();

        // ✅ Si está en manos siempre debe poder soltarse sin importar la fase
        if (enManos) return true;

        return fase == SalaCocinaManager.FaseJuego.Cajas;
    }
}