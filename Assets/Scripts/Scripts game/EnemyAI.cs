using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float visionRange = 10f;
    public float visionAngle = 60f;
    public float detectionTime = 3f;

    public Image stateIcon;
    public Sprite noVisualizaSprite;
    public Sprite visualizaSprite;
    public Sprite persigueSprite;

    private NavMeshAgent agent;
    private Animator animator;   // 👈 referencia al Animator
    private float currentDetection = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>(); // 👈 busca el Animator en el hijo (modelo)

        if (stateIcon != null && noVisualizaSprite != null)
        {
            stateIcon.sprite = noVisualizaSprite;
        }
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

                // 👇 activa animación de caminar
                if (animator != null)
                    animator.SetBool("isWalking", true);
            }
        }
        else
        {
            currentDetection = 0f;
            if (stateIcon != null && noVisualizaSprite != null)
                stateIcon.sprite = noVisualizaSprite;

            // 👇 vuelve a Idle
            if (animator != null)
                animator.SetBool("isWalking", false);
        }
    }
}
