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
    }

    void Update()
    {
        // Patrulla básica: si no persigue, se queda quieto o camina aleatorio
        PatrullaAleatoria();

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

            agent.ResetPath();
        }

        // Actualiza el parámetro Speed en el Animator
        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);

        if (Vector3.Distance(transform.position, player.position) <= catchDistance)
            TriggerJumpscare();
    }

    private void PatrullaAleatoria()
    {
        // Aquí puedes poner lógica de patrulla si quieres
        // Por ejemplo: agent.SetDestination(puntoAleatorio.position);
        // Si no, el enemigo se queda quieto hasta detectar al jugador
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
