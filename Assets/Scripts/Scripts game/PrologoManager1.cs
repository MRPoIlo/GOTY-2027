using UnityEngine;

public class PrologoManager : MonoBehaviour
{
    public EnemyAI enemigo;
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
            "Escucho pasos...",
            "No debería estar aquí."
        });

        enemigo.gameObject.SetActive(true);
    }
}
