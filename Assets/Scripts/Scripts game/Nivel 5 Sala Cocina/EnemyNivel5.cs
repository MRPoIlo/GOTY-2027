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
    private int indicePatrulla = 0;

    [Header("Movimiento")]
    public float velocidadPatrulla = 2f;
    public float velocidadPersecucion = 3.5f;

    private NavMeshAgent agent;
    private Animator animator;

    private float currentDetection = 0f;
    private float tiempoPerdido = 0f;

    private bool persiguiendo = false;
    private bool investigando = false;
    private bool jumpscareActivo = false;
    private bool inicializado = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.speed = velocidadPatrulla;
            agent.isStopped = true; // enemigo detenido al inicio
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    void Start()
    {
        SalaCocinaManager manager = FindObjectOfType<SalaCocinaManager>();
        if (manager != null)
        {
            manager.OnJugadorListo.AddListener(InicializarEnemigo);
            Debug.Log("🎯 Enemigo esperando señal de inicio...");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró SalaCocinaManager, inicializando directamente");
            StartCoroutine(InicializarEnemigoCoroutine());
        }
    }

    public void InicializarEnemigo()
    {
        StartCoroutine(InicializarEnemigoCoroutine());
    }

    IEnumerator InicializarEnemigoCoroutine()
    {
        yield return null; // esperar un frame

        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogError("❌ Enemigo no está sobre el NavMesh");
            yield break;
        }

        agent.isStopped = false;
        agent.speed = velocidadPatrulla;

        if (puntosPatrulla != null && puntosPatrulla.Length > 0)
        {
            indicePatrulla = 0;
            agent.SetDestination(puntosPatrulla[0].position);
            inicializado = true;
            Debug.Log("✅ Enemigo inicializado - Patrullando hacia punto 0");
        }
    }

    void Update()
    {
        if (!inicializado || jumpscareActivo) return;
        if (player == null || agent == null || !agent.isOnNavMesh) return;

        // 🔹 Debug de estado del agente
        Debug.Log("Agent isStopped: " + agent.isStopped +
                  " | hasPath: " + agent.hasPath +
                  " | velocity: " + agent.velocity);

        Vector3 dirToPlayer = player.position - transform.position;
        float distancia = dirToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        bool veJugador = distancia < visionRange && angle < visionAngle / 2f;

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
                Debug.Log("🔥 PERSECUCIÓN ACTIVADA");
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
                    Debug.Log("👀 Perdió al jugador");
                }
            }
        }

        ActualizarIconoEstado(veJugador);

        if (persiguiendo)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else if (!investigando)
        {
            Patrulla();
        }

        RotarHaciaMovimiento();

        if (animator != null)
        {
            float velocidadActual = agent.velocity.magnitude;
            animator.SetFloat("Speed", velocidadActual);
            Debug.Log("Animator Speed param: " + velocidadActual);
        }

        if (distancia <= catchDistance)
        {
            TriggerJumpscare();
        }
    }

    void RotarHaciaMovimiento()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 direccion = agent.velocity.normalized;
            direccion.y = 0;
            if (direccion != Vector3.zero)
            {
                Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 5f);
            }
        }
    }

    void ActualizarIconoEstado(bool veJugador)
    {
        if (stateIcon == null) return;
        if (persiguiendo && persigueSprite != null) stateIcon.sprite = persigueSprite;
        else if (veJugador && visualizaSprite != null) stateIcon.sprite = visualizaSprite;
        else if (noVisualizaSprite != null) stateIcon.sprite = noVisualizaSprite;
    }

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

    public void Investigar(Vector3 punto)
    {
        if (!inicializado || !agent.isOnNavMesh) return;

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
        if (!persiguiendo && investigando)
        {
            investigando = false;
            IrAPatrulla();
        }
    }

    void TriggerJumpscare()
    {
        if (jumpscareActivo) return;
        jumpscareActivo = true;
        inicializado = false;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }

        if (jumpscareUI != null)
        {
            jumpscareUI.SetActive(true);
        }

        this.enabled = false;
        StartCoroutine(RecargarEscena());
    }

    IEnumerator RecargarEscena()
    {
        yield return new WaitForSecondsRealtime(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        SalaCocinaManager manager = FindObjectOfType<SalaCocinaManager>();
        if (manager != null)
        {
            manager.OnJugadorListo.RemoveListener(InicializarEnemigo);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Vector3 forward = transform.forward * visionRange;
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2f, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2f, 0) * forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
