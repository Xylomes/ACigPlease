
using UnityEngine;
using static PlayerStateMachine;

public class HeadMouvementEffect : MonoBehaviour
{
    [SerializeField] private float oscillationSpeed;
    [SerializeField] private float amplitudeY;
    [SerializeField] private float amplitudeX;
    [SerializeField] private float smoothSpeed;

    private float runningRatio = 1.3f;
    private float defaultRatio = 0.15f;

    private float mouvementTimer;
    private float fadeTimer;

    private float currentAmpY;
    private float currentAmpX;

    private void LateUpdate()
    {
        if (PlayerStateMachine.CurrentState == PlayerState.Walking)
        {
            SetOscillation(oscillationSpeed, amplitudeY, amplitudeX);
        }
        else if (PlayerStateMachine.CurrentState == PlayerState.Running)
        {
            SetOscillation(oscillationSpeed * runningRatio, amplitudeY * runningRatio, amplitudeX * runningRatio);
        }
        else
        {
            SetOscillation(oscillationSpeed * defaultRatio, amplitudeY * defaultRatio, amplitudeX * defaultRatio);
        }
    }

    private void SetOscillation(float pOscillationSpeed, float pAmplitudeY, float pAmplitudeX)
    {
        mouvementTimer += Time.deltaTime * pOscillationSpeed;

        currentAmpY = Mathf.MoveTowards(currentAmpY, pAmplitudeY, Time.deltaTime * smoothSpeed);
        currentAmpX = Mathf.MoveTowards(currentAmpX, pAmplitudeX, Time.deltaTime * smoothSpeed);

        float lY = Mathf.Sin(mouvementTimer) * currentAmpY;
        float lX = Mathf.Cos(mouvementTimer * 0.5f) * currentAmpX;

        transform.localPosition = new Vector3(lX, lY, 0f);

    }
}
