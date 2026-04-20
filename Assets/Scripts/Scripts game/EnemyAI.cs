using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.SceneManagement; // 👈 para reiniciar la escena

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float visionRange = 5f;
    public float visionAngle = 60f;
    public float detectionTime = 3f;
    public float catchDistance = 1.5f; // 👈 distancia mínima para jumpscare

    public Image stateIcon;
    public Sprite noVisualizaSprite;
    public Sprite visualizaSprite;
    public Sprite persigueSprite;

    public GameObject jumpscareUI; // 👈 asigna un Canvas o imagen de jumpscare

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
            jumpscareUI.SetActive(false); // oculto al inicio
    }

    void Update()
    {
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

            agent.ResetPath();
        }

        // 👇 Actualiza animaciones con la velocidad del agente
        if (animator != null)
        {
            float velocidad = agent.velocity.magnitude;
            animator.SetFloat("Speed", velocidad);
        }

        // 👇 Si alcanza al jugador, dispara jumpscare
        if (Vector3.Distance(transform.position, player.position) <= catchDistance)
        {
            TriggerJumpscare();
        }
    }

    void TriggerJumpscare()
    {
        if (jumpscareUI != null)
            jumpscareUI.SetActive(true);

        // reinicia el nivel después de 2 segundos
        Invoke("RestartLevel", 2f);
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
