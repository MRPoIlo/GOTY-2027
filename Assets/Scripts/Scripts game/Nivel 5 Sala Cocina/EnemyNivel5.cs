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
    public float visionRange    = 10f;
    public float visionAngle    = 160f;
    public float detectionTime  = 2f;
    public float catchDistance  = 1.5f;

    [Header("UI Estado")]
    public Image  stateIcon;
    public Sprite noVisualizaSprite;
    public Sprite visualizaSprite;
    public Sprite persigueSprite;

    [Header("Jumpscare")]
    public GameObject jumpscareUI;

    [Header("Patrulla")]
    [SerializeField] private Transform[] puntosPatrulla;

    [Header("Movimiento")]
    public float velocidadPatrulla    = 2f;
    public float velocidadPersecucion = 3.5f;

    [Header("NavMesh")]
    public float navmeshWaitTimeout = 5f;

    private NavMeshAgent agent;
    private Animator     animator;
    private int   indicePatrulla;
    private float currentDetection;
    private float tiempoPerdido;
    private bool  persiguiendo;
    private bool  investigando;
    private bool  jumpscareActivo;
    private bool  inicializado;

    void Awake()
    {
        agent    = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (agent != null)
        {
            agent.updateRotation = true; // fix clave — sincroniza collider con movimiento
            agent.isStopped      = true;
            agent.velocity       = Vector3.zero;
            agent.ResetPath();
        }

        if (stateIcon != null && noVisualizaSprite != null)
            stateIcon.sprite = noVisualizaSprite;

        ResetearEstado();
    }

    void Start()
    {
        var manager = FindObjectOfType<SalaCocinaManager>();
        if (manager != null)
            manager.OnJugadorListo.AddListener(InicializarEnemigo);
        else
            StartCoroutine(InicializarCoroutine());
    }

    void OnDestroy()
    {
        var manager = FindObjectOfType<SalaCocinaManager>();
        manager?.OnJugadorListo.RemoveListener(InicializarEnemigo);
    }

    public void InicializarEnemigo()
    {
        if (!gameObject.activeInHierarchy || !enabled) return;
        StartCoroutine(InicializarCoroutine());
    }

    IEnumerator InicializarCoroutine()
    {
        yield return null;
        yield return null;

        if (agent    == null) agent    = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (agent == null || animator == null) { Debug.LogError("[Enemy5] Faltan componentes"); yield break; }

        animator.applyRootMotion = false;

        float waited = 0f;
        while (!agent.isOnNavMesh && waited < navmeshWaitTimeout)
        {
            waited += 0.25f;
            yield return new WaitForSeconds(0.25f);
        }

        if (player == null)
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc != null) player = pc.transform;
        }

        ResetearEstado();
        agent.speed     = velocidadPatrulla;
        agent.isStopped = false;
        agent.velocity  = Vector3.zero;
        agent.ResetPath();
        ActualizarAnimacion(0f);

        if (puntosPatrulla != null && puntosPatrulla.Length > 0)
        {
            indicePatrulla = 0;
            if (agent.isOnNavMesh)
                agent.SetDestination(puntosPatrulla[0].position);
        }

        inicializado = true;
        Debug.Log("[Enemy5] Inicializado");
    }

    void Update()
    {
        if (!inicializado || jumpscareActivo) return;
        if (player == null || agent == null)  return;
        if (!agent.isOnNavMesh) { ActualizarAnimacion(0f); return; }

        Vector3 dirToPlayer = player.position - transform.position;
        float   distancia   = dirToPlayer.magnitude;
        float   angle       = Vector3.Angle(transform.forward, dirToPlayer);
        bool    veJugador   = distancia < visionRange && angle < visionAngle * 0.5f;

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

        ActualizarIcono(veJugador);

        if (persiguiendo)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
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

    void TriggerJumpscare()
    {
        if (jumpscareActivo) return;
        jumpscareActivo = true;
        inicializado    = false;

        if (agent != null) { agent.isStopped = true; agent.velocity = Vector3.zero; agent.ResetPath(); }

        ActualizarAnimacion(0f);
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

    void ResetearEstado()
    {
        persiguiendo = investigando = jumpscareActivo = inicializado = false;
        currentDetection = tiempoPerdido = 0f;
    }

    void ActualizarAnimacion(float velocidad)
    {
        if (animator == null) return;
        animator.SetFloat("Speed", velocidad);
    }

    void ActualizarIcono(bool veJugador)
    {
        if (stateIcon == null) return;
        if (persiguiendo && persigueSprite    != null) stateIcon.sprite = persigueSprite;
        else if (veJugador && visualizaSprite != null) stateIcon.sprite = visualizaSprite;
        else if (noVisualizaSprite            != null) stateIcon.sprite = noVisualizaSprite;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Vector3 fwd = transform.forward * visionRange;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, -visionAngle * .5f, 0) * fwd);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0,  visionAngle * .5f, 0) * fwd);
    }
}