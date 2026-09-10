using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameFlags
{
    #region Flags

    public const string MARAINTRO1INTERACTIOM = "mara_intro_1_interaction";
    public const string MARAINTRO2INTERACTIOM = "mara_intro_2_interaction";
    public const string MARAINTRO3INTERACTIOM = "mara_intro_3_interaction";
    public const string MARAINTRO4INTERACTIOM = "mara_intro_4_interaction";
    public const string MARAINTRO5INTERACTIOM = "mara_intro_5_interaction";

    public const string GAME_SETUP_DONE = "game_setup_done";
    public const string CIGARETTES_FOUND = "cigarettes_found";
    public const string WORKING_LIGHTER_FOUND = "working_lighter_found";
    public const string GAME_OVER = "game_over";
    public const string GAME_WON = "game_won";
    public const string CHOICE_PROMPT = "choice_prompt";
    public const string SMOKE_CHOSEN = "smoke_chosen";
    public const string THROW_AWAY_CHOSEN = "throw_away_chosen";

    #endregion Flags

    private static Dictionary<string, bool> flagsBoard = new Dictionary<string, bool>();

    public static event Action<string> OnFlagSet;

    public static void SetFlag(string pFlag)
    {
        if (flagsBoard.ContainsKey(pFlag) && flagsBoard[pFlag])
            return;

        flagsBoard[pFlag] = true;
        OnFlagSet?.Invoke(pFlag);
    }

    public static bool IsSetFlag(string pFlag)
    {
        return flagsBoard.ContainsKey(pFlag) && flagsBoard[pFlag];
    }

    public static void ResetAllFlags()
    {
        flagsBoard.Clear();
    }
}
