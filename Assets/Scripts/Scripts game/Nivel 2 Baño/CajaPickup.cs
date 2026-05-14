using UnityEngine;

public class CajaPickup : MonoBehaviour, IInteractuable
{
    public static bool rejillaVista = false;

    private bool siendoLlevada = false;

    private Rigidbody rb;
    private Collider col;

    [Header("Punto de carga")]
    [SerializeField] private Transform puntoCarga;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Buscar automáticamente el punto si no está asignado
        if (puntoCarga == null)
        {
            GameObject punto = GameObject.Find("PuntoCarga");

            if (punto != null)
                puntoCarga = punto.transform;
        }
    }

    private void Update()
    {
        if (siendoLlevada && Input.GetKeyDown(KeyCode.E))
        {
            Soltar();
        }
    }

    // ─────────────────────────────────────
    // INTERACTUAR
    // ─────────────────────────────────────

    public void Interactuar()
    {
        if (!siendoLlevada)
            Recoger();
        else
            Soltar();
    }

    public bool EstaActivo()
    {
        return rejillaVista;
    }

    public string ObtenerTextoAccion()
    {
        return siendoLlevada
            ? "Soltar [E]"
            : "Coger [E]";
    }

    // ─────────────────────────────────────
    // RECOGER
    // ─────────────────────────────────────

    private void Recoger()
    {
        if (puntoCarga == null) return;

        siendoLlevada = true;

        // Desactivar físicas
        rb.isKinematic = true;
        rb.useGravity = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Poner caja EXACTAMENTE en el punto
        transform.position = puntoCarga.position;
        transform.rotation = puntoCarga.rotation;

        // Hacer hija del punto
        transform.SetParent(puntoCarga);
    }

    // ─────────────────────────────────────
    // SOLTAR
    // ─────────────────────────────────────

    private void Soltar()
    {
        siendoLlevada = false;

        // Soltar del punto
        transform.SetParent(null);

        // Reactivar físicas
        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}