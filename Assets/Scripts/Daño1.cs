using UnityEngine;

public class Daño1 : MonoBehaviour
{
    public float damagePercent = 0.2f; // 20% de vida

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();

            if (player != null)
            {
                player.TakeDamage(damagePercent);
            }
        }
    }
}