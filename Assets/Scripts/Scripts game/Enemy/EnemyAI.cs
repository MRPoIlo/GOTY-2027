using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    [Header("Jugador")]
    public Transform player;

    [Header("Detección")]
    public float visionRange = 5f;
    public float visionAngle = 60f;
    public float detectionTime = 3f;
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

    private NavMeshAgent agent;
    private Animator animator;
    private float currentDetection = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (stateIcon != null && noVisualizaSprite != null)
            stateIcon.sprite = noVisualizaSprite;

        if (jumpscareUI != null)
            jumpscareUI.SetActive(false);

        // 🔹 Iniciar patrulla en el primer punto
        if (puntosPatrulla.Length > 0)
            agent.SetDestination(puntosPatrulla[indicePatrulla].position);
    }

    void Update()
    {
        // Patrulla básica
        Patrulla();

        // Detección del jugador
        Vector3 dirToPlayer = player.position - transform.position;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (dirToPlayer.magnitude < visionRange && angle < visionAngle / 2f)
        {
            currentDetection += Time.deltaTime;

            if (stateIcon != null && visualizaSprite != null)
                stateIcon.sprite = visualizaSprite;

            if (currentDetection >= detectionTime)
            {
                if (stateIcon != null && persigueSprite != null)
                    stateIcon.sprite = persigueSprite;

                agent.SetDestination(player.position);
            }
        }
        else
        {
            currentDetection = 0f;
            if (stateIcon != null && noVisualizaSprite != null)
                stateIcon.sprite = noVisualizaSprite;

            // 🔹 Si no persigue, vuelve a patrullar
            if (puntosPatrulla.Length > 0 && !agent.pathPending && agent.remainingDistance < 0.5f)
            {
                indicePatrulla = (indicePatrulla + 1) % puntosPatrulla.Length;
                agent.SetDestination(puntosPatrulla[indicePatrulla].position);
            }
        }

        // Actualiza animación
        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);

        if (Vector3.Distance(transform.position, player.position) <= catchDistance)
            TriggerJumpscare();
    }

    private void Patrulla()
    {
        if (puntosPatrulla.Length == 0) return;

        // Si no está persiguiendo, sigue patrullando
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            indicePatrulla = (indicePatrulla + 1) % puntosPatrulla.Length;
            agent.SetDestination(puntosPatrulla[indicePatrulla].position);
        }
    }

    void TriggerJumpscare()
    {
        if (jumpscareUI != null)
            jumpscareUI.SetActive(true);

        Invoke("RestartLevel", 2f);
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
