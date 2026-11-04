using UnityEngine;

/// <summary>
/// Caja que se puede coger y soltar (mismo comportamiento que CajaPickup del Baño).
/// - Al cogerla se queda fija en el punto "Mano" (sin gravedad) hasta que el jugador la suelta.
/// - Se suelta con E (no hace falta estar mirándola).
/// - Al soltarla vuelve a tener físicas y avisa al SalaCocinaManager.
///
/// IMPORTANTE: el nombre de la clase debe coincidir con el del archivo (CajaInteractiva.cs);
/// si no, Unity no puede cargar el componente en la escena.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CajaInteractiva : MonoBehaviour, IInteractuable
{
    [Header("Texto acción")]
    [SerializeField] private string textoAccion = "Coger caja";
    [SerializeField] private string textoSoltar = "Soltar caja [E]";

    [Header("Referencia de mano")]
    [Tooltip("Punto delante del jugador donde se sostiene la caja. Si está vacío se busca un objeto llamado 'Mano'.")]
    [SerializeField] private Transform manoTransform;

    private bool enManos = false;
    private int frameUltimoCambio = -1; // evita coger y soltar con la misma pulsación de E

    private Rigidbody rb;
    private Collider[] colisionadores;
    private Collider colliderJugador;
    private PlayerController jugador;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        colisionadores = GetComponentsInChildren<Collider>();
    }

    void Start()
    {
        jugador = FindFirstObjectByType<PlayerController>();
        if (jugador != null)
            colliderJugador = jugador.GetComponent<Collider>(); // CharacterController es un Collider

        if (manoTransform == null)
        {
            var mano = GameObject.Find("Mano");
            if (mano != null) manoTransform = mano.transform;
        }

        if (manoTransform == null)
            Debug.LogWarning($"[CajaInteractiva] {name}: no hay 'Mano' asignada; no se podrá coger.");
    }

    void Update()
    {
        // Soltar con E en cualquier momento mientras se lleva
        if (!enManos || Time.frameCount == frameUltimoCambio) return;
        if (NarracionManager.Instance != null && NarracionManager.Instance.EstaActivo()) return; // E avanza la narración
        if (jugador != null && jugador.IsBloqueado()) return;

        if (Input.GetKeyDown(KeyCode.E))
            SoltarCaja();
    }

    // ─────────────────────────────────────
    // IInteractuable
    // ─────────────────────────────────────

    public void Interactuar()
    {
        if (Time.frameCount == frameUltimoCambio) return;

        if (!enManos) CogerCaja();
        else          SoltarCaja();
    }

    public string ObtenerTextoAccion() => enManos ? textoSoltar : textoAccion;

    public bool EstaActivo()
    {
        if (enManos) return true; // siempre se puede soltar
        if (SalaCocinaManager.Instance == null) return false;
        return SalaCocinaManager.Instance.GetFaseActual() == SalaCocinaManager.FaseJuego.Cajas;
    }

    // ─────────────────────────────────────
    // COGER
    // ─────────────────────────────────────

    private void CogerCaja()
    {
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

        if (manoTransform == null) return;

        enManos = true;
        frameUltimoCambio = Time.frameCount;

        // Sin físicas mientras se lleva: no cae ni empuja al jugador
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None; // si no, la caja se queda rezagada respecto a la mano
        IgnorarJugador(true);

        // Exactamente en el punto de la mano
        transform.SetParent(manoTransform, true);
        transform.position = manoTransform.position;
        transform.rotation = manoTransform.rotation;
    }

    // ─────────────────────────────────────
    // SOLTAR
    // ─────────────────────────────────────

    private void SoltarCaja()
    {
        enManos = false;
        frameUltimoCambio = Time.frameCount;

        transform.SetParent(null, true);
        ColocarDelanteDelJugador();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        IgnorarJugador(false);

        SalaCocinaManager.Instance?.MoverCaja(gameObject);
    }

    /// <summary>
    /// La "Mano" está muy pegada al cuerpo: al soltar, la caja quedaría metida dentro del
    /// jugador. Se mueve justo delante de él (si no hay una pared en ese sitio).
    /// </summary>
    private void ColocarDelanteDelJugador()
    {
        if (!(colliderJugador is CharacterController cc) || colisionadores.Length == 0) return;

        Bounds b = colisionadores[0].bounds;
        foreach (var c in colisionadores)
            if (c != null && !c.isTrigger) b.Encapsulate(c.bounds);

        Transform tj = cc.transform;
        Vector3 adelante = Vector3.ProjectOnPlane(tj.forward, Vector3.up).normalized;
        if (adelante == Vector3.zero) return;

        float medio = Mathf.Max(b.extents.x, b.extents.z);
        float distancia = cc.radius + medio + 0.1f;

        Vector3 centroJugador = new Vector3(tj.position.x, b.center.y, tj.position.z);
        Vector3 destino = centroJugador + adelante * distancia;

        // Solo moverla si el sitio está libre (ignorando la propia caja, el jugador y triggers)
        foreach (var h in Physics.OverlapBox(destino, b.extents * 0.95f, transform.rotation,
                     ~0, QueryTriggerInteraction.Ignore))
        {
            if (h == colliderJugador || System.Array.IndexOf(colisionadores, h) >= 0) continue;
            return; // hay algo (pared, mueble): se deja donde está
        }

        transform.position += destino - b.center;
    }

    private void IgnorarJugador(bool ignorar)
    {
        if (colliderJugador == null) return;
        foreach (var c in colisionadores)
            if (c != null && !c.isTrigger)
                Physics.IgnoreCollision(c, colliderJugador, ignorar);
    }
}
