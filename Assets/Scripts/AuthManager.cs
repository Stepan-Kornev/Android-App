using Firebase.Auth;
using Firebase;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using MySql.Data.MySqlClient;

public class AuthManager : MonoBehaviour
{
    // Firebase
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public static FirebaseAuth auth;
    public FirebaseUser User;

    // ��� �����
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    //��� �����������
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    // ��� �������
    [Header("Profile")]
    public TMP_InputField usernameProfileFiled;
    public TMP_InputField emailProfileFiled;
    public TMP_InputField passwordProfileFiled;


    void Awake()
    {
        // ��������, ��� ��� ����������� Firebase ������������ � �������
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                // ���� �� �������� ��������������
                InitializeFirebase();
                Debug.Log("��������� �������������� Firebase ��������� �������!");
            }
            else
            {
                Debug.LogError("�� ������� ��������� ��� ����������� Firebase: " + dependencyStatus);
            }
        });

        emailProfileFiled.textComponent.overflowMode = TextOverflowModes.Ellipsis;
        passwordProfileFiled.textComponent.overflowMode = TextOverflowModes.Ellipsis;
    }

    private void InitializeFirebase()
    {
        Debug.Log("��������� �������������� Firebase");
        //������������ ������� ���������� ������������
        auth = FirebaseAuth.DefaultInstance;
    }

    // ���������� ������ �����
    public void LoginButton()
    {
        // ������ ������� ����� � �������, ��������� email � ������
        try
        {
            StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
        }
        catch (TimeoutException ex)
        {
            Debug.Log("����-��� �����. ������ ��������, ��������� �� ���������� ��������, ��� ������ �� ��������." + ex);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (MySqlException ex)
        {
            Debug.Log("������ ����� � �������: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (Exception ex)
        {
            Debug.Log("������ ����� � �������: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
    }

    // ���������� ������ �����������
    public void RegisterButton()
    {
        // ������ ������� ����������� � �������, ��������� email, password and username
        try
        {
            StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
        }
        catch (TimeoutException ex)
        {
            Debug.Log("����-��� �����. ������ ��������, ��������� �� ���������� ��������, ��� ������ �� ��������." + ex);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (MySqlException ex)
        {
            Debug.Log("������ ����������� � �������: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (Exception ex)
        {
            Debug.Log("������ ����������� � �������: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
    }

    // ����� �� ��������
    public void Logout()
    {
        try
        {
            auth.SignOut();
            Debug.Log("User signed out successfully!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            UIManager.instance.OpenLoginPanel();
        }
        catch (TimeoutException ex)
        {
            Debug.Log("����-��� �����. ������ ��������, ��������� �� ���������� ��������, ��� ������ �� ��������." + ex);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (MySqlException ex)
        {
            Debug.Log("������ ������ �� �������: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (Exception ex)
        {
            Debug.Log("������ ������ �� �������: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
    }
    
    private IEnumerator Login(string _email, string _password)
    {
        // ������������ ������� ����� � �������
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        // ���� ���� ������� ����� (�� ������� ��� ��� ��������)
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            // ���� �� �������� ������
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "������ �����!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "����������� Email!";
                    break;
                case AuthError.MissingPassword:
                    message = "����������� ������!";
                    break;
                case AuthError.WrongPassword:
                    message = "������������ ������!";
                    break;
                case AuthError.InvalidEmail:
                    message = "������������ Email!";
                    break;
                case AuthError.UserNotFound:
                    message = "������ ������� ������ �� ����������!";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            // ������������ ��������������� � �� ��� ����������� ���������� ������
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "���� ��������!";

            // ��������� ������ � �������
            usernameProfileFiled.text = User.DisplayName;
            emailProfileFiled.text = User.Email;
            passwordProfileFiled.text = passwordLoginField.text;

            // ������� � ���� ���������� ����� �������� �����������
            UIManager.instance.OpenMainPanel();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            warningRegisterText.text = "����������� ��� ������������!";
        }
        else if (_username.Length > 10)
        {
            warningRegisterText.text = "������� ������� ���! �������� 10 ��������.";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            warningRegisterText.text = "������ �� ���������!";
        }
        else
        {
            // ��������� ������� �����������
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            // ���� ���� ������� ����� (�� ������� ��� ��� ��������)
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                // ���� �� �������� ������
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "������ �����������!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "����������� Email!";
                        break;
                    case AuthError.MissingPassword:
                        message = "����������� ������!";
                        break;
                    case AuthError.WeakPassword:
                        message = "������ ������!";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "������ ����������� ����� ��� ������������.";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                // ���� ������ �� ������� ������������ ��������� ����������
                User = RegisterTask.Result;

                if (User != null)
                {
                    // ��������� ������� ������������ � ��� �������� ���
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    // ��������� ������� ������������
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    // ���� ���� ������� ����� (�� ������� ��� ��� ��������)
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        // ���� ���� �����-���� ������
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "�� ������� ���������� ��� ������������!";
                    }
                    else
                    {
                        // ���� ��� ������������ ������� ���������
                        warningRegisterText.text = "�� ������� ����������������!";
                        UIManager.instance.OpenLoginPanel();
                    }
                }
            }
        }
    }
}