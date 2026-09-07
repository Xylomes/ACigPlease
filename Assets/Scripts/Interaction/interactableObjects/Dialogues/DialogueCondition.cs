using UnityEngine;

[System.Serializable] public class DialogueCondition 
{

    public string[] requieredFlags; //AND Logic 
    public string[] anyOfFlags; //OR Logic

    public bool IsConditionMet()
    {
        if (requieredFlags != null)
        {
            foreach (var lFlagR in requieredFlags)
            {

                if (GameFlags.IsSetFlag(lFlagR) == false)
                {
                    return false;
                }

            }
        }
        
        if (anyOfFlags == null || anyOfFlags.Length <= 0)
        {
            return true;
        }
        else
        {
            foreach (var lFlagA in anyOfFlags)
            {
                if (GameFlags.IsSetFlag(lFlagA) == true)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
