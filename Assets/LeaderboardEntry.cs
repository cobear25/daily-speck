using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardEntry : MonoBehaviour
{
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI totalAttemptsText;
    public TextMeshProUGUI level0AttemptsText;
    public TextMeshProUGUI level1AttemptsText;
    public TextMeshProUGUI level2AttemptsText;
    public Image jam0Image;
    public Image jam1Image;
    public Image jam2Image;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetEntry(string username, int totalAttempts, int level0Attempts, int level1Attempts, int level2Attempts, bool jam0, bool jam1, bool jam2)
    {
        if (usernameText != null)
        {
            usernameText.text = username;
        }
        if (totalAttemptsText != null)
        {
            totalAttemptsText.text = totalAttempts.ToString();
        }
        if (level0AttemptsText != null)
        {
            level0AttemptsText.text = level0Attempts.ToString();
        }
        if (level1AttemptsText != null)
        {
            level1AttemptsText.text = level1Attempts.ToString();
        }
        if (level2AttemptsText != null)
        {
            level2AttemptsText.text = level2Attempts.ToString();
        }

        SetJamImage(jam0Image, jam0);
        SetJamImage(jam1Image, jam1);
        SetJamImage(jam2Image, jam2);
    }

    void SetJamImage(Image image, bool isFound)
    {
        if (image == null)
        {
            return;
        }

        Color color = Color.white;
        color.a = isFound ? 1f : 0.25f;
        image.color = color;
    }
}
