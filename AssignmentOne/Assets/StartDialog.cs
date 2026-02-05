using UnityEngine;

public class StartDialog : MonoBehaviour
{
    public Dialog Dialog;


    public void OnInteract()
    {
        Dialog.gameObject.SetActive(true);
        Dialog.StartDialogue();
    }
}
