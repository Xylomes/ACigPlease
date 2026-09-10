using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;
using TMPro;
using System.Globalization;
using System;
using UnityEngine.InputSystem;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private Button signBTN;
    [SerializeField] private TextMeshProUGUI textObj;
    [SerializeField] private CanvasGroup skipCanvasGroup;
    [SerializeField] private float skipDuration = 4f;

    private PlayerInput playerInput;
    private InputAction skipIntro;
    private Coroutine skipFillCoroutine;

    private float typingTime = 0.03f;
    private string introTextReplaced;
    private const string INTROTXT =
    "BAILER MUSTARD INDUSTRY\n" +
    "Portail Employ\u00e9 v4.2.1 \u2014 Acc\u00e8s s\u00e9curis\u00e9\n" +
    "Connexion chiffr\u00e9e \u2014 Canal interne uniquement\n\n" +

    "Identification en cours...\n\n" +

    "Utilisateur d\u00e9tect\u00e9 : {USERNAME}\n" +
    "Machine : {MACHINE}\n" +
    "Date : {DATE}\n" +
    "Heure locale : {LOCAL_TIME}\n" +
    "Localisation : {LOCATION}\n\n" +

    "V\u00e9rification des ant\u00e9c\u00e9dents...\n" +
    "> Casier judiciaire : VIERGE\n" +
    "> Personnes \u00e0 charge : 1 (dossier m\u00e9dical li\u00e9)\n" +
    "> Signalements actifs : AUCUN\n" +
    "> Contacts recens\u00e9s : {FRIEND_COUNT}\n" +
    "> Derni\u00e8re interaction sociale significative : >48h\n" +
    "> Indice d'isolement : CONFORME\n\n" +

    "Analyse comportementale...\n" +
    "> Profil psychoacoustique : OPTIMAL\n" +
    "> Sensibilit\u00e9 perceptive : 94e percentile\n" +
    "> Capacit\u00e9 d'adaptation morale : SUPERIEURE\n" +
    "> Empathie r\u00e9siduelle : dans les marges acceptables\n" +
    "> R\u00e9sistance au stress sensoriel : VALID\u00c9E\n" +
    "> Seuil de tol\u00e9rance : NON ATTEINT\n\n" +

    "Bienvenue, {USERNAME}.\n\n" +

    "Votre candidature a \u00e9t\u00e9 retenue parmi 3 dossiers compatibles.\n" +
    "Votre contrat prend effet imm\u00e9diatement.\n" +
    "Dur\u00e9e : ind\u00e9termin\u00e9e.\n" +
    "Clause de rupture : voir article 12-B.\n\n" +

    "Chargement du dossier employ\u00e9...\n\n" +

    "BAILER MUSTARD INDUSTRY \u2014 Fiche entreprise\n" +
    "> Fond\u00e9e en 1917\n" +
    "> Secteur : agroalimentaire / condiments industriels\n" +
    "> Classification produit : YPR-T400\n" +
    "> Mati\u00e8re premi\u00e8re : source locale, approvisionnement r\u00e9gulier\n" +
    "> Temp\u00e9rature de processus : 14\u00b0C \u2014 218\u00b0C\n" +
    "> Taux de rendement organique : 97.3%\n" +
    "> Politique z\u00e9ro d\u00e9chet : valorisation int\u00e9grale\n" +
    "> Partenaire logistique : transport sp\u00e9cial, v\u00e9hicules b\u00e2ch\u00e9s\n" +
    "> CONSIGNE : ne jamais go\u00fbter le produit\n\n" +

    "Programme d'aide familiale...\n" +
    "> B\u00e9n\u00e9ficiaire associ\u00e9 : Jennifer\n" +
    "> Statut : PRISE EN CHARGE\n" +
    "> Suivi m\u00e9dical : actif\n" +
    "> Prochaine correspondance pr\u00e9vue : J+2\n" +
    "> Rappel : toute tentative de contact direct est inutile\n\n" +

    "Note interne :\n" +
    "\"Le processus ne n\u00e9cessite aucune comp\u00e9tence particuli\u00e8re.\n" +
    "La pr\u00e9cision s'acquiert avec la r\u00e9p\u00e9tition.\n" +

    "D\u00e9lai de traitement moyen par unit\u00e9 : 4h12\n" +
    "Cadence recommand\u00e9e : 3 unit\u00e9s / rotation\n" +
    "Votre formateur :  Karl (secteur B-7)\n\n" +

    "Aucune absence tol\u00e9r\u00e9e.\n" +
    "Aucune question n\u00e9cessaire.\n" +
    "Aucune question recommand\u00e9e.\n\n" +

    "{USERNAME} correspond \u00e0 tous les crit\u00e8res de s\u00e9lection.\n" +
    "Nous sommes heureux que vous ayez accept\u00e9.\n\n" +

    "Lancement de l'environnement de travail\u2026";

    private const string GAME_SCENE_NAME = "Game";
    private const string INTRO_SCENE_NAME = "Intro";
    void Start()
    {
        signBTN.gameObject.SetActive(false);
        StartCoroutine(WaitUntilDataReady());

        playerInput = GetComponent<PlayerInput>();
        skipIntro = playerInput.actions["SkipIntro"];
        skipIntro.started += OnSkipStarted;
        skipIntro.performed += OnSkipPerformed;
        skipIntro.canceled += OnSkipCanceled;
        skipCanvasGroup.alpha = 0f;
        skipCanvasGroup.gameObject.SetActive(false);
    }
    private IEnumerator WaitUntilDataReady()
    {
        yield return new WaitUntil(() => DataCollector.Instance.IsReady);
        injectData(INTROTXT);
        StartCoroutine(TypeText(introTextReplaced));
    }

    private string injectData(string pText)
    {
        introTextReplaced = pText.Replace("{USERNAME}", DataCollector.Instance.UtilisatorName);
        introTextReplaced = introTextReplaced.Replace("{MACHINE}", DataCollector.Instance.PcName);
        introTextReplaced = introTextReplaced.Replace("{DATE}", DataCollector.Instance.Date); 
        introTextReplaced = introTextReplaced.Replace("{LOCAL_TIME}", DataCollector.Instance.Hour);
        introTextReplaced = introTextReplaced.Replace("{LOCATION}", $"{DataCollector.Instance.City}, {DataCollector.Instance.Country}");
        introTextReplaced = introTextReplaced.Replace("{FRIEND_COUNT}", DataCollector.Instance.FriendCount);
        return introTextReplaced;
    }
    private IEnumerator TypeText(string pTextIntro)
    {
        textObj.text = "";
        textObj.text = pTextIntro;
        textObj.ForceMeshUpdate();

        int lCharacterAmount = textObj.textInfo.characterCount;

        for(int i = 20; i < lCharacterAmount; i++)
        {
            textObj.text = pTextIntro.Substring(0, i + 1);
            yield return new WaitForSeconds(typingTime);
        }

        signBTN.gameObject.SetActive(true);
    }
    public void OnSignClicked()
    {
        SceneLoader.Instance.StartCoroutine(SceneLoader.Instance.AddScene(GAME_SCENE_NAME));
        SceneLoader.Instance.StartCoroutine(SceneLoader.Instance.UnloadScene(INTRO_SCENE_NAME));
    }

    private void OnSkipStarted(InputAction.CallbackContext pContext)
    {
        skipCanvasGroup.gameObject.SetActive(true);
        skipFillCoroutine = StartCoroutine(FillSkipProgress());
    }
    private void OnSkipPerformed(InputAction.CallbackContext pContext)
    {
        SkipIntroAsked();
    }
    private void OnSkipCanceled(InputAction.CallbackContext pContext)
    {
        StopCoroutine(skipFillCoroutine);
        skipCanvasGroup.alpha = 0f;
        skipCanvasGroup.gameObject.SetActive(false);
    }
    private IEnumerator FillSkipProgress()
    {
        float lTimeElapsed = 0f;

        while (lTimeElapsed < skipDuration)
        {
            lTimeElapsed += Time.deltaTime;
            skipCanvasGroup.alpha = lTimeElapsed / skipDuration;
            yield return null;
        }
    }

    private void SkipIntroAsked()
    {
        StopAllCoroutines();
        OnSignClicked();
    }

    private void OnDestroy()
    {
        skipIntro.started -= OnSkipStarted;
        skipIntro.performed -= OnSkipPerformed;
        skipIntro.canceled -= OnSkipCanceled;
    }
}
