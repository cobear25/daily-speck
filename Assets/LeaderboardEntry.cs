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
        SetText(usernameText, username);
        SetText(totalAttemptsText, totalAttempts.ToString());
        SetText(level0AttemptsText, level0Attempts.ToString());
        SetText(level1AttemptsText, level1Attempts.ToString());
        SetText(level2AttemptsText, level2Attempts.ToString());

        SetJamImage(jam0Image, jam0);
        SetJamImage(jam1Image, jam1);
        SetJamImage(jam2Image, jam2);
    }

    public void SetPlaceholder(string username, string placeholder)
    {
        SetText(usernameText, username);
        SetText(totalAttemptsText, placeholder);
        SetText(level0AttemptsText, placeholder);
        SetText(level1AttemptsText, placeholder);
        SetText(level2AttemptsText, placeholder);

        SetJamImage(jam0Image, false);
        SetJamImage(jam1Image, false);
        SetJamImage(jam2Image, false);
    }

    void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
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
