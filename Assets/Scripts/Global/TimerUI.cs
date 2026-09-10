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

    private void UpdateTimer(float pRemaining)
    {
        if (timerText != null)
        {
            timerText.text = pRemaining.ToString(TIMER_FORMAT) + "s";
        }
    }

    private void HandleTimerExpired()
    {
        if (timerText != null)
        {
            timerText.text = "0.0s";
            timerText.color = Color.red;
        }
    }
}
