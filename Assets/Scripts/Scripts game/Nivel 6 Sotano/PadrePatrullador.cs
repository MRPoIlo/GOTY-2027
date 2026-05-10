using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PadrePatrullador : MonoBehaviour
{
    [Header("Jugador")]
    public Transform player;

    [Header("Detección")]
    public float visionRange    = 8f;
    public float visionAngle    = 110f;
    public float detectionTime  = 1.5f;
    public float catchDistance  = 1.5f;

    [Header("Capas que bloquean visión")]
    [SerializeField] private LayerMask capasObstaculos;

    [Header("Detección por linterna")]
    [SerializeField] private float radioExtraLinterna = 5f;
    [SerializeField] private Light luzLinterna;

    [Header("Jumpscare / Game Over")]
    public GameObject jumpscareUI;

    [Header("Patrulla")]
    [SerializeField] private Transform[] puntosPatrulla;

    [Header("Movimiento")]
    public float velocidadPatrulla    = 2f;
    public float velocidadPersecucion = 3.5f;

    [Header("NavMesh")]
    public float navmeshWaitTimeout = 5f;

    // ── Privados ──────────────────────────────────────────────────────────────
    private NavMeshAgent agent;
    private Animator     animator;
    private int   indicePatrulla;
    private float currentDetection;
    private float tiempoPerdido;
    private float tiempoAtascado;
    private bool  persiguiendo;
    private bool  investigando;
    private bool  jumpscareActivo;
    private bool  inicializado;

    // ══════════════════════════════════════════════════════════════════════════
    void Awake()
    {
        agent    = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (agent != null)
        {
            agent.updateRotation          = true;
            agent.autoRepath              = true;
            agent.autoBraking             = true;
            agent.obstacleAvoidanceType   = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority       = 50;
            agent.isStopped               = true;
            agent.velocity                = Vector3.zero;
            agent.ResetPath();
        }

        ResetearEstado();
        SetBloqueado(true);
    }

    void Start()
    {
        if (player == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) player = pc.transform;
        }

        // Inicializar después de un frame para que el NavMesh esté listo
        StartCoroutine(InicializarCoroutine());
    }

    IEnumerator InicializarCoroutine()
    {
        yield return null;
        yield return null;

        float waited = 0f;
        while (!agent.isOnNavMesh && waited < navmeshWaitTimeout)
        {
            waited += 0.25f;
            yield return new WaitForSeconds(0.25f);
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("[PadrePatrullador] No está en NavMesh — continuando");
        }

        agent.speed = velocidadPatrulla;

        if (puntosPatrulla != null && puntosPatrulla.Length > 0)
            agent.SetDestination(puntosPatrulla[0].position);

        SetBloqueado(false);
        Debug.Log("[PadrePatrullador] Inicializado");
    }

    // ══════════════════════════════════════════════════════════════════════════
    void Update()
    {
        if (!inicializado || jumpscareActivo) return;
        if (player == null || agent == null)  return;
        if (!agent.isOnNavMesh) { ActualizarAnimacion(0f); return; }

        // Anti-atasco
        if (!agent.pathPending && agent.velocity.sqrMagnitude < 0.01f)
        {
            tiempoAtascado += Time.deltaTime;
            if (tiempoAtascado > 2f)
            {
                agent.ResetPath();
                if (persiguiendo) agent.SetDestination(player.position);
                else              IrAPatrulla();
                tiempoAtascado = 0f;
            }
        }
        else tiempoAtascado = 0f;

        // ── Detección ─────────────────────────────────────────────────────────
        Vector3 dirToPlayer = player.position - transform.position;
        float   distancia   = dirToPlayer.magnitude;
        float   angle       = Vector3.Angle(transform.forward, dirToPlayer);

        // Radio aumenta si la linterna está encendida
        float radio = visionRange;
        if (luzLinterna != null && luzLinterna.enabled)
            radio += radioExtraLinterna;

        bool veJugador = distancia < radio && angle < visionAngle * 0.5f;

        // Raycast — si hay pared o caja en medio no lo ve
        if (veJugador)
        {
            Vector3 origen  = transform.position + Vector3.up * 1.5f;
            Vector3 destino = player.position    + Vector3.up * 1f;
            float   dist    = Vector3.Distance(origen, destino);

            if (Physics.Raycast(origen, (destino - origen).normalized, dist, capasObstaculos))
            {
                veJugador = false;
                Debug.DrawLine(origen, destino, Color.green);
            }
            else
            {
                Debug.DrawLine(origen, destino, Color.red);
            }
        }

        if (veJugador)
        {
            currentDetection += Time.deltaTime;
            tiempoPerdido     = 0.3f;

            if (!persiguiendo && currentDetection >= detectionTime)
            {
                persiguiendo    = true;
                investigando    = false;
                agent.speed     = velocidadPersecucion;
                agent.isStopped = false;
                NarracionManager.Instance?.Narrar("Me vio.");
                Debug.Log("[PadrePatrullador] PERSECUCIÓN");
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
                    agent.speed  = velocidadPatrulla;
                    IrAPatrulla();
                }
            }
        }

        // ── Movimiento ────────────────────────────────────────────────────────
        if (persiguiendo)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else if (!investigando)
            Patrulla();

        ActualizarAnimacion(agent.velocity.magnitude);

        // ── Captura ───────────────────────────────────────────────────────────
        if (distancia <= catchDistance)
            TriggerJumpscare();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // PATRULLA
    // ══════════════════════════════════════════════════════════════════════════

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

    public void Investigar(Vector3 punto)
    {
        if (!inicializado || agent == null || !agent.isOnNavMesh) return;
        investigando    = true;
        persiguiendo    = false;
        agent.isStopped = false;
        agent.speed     = velocidadPatrulla;
        agent.SetDestination(punto);
        StartCoroutine(VolverAPatrullaDelay(3f));
    }

    IEnumerator VolverAPatrullaDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!persiguiendo && investigando) { investigando = false; IrAPatrulla(); }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // JUMPSCARE / GAME OVER
    // ══════════════════════════════════════════════════════════════════════════

    void TriggerJumpscare()
    {
        if (jumpscareActivo) return;
        jumpscareActivo = true;
        inicializado    = false;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity  = Vector3.zero;
            agent.ResetPath();
        }

        ActualizarAnimacion(0f);

        // Mostrar pantalla de Game Over igual que Nivel 5
        jumpscareUI?.SetActive(true);

        GameOverManager go = FindFirstObjectByType<GameOverManager>();
        if (go != null)
            go.ActivarGameOver();
        else
            StartCoroutine(RecargarEscena());
    }

    IEnumerator RecargarEscena()
    {
        yield return new WaitForSecondsRealtime(2f);
        AsyncOperation op = SceneManager.LoadSceneAsync(
            SceneManager.GetActiveScene().name);
        op.allowSceneActivation = false;
        while (op.progress < 0.9f) yield return null;
        op.allowSceneActivation = true;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // API PÚBLICA
    // ══════════════════════════════════════════════════════════════════════════

    public void SetBloqueado(bool bloqueado)
    {
        if (agent == null) return;

        if (bloqueado)
        {
            inicializado    = false;
            agent.isStopped = true;
            agent.velocity  = Vector3.zero;
            agent.ResetPath();
            ActualizarAnimacion(0f);
            if (animator != null) animator.Play("Idle");
        }
        else
        {
            inicializado    = true;
            agent.isStopped = false;
            agent.ResetPath();
        }
    }

    public void Detener()
    {
        SetBloqueado(true);
        jumpscareActivo = true;
    }

    public void Reiniciar(Vector3 posicion)
    {
        jumpscareActivo = false;
        ResetearEstado();
        agent.Warp(posicion);
        SetBloqueado(false);
        if (puntosPatrulla != null && puntosPatrulla.Length > 0)
            agent.SetDestination(puntosPatrulla[0].position);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════════

    void ResetearEstado()
    {
        persiguiendo = investigando = jumpscareActivo = inicializado = false;
        currentDetection = tiempoPerdido = tiempoAtascado = 0f;
    }

    void ActualizarAnimacion(float velocidad)
    {
        if (animator == null) return;
        animator.SetFloat("Speed", velocidad);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, visionRange + radioExtraLinterna);
        Vector3 fwd = transform.forward * visionRange;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position,
            transform.position + Quaternion.Euler(0, -visionAngle * .5f, 0) * fwd);
        Gizmos.DrawLine(transform.position,
            transform.position + Quaternion.Euler(0,  visionAngle * .5f, 0) * fwd);
    }
}