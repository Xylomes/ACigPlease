using System.Collections.Generic;
using UnityEngine;

public static class GameFlags
{
    #region Flags

    //MARA
    public const string MARAINTRO1INTERACTIOM = "mara_intro_1_interaction";
    public const string MARAINTRO2INTERACTIOM = "mara_intro_2_interaction";
    public const string MARAINTRO3INTERACTIOM = "mara_intro_3_interaction";
    public const string MARAINTRO4INTERACTIOM = "mara_intro_4_interaction";
    public const string MARAINTRO5INTERACTIOM = "mara_intro_5_interaction";

    //DOOR
    //LETTER
    //ACT

    #endregion Flags

    private static Dictionary<string, bool> flagsBoard = new Dictionary<string, bool>();
    public static void SetFlag(string pFlag)
    {
        flagsBoard[pFlag] = true;
    }
    public static bool IsSetFlag(string pFlag)
    {
        if (flagsBoard.ContainsKey(pFlag) && (flagsBoard[pFlag] == true))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
