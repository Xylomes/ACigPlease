using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceUI : MonoBehaviour
{
    private const string SMOKE_LABEL = "Fumer";
    private const string THROW_AWAY_LABEL = "Jeter";

    private void Start()
    {
        Button[] lButtons = GetComponentsInChildren<Button>(true);
        foreach (Button lBtn in lButtons)
        {
            TMP_Text lLabel = lBtn.GetComponentInChildren<TMP_Text>();
            if (lLabel == null) continue;

            if (lLabel.text == SMOKE_LABEL)
            {
                lBtn.onClick.AddListener(() => GameFlowController.Instance.OnSmokeClicked());
            }
            else if (lLabel.text == THROW_AWAY_LABEL)
            {
                lBtn.onClick.AddListener(() => GameFlowController.Instance.OnThrowAwayClicked());
            }
        }
    }
}
