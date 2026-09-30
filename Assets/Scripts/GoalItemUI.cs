using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private GameObject checkMark;

    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }

    public void SetRemaining(int remaining)
    {
        bool completed = remaining <= 0;

        countText.text = remaining.ToString();
        countText.gameObject.SetActive(!completed);
        checkMark.SetActive(completed);
    }
}
