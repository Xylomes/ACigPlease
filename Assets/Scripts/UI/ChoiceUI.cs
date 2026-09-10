using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Auto-wires "Fumer" and "Jeter" buttons to the GameFlowController choice handlers.</summary>
public class ChoiceUI : MonoBehaviour
{
    private const string SMOKE_LABEL = "Fumer";
    private const string THROW_AWAY_LABEL = "Jeter";

    private void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label == null) continue;

            if (label.text == SMOKE_LABEL)
            {
                btn.onClick.AddListener(() => GameFlowController.Instance.OnSmokeClicked());
            }
            else if (label.text == THROW_AWAY_LABEL)
            {
                btn.onClick.AddListener(() => GameFlowController.Instance.OnThrowAwayClicked());
            }
        }
    }
}
