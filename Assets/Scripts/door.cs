using UnityEngine;
using System.Collections;
public class Door : MonoBehaviour
{
    public GameObject messageUI; // arrastrar el texto aquí
    private Animator anim;
    public GameObject messageUIError;

    private bool playerNear = false;
    private PlayerScript player;

    void Start()
    {
        anim = GetComponent<Animator>();
        messageUI.SetActive(false);
        messageUIError.SetActive(false);
    }

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (player.hasKey)
            {
                anim.SetTrigger("Open");
                messageUI.SetActive(false);
            }
            else
            {
                StartCoroutine(ShowErrorMessage());
            }
        }
    }

    IEnumerator ShowErrorMessage()
    {
        messageUIError.SetActive(true);
        yield return new WaitForSeconds(5f);
        messageUIError.SetActive(false);
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