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

    [Header("NavMesh Fix")]
    public float navmeshWaitTimeout = 5f;
    private float reintentoIntervalo = 0.25f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.speed = velocidadPatrulla;
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            animator.SetFloat("Speed", 0f);
        }

        if (stateIcon != null && noVisualizaSprite != null)
            stateIcon.sprite = noVisualizaSprite;
    }

    void Start()
    {
        StartCoroutine(InicializarEnemigoCoroutine());
    }

    IEnumerator InicializarEnemigoCoroutine()
    {
        yield return null;
        yield return null;

        float waited = 0f;
        while (!agent.isOnNavMesh && waited < navmeshWaitTimeout)
        {
            waited += reintentoIntervalo;
            yield return new WaitForSeconds(reintentoIntervalo);
        }

        agent.isStopped = false;
        agent.speed = velocidadPatrulla;
        agent.ResetPath();

        if (player == null)
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc != null) player = pc.transform;
        }

        if (puntosPatrulla.Length > 0)
        {
            indicePatrulla = 0;
            agent.SetDestination(puntosPatrulla[0].position);
        }

        inicializado = true;
    }

    void Update()
    {
        if (!inicializado || jumpscareActivo) return;
        if (player == null || !agent.isOnNavMesh) return;

        Vector3 dirToPlayer = player.position - transform.position;
        float distancia = dirToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        bool veJugador = distancia < visionRange && angle < visionAngle / 2f;

        // DETECCIÓN
        if (veJugador)
        {
            currentDetection += Time.deltaTime;
            tiempoPerdido = 0.3f;

            if (!persiguiendo && currentDetection >= detectionTime)
            {
                persiguiendo = true;
                investigando = false;
                agent.speed = velocidadPersecucion;
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

        ActualizarIconoEstado(veJugador);

        // MOVIMIENTO
        if (persiguiendo)
        {
            agent.SetDestination(player.position);
        }
        else if (!investigando)
        {
            Patrulla();
        }

        RotarHaciaMovimiento();

        // 🔥 ANIMACIÓN (FIX BUILD)
        float velocidadActual = agent.velocity.magnitude;
        if (animator != null)
            animator.SetFloat("Speed", velocidadActual);

        // CAPTURA
        if (distancia <= catchDistance)
            TriggerJumpscare();
    }

    void RotarHaciaMovimiento()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 dir = agent.velocity.normalized;
            dir.y = 0;

            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
        }
    }

    void ActualizarIconoEstado(bool veJugador)
    {
        if (stateIcon == null) return;

        if (persiguiendo && persigueSprite != null)
            stateIcon.sprite = persigueSprite;
        else if (veJugador && visualizaSprite != null)
            stateIcon.sprite = visualizaSprite;
        else if (noVisualizaSprite != null)
            stateIcon.sprite = noVisualizaSprite;
    }

    void Patrulla()
    {
        if (puntosPatrulla.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            indicePatrulla = (indicePatrulla + 1) % puntosPatrulla.Length;
            agent.SetDestination(puntosPatrulla[indicePatrulla].position);
        }
    }

    void IrAPatrulla()
    {
        if (puntosPatrulla.Length == 0) return;

        agent.SetDestination(puntosPatrulla[indicePatrulla].position);
    }

    public void Investigar(Vector3 punto)
    {
        if (!inicializado) return;

        investigando = true;
        persiguiendo = false;

        agent.speed = velocidadPatrulla;
        agent.SetDestination(punto);

        StartCoroutine(VolverAPatrulla());
    }

    IEnumerator VolverAPatrulla()
    {
        yield return new WaitForSeconds(3f);

        investigando = false;
        IrAPatrulla();
    }

    void TriggerJumpscare()
    {
        if (jumpscareActivo) return;

        jumpscareActivo = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (animator != null)
            animator.SetFloat("Speed", 0f);

        if (jumpscareUI != null)
            jumpscareUI.SetActive(true);

        StartCoroutine(Reiniciar());
    }

    IEnumerator Reiniciar()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}