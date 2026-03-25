using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour
{
    private float jumpTimer = 0f;
    private bool isJumping = false;

    public float attackDistance = 2f;
    public float attackDamage = 20f;

    // -------- VIDA --------
    public float maxHealth = 100f;
    public float currentHealth;

    // -------- MOVIMIENTO --------
    private float speed;
    public float Minspeed = 7f, Maxspeed = 15f;
    public float jumpforce = 6f;

    private Rigidbody rb;
    private Animator anim;
    public bool hasKey = false;
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
        currentHealth = maxHealth;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Update()
    {
        if (!IsWin)
        {
            x = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");

            speed = Input.GetKey(KeyCode.LeftShift) ? Maxspeed : Minspeed;

            float moveAmount = Mathf.Abs(x) + Mathf.Abs(z);

            // -------- ANIMACIONES DE MOVIMIENTO --------
            if (IsGrounded)
            {
                if (moveAmount == 0)
                    anim.SetInteger("States", 0);
                else if (Input.GetKey(KeyCode.LeftShift))
                    anim.SetInteger("States", 2);
                else
                    anim.SetInteger("States", 1);
            }

            // -------- SALTO --------
            if (Input.GetKeyDown(KeyCode.Space) && IsGrounded)
            {
                jumpRequest = true;
                anim.SetInteger("States", 3);

                isJumping = true;
                jumpTimer = 0f;
            }

            // -------- ATAQUE --------
            if (Input.GetKeyDown(KeyCode.X))
            {
                anim.SetInteger("States", 4);
                AttackEnemy();
            }

            // -------- CAMARA --------
            float mouseX = Input.GetAxis("Mouse X") * Sencibility;
            float mouseY = Input.GetAxis("Mouse Y") * Sencibility;

            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -LimitX, LimitX);

            cam.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        // -------- DETECCION DE CAIDA --------
        if (!IsGrounded)
        {
            if (isJumping)
            {
                jumpTimer += Time.deltaTime;

                if (jumpTimer >= 1f)
                {
                    isJumping = false;
                }
            }
            else
            {
                if (rb.linearVelocity.y < 0) // solo si realmente está cayendo
                {
                    anim.SetInteger("States", 5);
                }
            }
        }
    }

    void AttackEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackDistance);

        foreach (Collider hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsWin)
        {
            Vector3 move = transform.TransformDirection(new Vector3(x, 0, z)) * speed;
            rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

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
        {
            IsGrounded = true;
            isJumping = false;
        }

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

            if (SpawnPoint != null)
                transform.position = SpawnPoint.position;
        }
    }

    // -------- RECIBIR DAÑO --------
    public void TakeDamage(float percent)
    {
        float damage = maxHealth * percent;
        currentHealth -= damage;

        Debug.Log("Vida restante: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Jugador muerto");
        SceneManager.LoadScene("GameOver");
    }
}