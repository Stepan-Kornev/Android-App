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

    // Для входа
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    //Для регистрации
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    // Для профиля
    [Header("Profile")]
    public TMP_InputField usernameProfileFiled;
    public TMP_InputField emailProfileFiled;
    public TMP_InputField passwordProfileFiled;


    void Awake()
    {
        // Проверка, что все зависимости Firebase присутствуют в системе
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                // Если всё доступно инициализируем
                InitializeFirebase();
                Debug.Log("Настройка аутентификации Firebase завершена успешно!");
            }
            else
            {
                Debug.LogError("Не удалось разрешить все зависимости Firebase: " + dependencyStatus);
            }
        });

        emailProfileFiled.textComponent.overflowMode = TextOverflowModes.Ellipsis;
        passwordProfileFiled.textComponent.overflowMode = TextOverflowModes.Ellipsis;
    }

    private void InitializeFirebase()
    {
        Debug.Log("Настройка аутентификации Firebase");
        //Установление объекта экземпляра аунтефикации
        auth = FirebaseAuth.DefaultInstance;
    }

    // Обработчик кнопки входа
    public void LoginButton()
    {
        // Запуск функции входа в систему, передавая email и пароль
        try
        {
            StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
        }
        catch (TimeoutException ex)
        {
            Debug.Log("Тайм-аут истек. Период ожидания, прошедший до завершения операции, или сервер не отвечает." + ex);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (MySqlException ex)
        {
            Debug.Log("Ошибка входа в систему: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (Exception ex)
        {
            Debug.Log("Ошибка входа в систему: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
    }

    // Обработчик кнопки регистрации
    public void RegisterButton()
    {
        // Запуск функции регистрации в системе, передавая email, password and username
        try
        {
            StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
        }
        catch (TimeoutException ex)
        {
            Debug.Log("Тайм-аут истек. Период ожидания, прошедший до завершения операции, или сервер не отвечает." + ex);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (MySqlException ex)
        {
            Debug.Log("Ошибка регистарции в системе: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (Exception ex)
        {
            Debug.Log("Ошибка регистрации в системе: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
    }

    // Выход из аккаунта
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
            Debug.Log("Тайм-аут истек. Период ожидания, прошедший до завершения операции, или сервер не отвечает." + ex);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (MySqlException ex)
        {
            Debug.Log("Ошибка выхода из системы: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (Exception ex)
        {
            Debug.Log("Ошибка выхода из системы: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
    }
    
    private IEnumerator Login(string _email, string _password)
    {
        // Запускаяется функция входа в систему
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        // Ждем пока получим ответ (не понимаю как это работает)
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            // Если мы получили ошибку
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Ошибка входа!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Отсутствует Email!";
                    break;
                case AuthError.MissingPassword:
                    message = "Отсутствует пароль!";
                    break;
                case AuthError.WrongPassword:
                    message = "Неправильный пароль!";
                    break;
                case AuthError.InvalidEmail:
                    message = "Неправильный Email!";
                    break;
                case AuthError.UserNotFound:
                    message = "Данной учетной записи не существует!";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            // Пользователь зарегистрирован и мы его приравниаем результату работы
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Вход выполнен!";

            // Настройка данных в профиле
            usernameProfileFiled.text = User.DisplayName;
            emailProfileFiled.text = User.Email;
            passwordProfileFiled.text = passwordLoginField.text;

            // Переход в само приложение после успешной авторизации
            UIManager.instance.OpenMainPanel();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            warningRegisterText.text = "Отсутствует имя пользователя!";
        }
        else if (_username.Length > 10)
        {
            warningRegisterText.text = "Слишком длинное имя! Максимум 10 символов.";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            warningRegisterText.text = "Пароли не совпадают!";
        }
        else
        {
            // Запускаем функцию регистрации
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            // Ждем пока получим ответ (не понимаю как это работает)
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                // Если мы получили ошибку
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Ошибка регистрации!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Отсутствует Email!";
                        break;
                    case AuthError.MissingPassword:
                        message = "Отсутствует пароль!";
                        break;
                    case AuthError.WeakPassword:
                        message = "Слабый пароль!";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Данная электронная почта уже используется.";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                // Если ошибок не найдено пользователь равняется результату
                User = RegisterTask.Result;

                if (User != null)
                {
                    // Создается профиль пользователя и ему задается имя
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    // Обновляет профиль пользователя
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    // Ждем пока получим ответ (не понимаю как это работает)
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        // Если есть какие-либо ошибки
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "Не удалось установить имя пользователя!";
                    }
                    else
                    {
                        // Если имя пользователя успешно добавлено
                        warningRegisterText.text = "Вы успешно зарегистрированы!";
                        UIManager.instance.OpenLoginPanel();
                    }
                }
            }
        }
    }
}