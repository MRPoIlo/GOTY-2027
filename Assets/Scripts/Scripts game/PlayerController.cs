using UnityEngine;

/// <summary>
/// GOTY - Controlador de movimiento en primera persona para Lena.
/// Sistema de animaciones reciclado con States (0=Idle, 1=Walk, 2=Run).
/// Sin salto ni combate, ritmo pausado acorde al tono narrativo.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadCaminar = 2.5f;
    [SerializeField] private float velocidadCorrer  = 4.0f;

    [Header("Cámara")]
    [SerializeField] private float sensibilidadX = 2.0f;
    [SerializeField] private float sensibilidadY = 2.0f;
    [SerializeField] private float limiteVertical = 80f;
    [SerializeField] private Transform camaraTransform;

    [Header("Animaciones")]
    [Tooltip("Arrastra aquí el Animator del modelo de Lena (si lo tienes)")]
    [SerializeField] private Animator anim;

    // Referencias internas
    private CharacterController cc;
    private float rotacionX = 0f;
    private bool bloqueado = false;

    // Gravedad
    private Vector3 velocidadVertical;
    private const float gravedad = -9.81f;

    // Para calcular el estado de animación
    private float h, v;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        BloqueoCursor(true);
    }

    void Update()
    {
        if (bloqueado)
        {
            ActualizarAnimacion(0f, false); // Idle al bloquearse
            return;
        }

        ManejarCamara();
        ManejarMovimiento();
    }

    // ─── Cámara ───────────────────────────────────────────────────────────────

    private void ManejarCamara()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadX;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadY;

        rotacionX -= mouseY;
        rotacionX  = Mathf.Clamp(rotacionX, -limiteVertical, limiteVertical);

        camaraTransform.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // ─── Movimiento ───────────────────────────────────────────────────────────

    private void ManejarMovimiento()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        bool corriendo = Input.GetKey(KeyCode.LeftShift);
        float velocidad = corriendo ? velocidadCorrer : velocidadCaminar;

        Vector3 direccion = transform.right * h + transform.forward * v;
        direccion = Vector3.ClampMagnitude(direccion, 1f);

        // Gravedad
        if (cc.isGrounded && velocidadVertical.y < 0)
            velocidadVertical.y = -2f;

        velocidadVertical.y += gravedad * Time.deltaTime;

        cc.Move((direccion * velocidad + velocidadVertical) * Time.deltaTime);

        // Animaciones
        float moveAmount = Mathf.Abs(h) + Mathf.Abs(v);
        ActualizarAnimacion(moveAmount, corriendo);
    }

    // ─── Animaciones ─────────────────────────────────────────────────────────

    /// <summary>
    /// States: 0 = Idle, 1 = Walk, 2 = Run
    /// Igual que el sistema del proyecto anterior — fácil de expandir.
    /// </summary>
    private void ActualizarAnimacion(float moveAmount, bool corriendo)
    {
        if (anim == null) return;

        if (moveAmount == 0f)
            anim.SetInteger("States", 0); // Idle
        else if (corriendo)
            anim.SetInteger("States", 2); // Run
        else
            anim.SetInteger("States", 1); // Walk
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    public void SetBloqueado(bool estado)
    {
        bloqueado = estado;
        BloqueoCursor(!estado);
    }

    public void ActualizarSensibilidad(float x, float y)
    {
        sensibilidadX = x;
        sensibilidadY = y;
    }

    private void BloqueoCursor(bool bloquear)
    {
        Cursor.lockState = bloquear ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !bloquear;
    }
}