using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject questPanel;
    public TMP_Text questionText;
    public Button optionA;       
    public Button optionB;       
    public Button optionC;       
    public Button closeButton;   

    [Header("Quest Data")]
    [TextArea(2, 5)] 
    public string question = "Скільки планет у Сонячній системі?";
    public string answerA = "7";
    public string answerB = "8";
    public string answerC = "9";
    [Range(1, 3)] 
    public int correctAnswerIndex = 2;

    [Header("Target Object")]
    public GameObject doorOrBridge;

    private bool playerInside = false;

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseMenu);

        if (optionA != null) optionA.onClick.AddListener(() => CheckAnswer(1));
        if (optionB != null) optionB.onClick.AddListener(() => CheckAnswer(2));
        if (optionC != null) optionC.onClick.AddListener(() => CheckAnswer(3));

        if (questPanel != null)
            questPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            OpenMenu();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            CloseMenu();
        }
    }

    void OpenMenu()
    {
        questionText.text = question;
        optionA.GetComponentInChildren<TMP_Text>().text = answerA;
        optionB.GetComponentInChildren<TMP_Text>().text = answerB;
        optionC.GetComponentInChildren<TMP_Text>().text = answerC;

        questPanel.SetActive(true);
    }

    public void CloseMenu()
    {
        if (questPanel != null)
            questPanel.SetActive(false);
    }

    void CheckAnswer(int chosenOption)
    {
        if (chosenOption == correctAnswerIndex)
        {
            Debug.Log("Правильно!");
            
            if (doorOrBridge != null)
            {
                doorOrBridge.SetActive(false);
            }

            CloseMenu();
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Неправильна відповідь! Спробуй ще раз.");
        }
    }
}