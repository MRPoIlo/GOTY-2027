using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // -------- MOVIMIENTO --------
    private float speed = 7f;
    public float Minspeed = 7f, Maxspeed = 15f;
    public float jumpforce = 6f;

    private Rigidbody rb;
    private Animator anim;

    public bool IsGrounded;
    private bool IsWin;
    public Transform SpawnPoint;

    private float x;
    private float z;
    private bool jumpRequest;

    // -------- CAMARA --------
    public float Sencibility = 2f;
    public float LimitX = 40f;
    public Transform cam;
    private float rotationX = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!IsWin)
        {
            // -------- INPUT --------
            x = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");

            speed = Input.GetKey(KeyCode.LeftShift) ? Maxspeed : Minspeed;

            // -------- ANIMACIONES --------
            float moveAmount = Mathf.Abs(x) + Mathf.Abs(z);

            if (moveAmount == 0)
                anim.SetInteger("States", 0); // Idle
            else if (Input.GetKey(KeyCode.LeftShift))
                anim.SetInteger("States", 2); // Run
            else
                anim.SetInteger("States", 1); // Walking

            // -------- SALTO --------
            if (Input.GetKeyDown(KeyCode.Space))
                jumpRequest = true;

            // -------- CAMARA --------
            float mouseX = Input.GetAxis("Mouse X") * Sencibility;
            float mouseY = Input.GetAxis("Mouse Y") * Sencibility;

            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -LimitX, LimitX);
            cam.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

            transform.Rotate(Vector3.up * mouseX);
        }
    }

    void FixedUpdate()
    {
        if (!IsWin)
        {
            Vector3 move = new Vector3(x, 0, z) * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + transform.TransformDirection(move));

            if (jumpRequest && IsGrounded)
            {
                rb.AddForce(Vector3.up * jumpforce, ForceMode.Impulse);
            }

            jumpRequest = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            IsGrounded = true;

        if (collision.gameObject.CompareTag("End"))
        {
            UIManager.inst.ShowWinScreen();
            IsWin = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            IsGrounded = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GameOver"))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = SpawnPoint.position;
        }
    }
}