using UnityEngine;

public class CajaPickup : MonoBehaviour, IInteractuable
{
    public static bool rejillaVista = false;

    private bool siendoLlevada = false;

    private Rigidbody rb;

    [Header("Punto de carga")]
    public Transform puntoCarga;

    [Header("Distancia suavizado")]
    [SerializeField] private float velocidadMovimiento = 15f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (siendoLlevada &&
            Input.GetKeyDown(KeyCode.E))
        {
            Soltar();
        }
    }

    private void FixedUpdate()
    {
        // 🔥 MUY IMPORTANTE
        // Mantiene estable la caja en Build

        if (siendoLlevada && puntoCarga != null)
        {
            rb.MovePosition(
                Vector3.Lerp(
                    transform.position,
                    puntoCarga.position,
                    velocidadMovimiento * Time.fixedDeltaTime
                )
            );

            rb.MoveRotation(
                Quaternion.Lerp(
                    transform.rotation,
                    puntoCarga.rotation,
                    velocidadMovimiento * Time.fixedDeltaTime
                )
            );
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
        siendoLlevada = true;

        rb.useGravity = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.constraints =
            RigidbodyConstraints.FreezeRotation;
    }

    // ─────────────────────────────────────
    // SOLTAR
    // ─────────────────────────────────────

    private void Soltar()
    {
        siendoLlevada = false;

        rb.useGravity = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.constraints =
            RigidbodyConstraints.None;
    }
}