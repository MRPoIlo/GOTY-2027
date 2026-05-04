using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    [Header("Jugador")]
    public Transform player;

    [Header("Detección")]
    public float visionRange = 6f;
    public float visionAngle = 90f;
    public float detectionTime = 2f;
    public float catchDistance = 1.5f;

    [Header("UI Estado")]
    public Image stateIcon;
    public Sprite noVisualizaSprite;
    public Sprite visualizaSprite;
    public Sprite persigueSprite;

    [Header("Jumpscare")]
    public GameObject jumpscareUI;

    [Header("Patrulla")]
    [SerializeField] private Transform[] puntosPatrulla;
    private int indicePatrulla = 0;

    [Header("Movimiento")]
    public float velocidadGiro = 3f;

    private NavMeshAgent agent;
    private Animator animator;

    private float currentDetection = 0f;
    private float tiempoPerdido = 0f;

    private bool activo = false;
    private bool persiguiendo = false;
    private bool yaActivoJumpscare = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        agent.updateRotation = false;
        agent.isStopped = true;

        if (jumpscareUI != null)
            jumpscareUI.SetActive(false);

        // 🔥 Reset limpio (clave para build)
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            animator.SetFloat("Speed", 0f);
            animator.SetBool("Moving", false);
        }
    }

    void Update()
    {
        if (!activo || yaActivoJumpscare) return;

        Vector3 dirToPlayer = player.position - transform.position;
        float distancia = dirToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        bool veJugador = distancia < visionRange && angle < visionAngle / 2f;

        // ================= DETECCIÓN =================
        if (veJugador)
        {
            currentDetection += Time.deltaTime;
            tiempoPerdido = 0.3f;

            if (!persiguiendo && currentDetection >= detectionTime)
            {
                persiguiendo = true;
                agent.isStopped = false;
            }
        }
        else
        {
            tiempoPerdido -= Time.deltaTime;

            if (tiempoPerdido <= 0f)
            {
                currentDetection = 0f;

                if (persiguiendo)
                {
                    persiguiendo = false;
                    IrAPatrulla();
                }
            }
        }

        // ================= UI =================
        if (stateIcon != null)
        {
            if (persiguiendo && persigueSprite != null)
                stateIcon.sprite = persigueSprite;
            else if (veJugador && visualizaSprite != null)
                stateIcon.sprite = visualizaSprite;
            else if (noVisualizaSprite != null)
                stateIcon.sprite = noVisualizaSprite;
        }

        // ================= MOVIMIENTO =================
        if (persiguiendo)
        {
            agent.SetDestination(player.position);
            GirarSuave(dirToPlayer);
        }
        else
        {
            Patrulla();
        }

        // 🔥 ANIMACIÓN CORREGIDA PARA BUILD
        if (animator != null && agent != null)
        {
            float velocidadActual = agent.velocity.magnitude;

            animator.SetFloat("Speed", velocidadActual);

            bool moving = velocidadActual > 0.1f ||
                          (agent.hasPath && agent.remainingDistance > 0.1f);

            animator.SetBool("Moving", moving);
        }

        // Jumpscare
        if (distancia <= catchDistance)
            TriggerJumpscare();
    }

    void GirarSuave(Vector3 direccion)
    {
        direccion.y = 0;
        if (direccion == Vector3.zero) return;

        Quaternion rot = Quaternion.LookRotation(direccion);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rot,
            Time.deltaTime * velocidadGiro
        );
    }

    void Patrulla()
    {
        if (puntosPatrulla.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            indicePatrulla = (indicePatrulla + 1) % puntosPatrulla.Length;
            agent.SetDestination(puntosPatrulla[indicePatrulla].position);
        }

        Vector3 dir = puntosPatrulla[indicePatrulla].position - transform.position;
        GirarSuave(dir);
    }

    void IrAPatrulla()
    {
        if (puntosPatrulla.Length == 0) return;

        agent.isStopped = false;
        agent.SetDestination(puntosPatrulla[indicePatrulla].position);
    }

    public void Activar()
    {
        if (agent == null || puntosPatrulla.Length == 0) return;

        activo = true;
        indicePatrulla = 0;

        agent.isStopped = false;
        agent.SetDestination(puntosPatrulla[indicePatrulla].position);
    }

    void TriggerJumpscare()
    {
        if (yaActivoJumpscare) return;
        yaActivoJumpscare = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            animator.SetFloat("Speed", 0f);
            animator.SetBool("Moving", false);
        }

        if (jumpscareUI != null)
            jumpscareUI.SetActive(true);

        Invoke(nameof(RestartLevel), 2f);
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}