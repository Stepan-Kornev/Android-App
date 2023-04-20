using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using System.Collections;
using System.Security.Cryptography;
using Unity.VisualScripting;
using System;
using MySql.Data.MySqlClient;

public class UIChangeUserData : MonoBehaviour
{
    public FirebaseUser User;

    private bool saveNameBtnClickIsSuccessful = false;
    private bool saveEmailBtnClickIsSuccessful = false;
    private bool savePasswordBtnCkickSuccessful = false;

    [Header("Change username panel")]
    public TMP_InputField newNameField;
    public TMP_Text usernameWarningText;
    private string usernameWarningTextMessage;
    public TMP_InputField inputNameField;

    [Header("Change email panel")]
    public TMP_InputField newEmailField;
    public TMP_Text emailWarningText;
    private string emailWarningTextMessage;
    public TMP_InputField inputEmailField;

    [Header("Change password panel")]
    public TMP_InputField newPasswordField;
    public TMP_InputField newPasswordVerifyField;
    public TMP_Text passwordWarningText;
    private string passwordWarningTextMessage;
    public TMP_InputField inputPasswordField;


    private void Update()
    {
        usernameWarningText.text = usernameWarningTextMessage;
        emailWarningText.text = emailWarningTextMessage;
        passwordWarningText.text = passwordWarningTextMessage;

        if (User != null)
        {
            inputNameField.text = User.DisplayName;
        }

        if (saveNameBtnClickIsSuccessful) // ���� ��������� ����� ��������� �������
        {
            newNameField.text = "";
            usernameWarningTextMessage = "";

            UIManager.instance.Cancel();

            saveNameBtnClickIsSuccessful = false;
        }

        if (saveEmailBtnClickIsSuccessful) // ���� ��������� email ��������� �������
        {
            newEmailField.text = "";
            emailWarningTextMessage = "";

            UIManager.instance.Cancel();
            UIManager.instance.OpenInfAboutReloginPanel();

            saveEmailBtnClickIsSuccessful = false;
        }

        if (savePasswordBtnCkickSuccessful)
        {
            newPasswordField.text = "";
            newPasswordVerifyField.text = "";

            UIManager.instance.Cancel();
            UIManager.instance.OpenInfAboutReloginPanel();

            savePasswordBtnCkickSuccessful = false;
        }
    }

    public void SaveNewName()
    {
        try
        {
            StartCoroutine(SaveNewNameIEnumerator(newNameField.text));
        }
        catch (TimeoutException ex)
        {
            Debug.Log("����-��� �����. ������ ��������, ��������� �� ���������� ��������, ��� ������ �� ��������." + ex);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (MySqlException ex)
        {
            Debug.Log("������ ����� �����: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (Exception ex)
        {
            Debug.Log("������ ������ �����: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
    }

    public void SaveNewEmail()
    {
        try
        {
            StartCoroutine(SaveNewEmailIEnumerator(newEmailField.text));
        }
        catch (TimeoutException ex)
        {
            Debug.Log("����-��� �����. ������ ��������, ��������� �� ���������� ��������, ��� ������ �� ��������." + ex);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (MySqlException ex)
        {
            Debug.Log("������ ����� e-mail: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (Exception ex)
        {
            Debug.Log("������ ����� e-mail: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
    }

    public void SaveNewPassword()
    {
        try
        {
            StartCoroutine(SaveNewPasswordIEnumerator(newPasswordField.text, newPasswordVerifyField.text));
        }
        catch (TimeoutException ex)
        {
            Debug.Log("����-��� �����. ������ ��������, ��������� �� ���������� ��������, ��� ������ �� ��������." + ex);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (MySqlException ex)
        {
            Debug.Log("������ ����� ������: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (Exception ex)
        {
            Debug.Log("������ ����� ������: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
    }

    private IEnumerator SaveNewNameIEnumerator(string _newName)
    {
        if (_newName == "")
        {
            usernameWarningTextMessage = "�� �� ����� ���!";
        }
        else if (_newName.Length > 10)
        {
            usernameWarningTextMessage = "������� ������� ���! �������� 10 ��������.";
        }
        else
        {
            User = AuthManager.auth.CurrentUser;

            if (User != null)
            {
                Debug.Log($"Now username = {User.DisplayName}");

                UserProfile profile = new UserProfile { DisplayName = _newName };

                var updateUsernameTask = User.UpdateUserProfileAsync(profile);

                yield return new WaitUntil(predicate: () => updateUsernameTask.IsCompleted);

                if (updateUsernameTask.Exception != null)
                {
                    // ���� ���� �����-���� ������
                    Debug.LogWarning(message: $"Failed to register task with {updateUsernameTask.Exception}");
                    FirebaseException firebaseEx = updateUsernameTask.Exception.GetBaseException() as FirebaseException;
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                    usernameWarningTextMessage = "�� ������� ���������� ��� ������������!";
                }
                else
                {
                    // ���� ��� ������������ ������� ���������
                    Debug.Log($"User profile updated successfully. New username = {User.DisplayName}");

                    saveNameBtnClickIsSuccessful = true;
                }
            }
        }
    }

    private IEnumerator SaveNewEmailIEnumerator(string _newEmail)
    {
        User = AuthManager.auth.CurrentUser;

        if (User.Email == newEmailField.text)
        {
            emailWarningTextMessage = "�� ��� ����������� ���� email!";
        }
        else
        {
            Debug.Log($"Now username email = {User.Email}");

            var updateEmailTask = User.UpdateEmailAsync(_newEmail);

            yield return new WaitUntil(predicate: () => updateEmailTask.IsCompleted);

            if (updateEmailTask.Exception != null)
            {
                // ���� �� �������� ������
                Debug.LogWarning(message: $"Failed to update email task with {updateEmailTask.Exception}");
                FirebaseException firebaseEx = updateEmailTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "��������� ������!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "����������� Email!";
                        break;
                    case AuthError.InvalidEmail:
                        message = "������������ Email!";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email ��� ������������!";
                        break;
                }
                emailWarningTextMessage = message;
            }
            else
            {
                Debug.Log($"User email updated successfully. New email = {User.Email}");

                saveEmailBtnClickIsSuccessful = true;
            }
        }
    }

    private IEnumerator SaveNewPasswordIEnumerator(string _password, string _verifyPassword)
    {
        if (_password != _verifyPassword)
        {
            passwordWarningTextMessage = "������ �� ���������!";
        }
        else
        {
            User = AuthManager.auth.CurrentUser;

            var updatePasswordTask = User.UpdatePasswordAsync(_password);

            yield return new WaitUntil(predicate: () => updatePasswordTask.IsCompleted);

            if (updatePasswordTask.Exception != null)
            {
                // ���� �� �������� ������
                Debug.LogWarning(message: $"Failed to register task with {updatePasswordTask.Exception}");
                FirebaseException firebaseEx = updatePasswordTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "������ ����� ������!";
                switch (errorCode)
                {
                    case AuthError.MissingPassword:
                        message = "����������� ������!";
                        break;
                    case AuthError.WeakPassword:
                        message = "������ ������!";
                        break;
                }
                passwordWarningTextMessage = message;
            }
            else
            {
                Debug.Log($"User passsword updated successfully.");

                savePasswordBtnCkickSuccessful = true;
            }
        }
    }
}
