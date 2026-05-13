using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyRoute : MonoBehaviour
{
    [Header("Ruta del enemigo")]
    [SerializeField] private Transform puntoA; // primer punto de la ruta
    [SerializeField] private Transform puntoB; // segundo punto de la ruta
    [SerializeField] private float velocidad = 2f;

    private Transform objetivoActual;

    private void Start()
    {
        objetivoActual = puntoB;
    }

    private void Update()
    {
        // 🔹 Mover al enemigo hacia el objetivo
        transform.position = Vector3.MoveTowards(transform.position, objetivoActual.position, velocidad * Time.deltaTime);

        // 🔹 Cambiar de objetivo cuando llega
        if (Vector3.Distance(transform.position, objetivoActual.position) < 0.1f)
        {
            objetivoActual = (objetivoActual == puntoA) ? puntoB : puntoA;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 🔹 Si toca al jugador, mandar al Game Over
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("GameOver"); // asegúrate que tu escena se llame "GameOver"
        }
    }
}
