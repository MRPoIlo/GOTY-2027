using UnityEngine;

public class Nivel1Manager : MonoBehaviour
{
    public EnemyAI enemigo; // arrastra el enemigo desde la escena
    private int interaccionesCompletadas = 0;

    public void RegistrarInteraccion()
    {
        interaccionesCompletadas++;

        if (interaccionesCompletadas >= 3)
        {
            ActivarEnemigo();
        }
    }

    private void ActivarEnemigo()
    {
        NarracionManager.Instance.Narrar(new string[] {
            "Escucho pasos acercándose...",
            "No debería estar aquí."
        });

        enemigo.gameObject.SetActive(true); // activa al enemigo
    }
}
