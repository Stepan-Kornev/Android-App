using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.Mime;
using System;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Mozilla;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public static GameObject isActiveGameObj;
    public static GameObject isActiveMessagePanel;

    [Header("Login and register panels")]
    public GameObject loginUI;
    public GameObject registerUI;

    [Header("Main panel")]
    public GameObject mainUI;
    public GameObject forumUI;
    public GameObject gameUI;
    public GameObject calendarUI;
    public GameObject profileUI;
    public GameObject confirmLogoutUI;
    public GameObject infAboutReloginUI;
    public GameObject changeUsernameUI;
    public GameObject changeEmailUI;
    public GameObject changePasswordUI;
    public GameObject sendMessageUI;
    public GameObject openProfileBtn;
    public GameObject fullMessagePanelUI;
    public GameObject blockBtn;
    public GameObject blockBtnTwo;
    public GameObject errorSendReplyPanelUI;
    public GameObject restartForumBtn;
    public GameObject restartRepliesBtn;
    public GameObject restartNotesBtn;
    public GameObject ErrorInternetConnectionPanel;

    [Header("Login panel elements")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    [Header("Register panel elements")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    [Header("Profile panel elements")]
    public GameObject editAndSaveBtn;
    public TMP_Text editAndSaveBtnText;
    public GameObject editBtns;
    public TMP_InputField inputEmailField;
    public TMP_InputField inputPasswordField;
    public GameObject showPasswordBtn;
    public Sprite eyeCloseSprite;
    public Sprite eyeOpenSprite;
    public Sprite startEditBtn;
    public Sprite endEditBtn;

    [Header("SendMessage panel elements")]
    public TMP_Dropdown topicDropdown;
    public TMP_InputField inputMessageField;

    [Header("Send reply panel elements")]
    public TMP_Text errorSendReplyPanelText;

    [Header("Note panel elements")]
    public GameObject createNotePanel;
    public GameObject fullNoteInfPanel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void ShowFullStringInField(TMP_InputField _field)
    {
        _field.textComponent.overflowMode = TextOverflowModes.Overflow;
        if (editBtns.activeSelf)
        {
            editBtns.SetActive(false);
            showPasswordBtn.SetActive(true);

            editAndSaveBtn.GetComponent<Image>().sprite = endEditBtn;
            editAndSaveBtnText.text = "Редактировать";
        }
        if (_field == inputPasswordField)
        {
            showPasswordBtn.GetComponent<Image>().sprite = eyeCloseSprite;
            showPasswordBtn.SetActive(false);
        }
    }

    public void HideFullStringField(TMP_InputField _field)
    {
        _field.textComponent.overflowMode = TextOverflowModes.Ellipsis;
        if (_field == inputPasswordField)
        {
            showPasswordBtn.SetActive(true);

            inputPasswordField.contentType = TMP_InputField.ContentType.Password;
            inputPasswordField.textComponent.SetText(inputPasswordField.text);
        }
    }

    public void OpenLoginPanel()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
        warningLoginText.text = "";
        confirmLoginText.text = "";

        loginUI.SetActive(true);
        registerUI.SetActive(false);
    }
    public void OpenRegisterPanel()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
        warningRegisterText.text = "";

        loginUI.SetActive(false);
        registerUI.SetActive(true);
    }

    public void OpenMainPanel()
    {
        loginUI.SetActive(false);
        mainUI.SetActive(true);
        isActiveGameObj = forumUI;
    }

    public void OpenForumPanel()
    {
        if (!openProfileBtn.activeSelf)
            openProfileBtn.SetActive(true);

        isActiveGameObj.SetActive(false);
        forumUI.SetActive(true);
        isActiveGameObj = forumUI;
    }

    public void OpenGamePanel()
    {
        if (!openProfileBtn.activeSelf)
            openProfileBtn.SetActive(true);

        isActiveGameObj.SetActive(false);
        gameUI.SetActive(true);
        isActiveGameObj = gameUI;
    }

    public void OpenNotePanel()
    {
        if (!openProfileBtn.activeSelf)
            openProfileBtn.SetActive(true);

        isActiveGameObj.SetActive(false);
        calendarUI.SetActive(true);
        isActiveGameObj = calendarUI;

        try
        {
            StartCoroutine(NoteManager.instance.LoadNotesInUnity());
        }
        catch (TimeoutException ex)
        {
            Debug.Log("Тайм-аут истек. Период ожидания, прошедший до завершения операции, или сервер не отвечает." + ex);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (MySqlException ex)
        {
            Debug.Log("Ошибка подключения к базе данных" + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (Exception ex)
        {
            Debug.Log("Ошибка подключения к базе данных: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
    }

    public void OpenProfilePanel()
    {
        isActiveGameObj.SetActive(false);
        profileUI.SetActive(true);
        isActiveGameObj = profileUI;
        openProfileBtn.SetActive(false);
    }

    public void OpenConfirmLogoutPanel()
    {
        confirmLogoutUI.SetActive(true);
        isActiveMessagePanel = confirmLogoutUI;
        blockBtn.SetActive(true);
    }

    public void OpenInfAboutReloginPanel()
    {
        infAboutReloginUI.SetActive(true);
        isActiveMessagePanel = infAboutReloginUI;
        blockBtn.SetActive(true);
    }

    public void OpenChangeUsernamePanel()
    {
        changeUsernameUI.SetActive(true);
        isActiveMessagePanel = changeUsernameUI;
        blockBtn.SetActive(true);
    }

    public void OpenChangeEmailPanel()
    {
        changeEmailUI.SetActive(true);
        isActiveMessagePanel = changeEmailUI;
        blockBtn.SetActive(true);
    }

    public void OpenChangePaswwordPanel()
    {
        changePasswordUI.SetActive(true);
        isActiveMessagePanel = changePasswordUI;
        blockBtn.SetActive(true);
    }

    public void OpenSendMessagePanel()
    {
        topicDropdown.value = 0;
        inputMessageField.text = "";

        sendMessageUI.SetActive(true);
        isActiveMessagePanel = sendMessageUI;
        blockBtn.SetActive(true);
    }

    public void OpenFullMessagePanel(GameObject question)
    {   
        fullMessagePanelUI.SetActive(true);
        isActiveGameObj = fullMessagePanelUI;
        forumUI.SetActive(false);

        string questionId = question.transform.Find("QuestionId-text").GetComponent<TMP_Text>().text;

        try
        {
            StartCoroutine(ForumManager.instance.LoadFullQuestionInfInUnity(questionId));
        }
        catch (TimeoutException ex)
        {
            Debug.Log("Тайм-аут истек. Период ожидания, прошедший до завершения операции, или сервер не отвечает." + ex);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (MySqlException ex)
        {
            Debug.Log("Ошибка подключения к базе данных" + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (Exception ex)
        {
            Debug.Log("Ошибка подключения к базе данных: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
    }

    public void OpenErrorSendReplyPanel()
    {
        errorSendReplyPanelUI.SetActive(true);
        isActiveMessagePanel = errorSendReplyPanelUI;
        blockBtnTwo.SetActive(true);
    }

    public void OpenFullNotePanel(GameObject note)
    {
        fullNoteInfPanel.SetActive(true);
        isActiveMessagePanel = fullNoteInfPanel;
        blockBtn.SetActive(true);

        string noteId = note.transform.Find("NoteId-text").GetComponent<TMP_Text>().text;
        try
        {
            StartCoroutine(NoteManager.instance.LoadFullNoteInfInUnity(noteId));
        }
        catch (TimeoutException ex)
        {
            Debug.Log("Тайм-аут истек. Период ожидания, прошедший до завершения операции, или сервер не отвечает." + ex);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (MySqlException ex)
        {
            Debug.Log("Ошибка подключения к базе данных" + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (Exception ex)
        {
            Debug.Log("Ошибка подключения к базе данных: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
    }

    public void OpenCreateNotePanel()
    {
        createNotePanel.SetActive(true);
        isActiveMessagePanel = createNotePanel;
        blockBtn.SetActive(true);
    }

    public void OpenFullGameInfPanel(GameObject fullGameInfPanel)
    {
        fullGameInfPanel.SetActive(true);
        isActiveMessagePanel = fullGameInfPanel;
        blockBtn.SetActive(true);
    }

    public void HideRestartBtns()
    {
        restartForumBtn.SetActive(false);
        restartRepliesBtn.SetActive(false);
        restartNotesBtn.SetActive(false);
    }

    public void Cancel()
    {
        isActiveMessagePanel.SetActive(false);
        blockBtn.SetActive(false);
        blockBtnTwo.SetActive(false);
    }

    public void EditButtonClick()
    {
        if (editBtns.activeSelf)
        {
            editBtns.SetActive(false);
            showPasswordBtn.SetActive(true);

            editAndSaveBtn.GetComponent<Image>().sprite = startEditBtn;
            editAndSaveBtnText.text = "Редактировать";
        }
        else
        {
            showPasswordBtn.GetComponent<Image>().sprite = eyeCloseSprite;
            inputPasswordField.contentType = TMP_InputField.ContentType.Password;
            inputPasswordField.textComponent.SetText(inputPasswordField.text);

            showPasswordBtn.SetActive(false);
            editBtns.SetActive(true);

            editAndSaveBtn.GetComponent<Image>().sprite = endEditBtn;
            editAndSaveBtnText.text = "Готово";
        }
    }

    public void ShowPassword()
    {
        if (showPasswordBtn.GetComponent<Image>().sprite == eyeCloseSprite)
        {
            showPasswordBtn.GetComponent<Image>().sprite = eyeOpenSprite;
            inputPasswordField.contentType = TMP_InputField.ContentType.Standard;
            inputPasswordField.textComponent.SetText(inputPasswordField.text);
        }
        else if (showPasswordBtn.GetComponent<Image>().sprite == eyeOpenSprite)
        {
            showPasswordBtn.GetComponent <Image>().sprite = eyeCloseSprite;
            inputPasswordField.contentType = TMP_InputField.ContentType.Password;
            inputPasswordField.textComponent.SetText(inputPasswordField.text);
        }
    }
}