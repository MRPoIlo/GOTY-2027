using UnityEngine;

public class CajaPickup : MonoBehaviour, IInteractuable
{
    // Flag global: se activa cuando el jugador ve la rejilla
    public static bool rejillaVista = false;

    private bool siendoLlevada = false;
    private Rigidbody rb;

    [Header("Posición al llevarla")]
    public Transform puntoCarga; // Empty en el Player

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Permitir soltar siempre con E, aunque no estés mirando la caja
        if (siendoLlevada && Input.GetKeyDown(KeyCode.E))
        {
            Soltar();
        }
    }

    // ─── IInteractuable ───────────────────────────────
    public void Interactuar()
    {
        if (!siendoLlevada)
            Recoger();
        else
            Soltar();
    }

    public bool EstaActivo()
    {
        // Solo se puede interactuar si ya se vio la rejilla
        return rejillaVista;
    }

    public string ObtenerTextoAccion()
    {
        return siendoLlevada ? "Soltar [E]" : "Coger [E]";
    }

    // ─── Lógica propia ───────────────────────────────
    void Recoger()
    {
        siendoLlevada = true;
        rb.isKinematic = true;
        transform.SetParent(puntoCarga);
        transform.localPosition = Vector3.zero;
    }

    void Soltar()
    {
        siendoLlevada = false;
        transform.SetParent(null);
        rb.isKinematic = false;
    }
}
