using UnityEngine;

public class HealthBarUI : MonoBehaviour
{
    public Transform healthFill; 
    public PlayerScript player;

    void Update()
    {
        float healthPercent = player.currentHealth / player.maxHealth;

        healthFill.localScale = new Vector3(healthPercent, 1f, 1f);
    }
}