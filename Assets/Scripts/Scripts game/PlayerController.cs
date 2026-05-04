using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadCaminar = 3f;
    [SerializeField] private float velocidadCorrer = 4f;
    [SerializeField] private float velocidadAgachado = 2f;

    [Header("Cámara")]
    [SerializeField] private float sensibilidadX = 2f;
    [SerializeField] private float sensibilidadY = 2f;
    [SerializeField] private float limiteVertical = 80f;
    [SerializeField] private Transform camaraTransform;

    [Header("Agacharse")]
    [SerializeField] private float alturaNormal = 1.6f;
    [SerializeField] private float alturaAgachado = 1f;
    [SerializeField] private float zNormal = 0f;
    [SerializeField] private float zAgachado = 0.4f;
    [SerializeField] private float velocidadTransicion = 8f;

    [Header("Animaciones")]
    [SerializeField] private Animator anim;

    private CharacterController cc;
    private float rotacionX;
    private bool bloqueado;
    private Vector3 velocidadVertical;
    private bool agachado;
    private const float GRAVEDAD = -9.81f;
    private PausaManager pausaManager;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        BloqueoCursor(true);
        pausaManager = FindObjectOfType<PausaManager>();
    }

    void Start()
    {
        if (!cc.isGrounded) cc.Move(Vector3.down * 2f);
    }

    void Update()
    {
        bool pausado = pausaManager != null && pausaManager.juegoPausado;
        if (bloqueado || pausado) { AplicarGravedad(); ActualizarAnimacion(0f, false); return; }

        if (Input.GetKeyDown(KeyCode.LeftControl)) agachado = !agachado;
        ManejarCamara();
        ManejarMovimiento();
        AjustarAlturaCamara();
        AjustarCollider();
    }

    private void ManejarCamara()
    {
        rotacionX -= Input.GetAxis("Mouse Y") * sensibilidadY;
        rotacionX = Mathf.Clamp(rotacionX, -limiteVertical, limiteVertical);
        if (camaraTransform != null)
            camaraTransform.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * sensibilidadX);
    }

    private void ManejarMovimiento()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool corriendo = Input.GetKey(KeyCode.LeftShift) && !agachado;

        float vel = agachado ? velocidadAgachado : corriendo ? velocidadCorrer : velocidadCaminar;
        Vector3 dir = Vector3.ClampMagnitude(transform.right * h + transform.forward * v, 1f);

        AplicarGravedad();
        cc.Move((dir * vel + velocidadVertical) * Time.deltaTime);
        ActualizarAnimacion(Mathf.Abs(h) + Mathf.Abs(v), corriendo);
    }

    private void AplicarGravedad()
    {
        if (cc.isGrounded && velocidadVertical.y < 0) velocidadVertical.y = -2f;
        velocidadVertical.y += GRAVEDAD * Time.deltaTime;
        cc.Move(velocidadVertical * Time.deltaTime);
    }

    private void AjustarAlturaCamara()
    {
        if (camaraTransform == null) return;
        Vector3 pos = camaraTransform.localPosition;
        pos.y = Mathf.Lerp(pos.y, agachado ? alturaAgachado : alturaNormal, Time.deltaTime * velocidadTransicion);
        pos.z = Mathf.Lerp(pos.z, agachado ? zAgachado : zNormal, Time.deltaTime * velocidadTransicion);
        camaraTransform.localPosition = pos;
    }

    private void AjustarCollider()
    {
        cc.height = agachado ? alturaAgachado : alturaNormal;
        cc.center = new Vector3(cc.center.x, cc.height * 0.5f, cc.center.z);
    }

    private void ActualizarAnimacion(float move, bool corriendo)
    {
        if (anim == null) return;
        if (agachado) { anim.SetInteger("States", move == 0f ? 3 : 4); return; }
        anim.SetInteger("States", move == 0f ? 0 : corriendo ? 2 : 1);
    }

    public void SetBloqueado(bool estado)
    {
        bloqueado = estado;
        BloqueoCursor(!estado);
        Debug.Log($"[PC] SetBloqueado({estado}) en {gameObject.name}");
        if (!estado && !cc.isGrounded) cc.Move(Vector3.down * 2f);
    }

    public bool IsBloqueado() => bloqueado;

    public void ActualizarSensibilidad(float x, float y) { sensibilidadX = x; sensibilidadY = y; }

    private void BloqueoCursor(bool bloquear)
    {
        Cursor.lockState = bloquear ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !bloquear;
    }
}
