using System.Collections;
using UnityEngine;

public class WordTypingChecker : MonoBehaviour
{
    public string targetWord = "GOD";
    private string currentInput = "";
    private float maxTypingTimePerLetter = 1f; // 1 second for each letter
    private float typingStartTime;
    public GameObject credits;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(10f);
        Destroy(gameObject);
    }

    void Update()
    {
        CheckTyping();
    }

    void CheckTyping()
    {
        if (Input.anyKeyDown)
        {
            currentInput += Input.inputString;

            if (currentInput.Equals(targetWord.ToLower()))
            {
                Credits();
            }
            else
            {
                typingStartTime = Time.time;
            }
        }

        // Reset typing progress if time exceeds the allowed duration for a letter
        if (!string.IsNullOrEmpty(currentInput) && Time.time - typingStartTime > maxTypingTimePerLetter)
        {
            ResetTyping();
        }
    }

    void Credits()
    {
        Instantiate(credits, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    void ResetTyping()
    {
        currentInput = "";
    }
}
