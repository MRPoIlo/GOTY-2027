using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnemyNivel5 : MonoBehaviour
{
    [Header("Jugador")]
    public Transform player;

    [Header("Detección")]
    public float visionRange = 10f;
    public float visionAngle = 160f;
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

    [Header("NavMesh")]
    public float navmeshWaitTimeout = 5f;

    // ── Estado interno ───────────────────────────────────────────────────────
    private NavMeshAgent agent;
    private Animator animator;
    private int indicePatrulla;
    private float currentDetection;
    private float tiempoPerdido;
    private bool persiguiendo;
    private bool investigando;
    private bool jumpscareActivo;
    private bool inicializado;

    // ── Ciclo de vida ─────────────────────────────────────────────────────────
    void Awake()
    {
        this.enabled = true;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (stateIcon != null && noVisualizaSprite != null)
            stateIcon.sprite = noVisualizaSprite;

        ResetearEstado();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }
    }

    void Start()
    {
        var manager = FindObjectOfType<SalaCocinaManager>();
        if (manager != null)
        {
            manager.OnJugadorListo.AddListener(InicializarEnemigo);
            Debug.Log("[Enemy5] Suscrito a OnJugadorListo");
        }
        else
        {
            Debug.LogWarning("[Enemy5] SalaCocinaManager no encontrado — inicialización directa");
            StartCoroutine(InicializarCoroutine());
        }
    }

    void OnDestroy()
    {
        var manager = FindObjectOfType<SalaCocinaManager>();
        manager?.OnJugadorListo.RemoveListener(InicializarEnemigo);
    }

    // ── Inicialización ────────────────────────────────────────────────────────
    public void InicializarEnemigo()
    {
        if (!gameObject.activeInHierarchy || !enabled)
        {
            Debug.LogWarning("[Enemy5] No se puede inicializar porque el objeto está inactivo");
            return;
        }
        StartCoroutine(InicializarCoroutine());
    }

    IEnumerator InicializarCoroutine()
    {
        Debug.Log("[Enemy5] Inicializando...");
        yield return null; yield return null;

        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (agent == null || animator == null)
        { Debug.LogError("[Enemy5] Faltan NavMeshAgent o Animator"); yield break; }

        // Asegurar que el Animator no aplique root motion si usamos NavMeshAgent
        animator.applyRootMotion = false;

        // Esperar a que el agente esté sobre el NavMesh
        float waited = 0f;
        while (!agent.isOnNavMesh && waited < navmeshWaitTimeout)
        {
            Debug.Log($"[Enemy5] Esperando NavMesh... ({waited:F1}s)");
            waited += 0.25f;
            yield return new WaitForSeconds(0.25f);
        }
        if (!agent.isOnNavMesh)
            Debug.LogWarning("[Enemy5] isOnNavMesh sigue false — continuando de todos modos");

        // Buscar jugador si la referencia se perdió tras recarga
        if (player == null)
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc != null) player = pc.transform;
        }

        ResetearEstado();
        agent.speed = velocidadPatrulla;
        agent.isStopped = false;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        animator.SetFloat("Speed", 0f);
        animator.SetBool("Moving", false);

        if (puntosPatrulla != null && puntosPatrulla.Length > 0)
        {
            indicePatrulla = 0;
            if (agent.isOnNavMesh)
                agent.SetDestination(puntosPatrulla[0].position);
            inicializado = true;
            Debug.Log("[Enemy5] Inicializado — patrullando");
        }
        else
        {
            Debug.LogWarning("[Enemy5] Sin puntos de patrulla");
            inicializado = true;
        }
    }

    // ── Update ────────────────────────────────────────────────────────────────
    void Update()
    {
        if (!inicializado || jumpscareActivo) return;
        if (player == null || agent == null) return;
        if (!agent.isOnNavMesh)
        { animator?.SetFloat("Speed", 0f); animator?.SetBool("Moving", false); return; }

        Vector3 dirToPlayer = player.position - transform.position;
        float distancia = dirToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        bool veJugador = distancia < visionRange && angle < visionAngle * 0.5f;

        // Detección
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
                Debug.Log("[Enemy5] PERSECUCIÓN");
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

        // Movimiento
        if (persiguiendo)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else if (!investigando) Patrulla();

        RotarHaciaMovimiento();

        // 🔥 Animación corregida: actualizar Speed y Moving
        if (animator != null && agent != null)
        {
            float velocidadActual = agent.velocity.magnitude;
            animator.SetFloat("Speed", velocidadActual);

            bool moving = velocidadActual > 0.1f ||
                          (agent.hasPath && agent.remainingDistance > 0.1f);

            animator.SetBool("Moving", moving);
        }

        // Captura
        if (distancia <= catchDistance) TriggerJumpscare();
    }

    // ── Patrulla ──────────────────────────────────────────────────────────────
    void Patrulla()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0) return;
        if (!agent.isOnNavMesh) return;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            indicePatrulla = (indicePatrulla + 1) % puntosPatrulla.Length;
            agent.SetDestination(puntosPatrulla[indicePatrulla].position);
        }
    }

    void IrAPatrulla()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0) return;
        if (!agent.isOnNavMesh) return;
        agent.isStopped = false;
        agent.SetDestination(puntosPatrulla[indicePatrulla].position);
    }

    // ── Investigar ────────────────────────────────────────────────────────────
    public void Investigar(Vector3 punto)
    {
        if (!inicializado || agent == null || !agent.isOnNavMesh) return;
        investigando = true;
        persiguiendo = false;
        agent.isStopped = false;
        agent.speed = velocidadPatrulla;
        agent.SetDestination(punto);
        StartCoroutine(VolverAPatrullaDelay(3f));
    }

    IEnumerator VolverAPatrullaDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!persiguiendo && investigando) { investigando = false; IrAPatrulla(); }
    }

    // ── Jumpscare ─────────────────────────────────────────────────────────────
    void TriggerJumpscare()
    {
        if (jumpscareActivo) return;
        jumpscareActivo = true;
        inicializado = false;
        Debug.Log("[Enemy5] JUMPSCARE ACTIVADO");

        if (agent != null)
        { agent.isStopped = true; agent.velocity = Vector3.zero; agent.ResetPath(); }

        animator?.SetFloat("Speed", 0f);
        animator?.SetBool("Moving", false);

        jumpscareUI?.SetActive(true);
        StartCoroutine(RecargarEscena());
    }

    IEnumerator RecargarEscena()
    {
        yield return new WaitForSecondsRealtime(2f);
        AsyncOperation op = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        op.allowSceneActivation = false;
        while (op.progress < 0.9f) yield return null;
        op.allowSceneActivation = true;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    void ResetearEstado()
    {
        persiguiendo = false;
        investigando = false;
        jumpscareActivo = false;
        inicializado = false;
        currentDetection = 0f;
        tiempoPerdido = 0f;
    }

    void RotarHaciaMovimiento()
    {
        if (agent == null || agent.velocity.sqrMagnitude <= 0.1f) return;
        Vector3 dir = agent.velocity.normalized; dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(
                transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
    }

    void ActualizarIcono(bool veJugador)
    {
        if (stateIcon == null) return;
        if (persiguiendo && persigueSprite != null) stateIcon.sprite = persigueSprite;
        else if (veJugador && visualizaSprite != null) stateIcon.sprite = visualizaSprite;
        else if (noVisualizaSprite != null) stateIcon.sprite = noVisualizaSprite;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Vector3 fwd = transform.forward * visionRange;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, -visionAngle * .5f, 0) * fwd);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, visionAngle * .5f, 0) * fwd);
    }
}