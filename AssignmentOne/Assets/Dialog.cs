using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    
    public string Cname;

    [TextArea(3, 10)]
    public string[] diaSen;
    
    public Text nameText;
    public Text dialogueText;
    public Image nextIndicator; // Assign in the Inspector


    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isDialogue = false;
    private string currentSentence = "";
    private Queue<string> sentences = new Queue<string>();
    
    public Button confirmButton;
    public Button cancelButton;




    void Start()
    {
        isDialogue = false;
        nextIndicator.gameObject.SetActive(false); 
        gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isDialogue) return;
        if (sentences.Count == 0)
        {
            confirmButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(true);

            if (ValueSingleton.Instance.coin >= 20)
            {
                confirmButton.interactable = true;
            }
            else
            {
                confirmButton.interactable = false;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue()
    {
        if (isDialogue) return;
        isDialogue = true;
        nameText.text = Cname;

        sentences.Clear();

        foreach (string sentence in diaSen)
        {
            sentences.Enqueue(sentence);
        }

        nextIndicator.gameObject.SetActive(false);
        gameObject.SetActive(true);
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            // Skip animation and display full sentence
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentSentence;
            isTyping = false;
            nextIndicator.gameObject.SetActive(true);
            return;
        }

        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentSentence = sentences.Dequeue();
        typingCoroutine = StartCoroutine(TypeSentence(currentSentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        nextIndicator.gameObject.SetActive(false);

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(0.02f);
        }

        isTyping = false;
        nextIndicator.gameObject.SetActive(true);
    }

    void EndDialogue()
    {
        isDialogue = false;
        nextIndicator.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void CancelDialogue()
    {
        DisplayNextSentence();
    }

    public void LevelUpgrade()
    {
        DisplayNextSentence();
        ValueSingleton.Instance.coin-=20;
        ValueSingleton.Instance.level++;
    }

    public void HealthUpgrade()
    {
        DisplayNextSentence();
        ValueSingleton.Instance.coin-=20;
        ValueSingleton.Instance.AddHealth();
    }
}