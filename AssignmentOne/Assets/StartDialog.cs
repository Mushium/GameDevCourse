using UnityEngine;

public class StartDialog : MonoBehaviour
{
    public Dialog dialog;


    public void OnInteract()
    {
        dialog.gameObject.SetActive(true);
        dialog.StartDialogue();
    }
}
