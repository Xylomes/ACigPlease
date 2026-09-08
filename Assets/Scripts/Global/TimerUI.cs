using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    private const string TIMER_FORMAT = "F1";

    private void OnEnable()
    {
        GameManager.OnTimerTick += UpdateTimer;
        GameManager.OnTimerExpired += HandleTimerExpired;
    }

    private void OnDisable()
    {
        GameManager.OnTimerTick -= UpdateTimer;
        GameManager.OnTimerExpired -= HandleTimerExpired;
    }

    /// <summary>Update the timer text with the remaining seconds.</summary>
    private void UpdateTimer(float remaining)
    {
        if (timerText != null)
        {
            timerText.text = remaining.ToString(TIMER_FORMAT) + "s";
        }
    }

    /// <summary>Called when the timer reaches zero.</summary>
    private void HandleTimerExpired()
    {
        if (timerText != null)
        {
            timerText.text = "0.0s";
            timerText.color = Color.red;
        }
    }
}
