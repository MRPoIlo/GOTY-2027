using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public GameManager gameManager;
    public float maxHealth = 100f;
    public float currentHealth;

    public GameObject healthBarUI;
    public float showDistance = 6f;

    private Transform player;

    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= showDistance)
        {
            healthBarUI.SetActive(true);
        }
        else
        {
            healthBarUI.SetActive(false);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        Debug.Log("Vida enemigo: " + currentHealth);

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        gameManager.ShowWin();
        Destroy(gameObject);
    }
}