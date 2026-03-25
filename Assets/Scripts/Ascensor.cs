using UnityEngine;

public class Ascensor : MonoBehaviour
{
    public GameObject messageUI;
    public Animator parkourAnim; // animator del objeto que se mueve
    private bool isUp = false;
    private bool playerNear = false;
    private PlayerScript player;

    void Start()
    {
        messageUI.SetActive(false);
    }

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (!isUp)
            {
                parkourAnim.Play("Subir");
                isUp = true;
            }
            else
            {
                parkourAnim.Play("Bajar");
                isUp = false;
            }
            messageUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            player = other.GetComponent<PlayerScript>();
            messageUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            messageUI.SetActive(false);
        }
    }
}