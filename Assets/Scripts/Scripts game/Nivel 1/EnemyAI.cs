using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Jugador")]
    public Transform player;

    [Header("Detección")]
    public float visionRange = 8f;
    public float visionAngle = 120f;
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

    [Header("Movimiento")]
    public float velocidadPatrulla = 2f;
    public float velocidadPersecucion = 3.5f;

    private NavMeshAgent agent;
    private Animator animator;
    private int indicePatrulla;
    private float currentDetection;
    private float tiempoPerdido;
    private float tiempoAtascado;

    private bool activo;
    private bool persiguiendo;
    private bool investigando;
    private bool jumpscareActivo;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (agent != null)
        {
            agent.updateRotation = true;
            agent.autoRepath = true;
            agent.autoBraking = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = 50;

            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }

        if (stateIcon != null && noVisualizaSprite != null)
            stateIcon.sprite = noVisualizaSprite;

        ResetearEstado();
        SetBloqueado(true); // arranca bloqueado
    }

    void Start()
    {
        if (player == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) player = pc.transform;
        }
    }

    void Update()
    {
        if (!activo || jumpscareActivo) return;
        if (player == null || agent == null) return;
        if (!agent.isOnNavMesh) { ActualizarAnimacion(0f); return; }

        // Prevención de atascos
        if (!agent.pathPending && agent.velocity.sqrMagnitude < 0.01f)
        {
            tiempoAtascado += Time.deltaTime;
            if (tiempoAtascado > 2f)
            {
                agent.ResetPath();
                if (persiguiendo)
                    agent.SetDestination(player.position);
                else
                    IrAPatrulla();
                tiempoAtascado = 0f;
            }
        }
        else tiempoAtascado = 0f;

        Vector3 dirToPlayer = player.position - transform.position;
        float distancia = dirToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        bool veJugador = distancia < visionRange && angle < visionAngle * 0.5f;

        if (veJugador)
        {
            currentDetection += Time.deltaTime;
            tiempoPerdido = 0.3f;

            if (!persiguiendo && currentDetection >= detectionTime)
            {
                persiguiendo = true;
                investigando = false;
                agent.speed = velocidadPersecucion;
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
                    agent.speed = velocidadPatrulla;
                    IrAPatrulla();
                }
            }
        }

        ActualizarIcono(veJugador);

        if (persiguiendo)
            agent.SetDestination(player.position);
        else if (!investigando)
            Patrulla();

        ActualizarAnimacion(agent.velocity.magnitude);

        if (distancia <= catchDistance)
            TriggerJumpscare();
    }

    void Patrulla()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0 || !agent.isOnNavMesh) return;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            indicePatrulla = (indicePatrulla + 1) % puntosPatrulla.Length;
            agent.SetDestination(puntosPatrulla[indicePatrulla].position);
        }
    }

    void IrAPatrulla()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0 || !agent.isOnNavMesh) return;
        agent.isStopped = false;
        agent.SetDestination(puntosPatrulla[indicePatrulla].position);
    }

    void TriggerJumpscare()
    {
        if (jumpscareActivo) return;
        jumpscareActivo = true;
        activo = false;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        ActualizarAnimacion(0f);
        jumpscareUI?.SetActive(true);
        StartCoroutine(RecargarEscena());
    }

    IEnumerator RecargarEscena()
    {
        yield return new WaitForSecondsRealtime(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ResetearEstado()
    {
        persiguiendo = investigando = jumpscareActivo = false;
        currentDetection = tiempoPerdido = tiempoAtascado = 0f;
    }

    void ActualizarAnimacion(float velocidad)
    {
        if (animator == null) return;
        animator.SetFloat("Speed", velocidad);
        animator.SetBool("Moving", velocidad > 0.1f);
    }

    void ActualizarIcono(bool veJugador)
    {
        if (stateIcon == null) return;
        if (persiguiendo && persigueSprite != null) stateIcon.sprite = persigueSprite;
        else if (veJugador && visualizaSprite != null) stateIcon.sprite = visualizaSprite;
        else if (noVisualizaSprite != null) stateIcon.sprite = noVisualizaSprite;
    }

    // ───── Bloqueo compartido ─────
    public void SetBloqueado(bool bloqueado)
    {
        if (agent == null) return;

        if (bloqueado)
        {
            activo = false;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
            ActualizarAnimacion(0f);
            if (animator != null) animator.Play("Idle");
        }
        else
        {
            activo = true;
            agent.isStopped = false;
            agent.ResetPath();
            agent.speed = velocidadPatrulla;
        }
    }
}
