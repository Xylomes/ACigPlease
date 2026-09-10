using UnityEngine;
using System;

public class ClockController : MonoBehaviour
{
    [Header("Aiguilles")]
    [SerializeField] private Transform hourHand;
    [SerializeField] private Transform minuteHand;
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;
    [SerializeField] private float updateInterval = 1f;

    private float timeRemaining;


    private float timer;

    void Start()
    {
        UpdateClock();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateClock();
        }
    }

    private void UpdateClock()
    {
        DateTime now = DateTime.Now;

        float hours = now.Hour % 12;
        float minutes = now.Minute;
        float seconds = now.Second;

        float minuteAngle = minutes * 6f; 

        float hourAngle = hours * 30f + 180f;

        if (hourHand != null)
            hourHand.localRotation = Quaternion.Euler(rotationAxis * hourAngle);

        if (minuteHand != null)
            minuteHand.localRotation = Quaternion.Euler(rotationAxis * minuteAngle);
    }
}