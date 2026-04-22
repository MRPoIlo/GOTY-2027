using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadCaminar = 3f;
    [SerializeField] private float velocidadCorrer = 4.0f;
    [SerializeField] private float velocidadAgachado = 2.0f;

    [Header("Cámara")]
    [SerializeField] private float sensibilidadX = 2.0f;
    [SerializeField] private float sensibilidadY = 2.0f;
    [SerializeField] private float limiteVertical = 80f;
    [SerializeField] private Transform camaraTransform;

    [Header("Agacharse")]
    [SerializeField] private float alturaNormal = 1.6f;
    [SerializeField] private float alturaAgachado = 1.0f;
    [SerializeField] private float zNormal = 0f;
    [SerializeField] private float zAgachado = 0.4f;
    [SerializeField] private float velocidadTransicion = 8f;

    [Header("Animaciones")]
    [SerializeField] private Animator anim;

    private CharacterController cc;
    private float rotacionX = 0f;
    private bool bloqueado = false;

    private Vector3 velocidadVertical;
    private const float gravedad = -9.81f;

    private float h, v;
    private bool agachado = false;

    // Referencia al PausaManager local
    private PausaManager pausaManager;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        BloqueoCursor(true);

        // Buscar el PausaManager de la escena
        pausaManager = FindObjectOfType<PausaManager>();
    }

    void Update()
    {
        // Si está bloqueado o el juego está pausado, no mover
        if (bloqueado || (pausaManager != null && pausaManager.juegoPausado))
        {
            ActualizarAnimacion(0f, false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
            agachado = !agachado;

        ManejarCamara();
        ManejarMovimiento();
        AjustarAlturaCamara();
        AjustarCollider();
    }

    private void ManejarCamara()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadX;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadY;

        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, -limiteVertical, limiteVertical);

        camaraTransform.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void ManejarMovimiento()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        bool corriendo = Input.GetKey(KeyCode.LeftShift) && !agachado;

        float velocidad = velocidadCaminar;
        if (agachado) velocidad = velocidadAgachado;
        else if (corriendo) velocidad = velocidadCorrer;

        Vector3 direccion = transform.right * h + transform.forward * v;
        direccion = Vector3.ClampMagnitude(direccion, 1f);

        if (cc.isGrounded && velocidadVertical.y < 0)
            velocidadVertical.y = -2f;

        velocidadVertical.y += gravedad * Time.deltaTime;

        cc.Move((direccion * velocidad + velocidadVertical) * Time.deltaTime);

        float moveAmount = Mathf.Abs(h) + Mathf.Abs(v);
        ActualizarAnimacion(moveAmount, corriendo);
    }

    private void AjustarAlturaCamara()
    {
        float alturaObjetivo = agachado ? alturaAgachado : alturaNormal;
        float zObjetivo = agachado ? zAgachado : zNormal;

        Vector3 pos = camaraTransform.localPosition;
        pos.y = Mathf.Lerp(pos.y, alturaObjetivo, Time.deltaTime * velocidadTransicion);
        pos.z = Mathf.Lerp(pos.z, zObjetivo, Time.deltaTime * velocidadTransicion);
        camaraTransform.localPosition = pos;
    }

    private void AjustarCollider()
    {
        if (agachado)
        {
            cc.height = alturaAgachado;
            cc.center = new Vector3(cc.center.x, alturaAgachado / 2f, cc.center.z);
        }
        else
        {
            cc.height = alturaNormal;
            cc.center = new Vector3(cc.center.x, alturaNormal / 2f, cc.center.z);
        }
    }

    private void ActualizarAnimacion(float moveAmount, bool corriendo)
    {
        if (anim == null) return;

        if (agachado)
        {
            anim.SetInteger("States", moveAmount == 0f ? 3 : 4);
            return;
        }

        if (moveAmount == 0f) anim.SetInteger("States", 0);
        else if (corriendo) anim.SetInteger("States", 2);
        else anim.SetInteger("States", 1);
    }

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
        Cursor.visible = !bloquear;
    }
}
