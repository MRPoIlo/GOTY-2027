using UnityEngine;

public class CajaInteractuable : MonoBehaviour, IInteractuable
{
    [Header("Texto acción")]
    [SerializeField]
    private string textoAccion =
        "Coger caja";

    [Header("Referencia de mano")]
    [SerializeField] private Transform manoTransform;

    private bool enManos = false;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = true;
        rb.isKinematic = false;
    }

    public void Interactuar()
    {
        if (!enManos)
            CogerCaja();
        else
            SoltarCaja();
    }

    // ─────────────────────────────────────
    // COGER
    // ─────────────────────────────────────

    private void CogerCaja()
    {
        // ✅ Solo permitir mover en fase cajas

        if (SalaCocinaManager.Instance == null ||
            SalaCocinaManager.Instance.GetFaseActual() !=
            SalaCocinaManager.FaseJuego.Cajas)
        {
            NarracionManager.Instance?.Narrar(
                new string[]
                {
                    "Aún no puedo mover las cajas...",
                    "Primero debo enfrentar la puerta de la cocina."
                }
            );

            return;
        }

        enManos = true;

        rb.isKinematic = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.SetParent(manoTransform);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        textoAccion = "Soltar caja";
    }

    // ─────────────────────────────────────
    // SOLTAR
    // ─────────────────────────────────────

    private void SoltarCaja()
    {
        enManos = false;

        transform.SetParent(null);

        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        textoAccion = "Coger caja";

        // ✅ Avisar manager
        SalaCocinaManager.Instance
            ?.MoverCaja(gameObject);
    }

    // ─────────────────────────────────────
    // UI
    // ─────────────────────────────────────

    public string ObtenerTextoAccion()
    {
        return textoAccion;
    }

    public bool EstaActivo()
    {
        if (SalaCocinaManager.Instance == null)
            return false;

        var fase =
            SalaCocinaManager.Instance
            .GetFaseActual();

        // ✅ Si la tiene en manos
        // siempre puede soltar

        if (enManos)
            return true;

        return fase ==
            SalaCocinaManager.FaseJuego.Cajas;
    }
}