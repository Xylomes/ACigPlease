using UnityEngine;

[System.Serializable] public class DialogueCondition 
{

    public string[] requieredFlags;
    public string[] anyOfFlags;

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
