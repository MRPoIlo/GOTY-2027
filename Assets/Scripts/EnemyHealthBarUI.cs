using UnityEngine;

public class EnemyHealthBarUI : MonoBehaviour
{
    public Transform healthFill;
    public EnemyHealth enemy;

    void Update()
    {
        float percent = enemy.currentHealth / enemy.maxHealth;
        healthFill.localScale = new Vector3(percent, 1f, 1f);
    }
}