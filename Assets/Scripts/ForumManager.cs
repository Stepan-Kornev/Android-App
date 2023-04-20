using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using Org.BouncyCastle.Asn1.Mozilla;
using Mysqlx.Crud;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Data;

public class ForumManager : MonoBehaviour
{
    public static ForumManager instance;
    private MessageController messageController;

    private MySqlCommand command;
    private MySqlDataReader reader;
    public List<GameObject> questions = new List<GameObject>();
    public List<GameObject> replies = new List<GameObject>();
    private bool isLoading = false;
    private int countRecords = 0;
    private int countUploadedRecords = 0;

    private Vector2 lastQuestionParentPos;
    private Vector2 startFullQuestionInfParent;

    [Header("Forum Panel")]
    public GameObject questionPrefab;
    public RectTransform questionParent;
    public float threshold = 1f;

    public RectTransform fullQuestionInfParent;
    public GameObject fullQuestionInfPanel;
    public GameObject inputMessagePanel;
    public GameObject ReplyPrefab;
    public TMP_Text topicText;
    public TMP_Text questionIdText;

    [Header("Send Question Panel")]
    public TMP_Dropdown titleDropdown;
    public TMP_InputField questionField;
    public TMP_Text errorSendQuestionPanelText;

    [Header("Send Reply Panel")]
    public TMP_InputField replyText;
    

    // Test
    public string questionParentlocalPositionY;
    public string questionParentRectHeightThreshold;

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

    private void Start()
    {
        startFullQuestionInfParent = fullQuestionInfParent.anchoredPosition;
        startFullQuestionInfParent.y = 0;

        messageController = gameObject.GetComponent<MessageController>();
        try
        {
            StartCoroutine(LoadQuestionsInUnity("SELECT * FROM questions ORDER BY id ASC LIMIT 30"));
        }
        catch (TimeoutException ex)
        {
            Debug.Log("Тайм-аут истек. Период ожидания, прошедший до завершения операции, или сервер не отвечает." + ex);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (MySqlException ex)
        {
            Debug.Log("Ошибка подключения к базе данных: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (Exception ex)
        {
            Debug.Log("Ошибка подключения к базе данных: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
    }

    private void Update()
    {
        if (!isLoading && countRecords > 0 && questionParent.localPosition.y > (questionParent.rect.height * threshold) && questions.Count != 0)
        {
            lastQuestionParentPos = questionParent.anchoredPosition;

            int lastQuestionId = Convert.ToInt32(questions[questions.Count - 1].transform.Find("QuestionId-text").GetComponent<TMP_Text>().text);
            try
            {
                StartCoroutine(LoadQuestionsInUnity("SELECT * FROM questions WHERE id > " + lastQuestionId + " ORDER BY id ASC LIMIT 30"));
            }
            catch (TimeoutException ex)
            {
                Debug.Log("Тайм-аут истек. Период ожидания, прошедший до завершения операции, или сервер не отвечает." + ex);
                InternetConnectionChecker.instance.StartChecking();
            }
            catch (MySqlException ex)
            {
                Debug.Log("Ошибка подключения к базе данных: " + ex.Message);
                InternetConnectionChecker.instance.StartChecking();
            }
            catch (Exception ex)
            {
                Debug.Log("Ошибка подключения к базе данных: " + ex.Message);
                InternetConnectionChecker.instance.StartChecking();
            }
        }

        // Test
        questionParentlocalPositionY = questionParent.localPosition.y.ToString();
        questionParentRectHeightThreshold = (questionParent.rect.height * threshold).ToString();
    }

    private IEnumerator LoadQuestionsInUnity(string query)
    {
        isLoading = true;

        using (DBManager.connection)
        {
            if (DBManager.connection.State == ConnectionState.Open)
                DBManager.connection.Close();

            DBManager.connection.Open();

            command = new MySqlCommand(query, DBManager.connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                GameObject question = Instantiate(questionPrefab, questionParent);

                TMP_Text idText = question.transform.Find("QuestionId-text").GetComponent<TMP_Text>();
                idText.text = reader.GetString(0);
                TMP_Text topicText = question.transform.Find("Topic-text").GetComponent<TMP_Text>();
                topicText.text = reader.GetString(1);
                TMP_Text questionText = question.transform.Find("Question-text").GetComponent<TMP_Text>();
                questionText.text = reader.GetString(2);
                questionText.overflowMode = TextOverflowModes.Ellipsis;
                TMP_Text usernameText = question.transform.Find("Username-text").GetComponent<TMP_Text>();
                usernameText.text = reader.GetString(3);
                TMP_Text dateText = question.transform.Find("Date-text").GetComponent<TMP_Text>();
                dateText.text = reader.GetString(5).Split()[0];

                // Добавление метода открытия полной информации о панели по нажатию на кнопку
                Button button = question.transform.Find("OpenQuestion-button").GetComponent<Button>();
                button.onClick.AddListener(() => UIManager.instance.OpenFullMessagePanel(question));

                questions.Add(question);
                countUploadedRecords++;
                
                ResizeQuestionParent(questionParent, questions);

                yield return null;
            }
            reader.Close();

            int lastQuestionId = 0;
            if (questions.Count > 0)
                lastQuestionId = Convert.ToInt32(questions[questions.Count - 1].transform.Find("QuestionId-text").GetComponent<TMP_Text>().text);
            command = new MySqlCommand("SELECT COUNT(*) FROM questions WHERE id > " + lastQuestionId, DBManager.connection);
            countRecords = Convert.ToInt32(command.ExecuteScalar());
            if (countRecords > 30)
                countRecords = 30;
            Debug.Log("Сообщений выгружено: " + countUploadedRecords + ", доcтупно: " + countRecords);
            countUploadedRecords = 0;

            DBManager.connection.Close();
        }
        isLoading = false;

        questionParent.anchoredPosition = lastQuestionParentPos;
    }

    public IEnumerator LoadFullQuestionInfInUnity(string questionId)
    {
        replies.Add(fullQuestionInfPanel);
        replies.Add(inputMessagePanel);

        using (DBManager.connection)
        {
            DBManager.connection.Open();

            command = new MySqlCommand("SELECT * FROM questions WHERE id = " + questionId, DBManager.connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                topicText.text = reader.GetString(1);

                TMP_Text question_id = fullQuestionInfPanel.transform.Find("QuestionId-text").GetComponent<TMP_Text>();
                question_id.text = questionId;
                TMP_Text questionText = fullQuestionInfPanel.transform.Find("Question-text").GetComponent<TMP_Text>();
                questionText.text = reader.GetString(2);
                TMP_Text usernameText = fullQuestionInfPanel.transform.Find("Username-text").GetComponent<TMP_Text>();
                usernameText.text = reader.GetString(3);
                TMP_Text dateText = fullQuestionInfPanel.transform.Find("Date-text").GetComponent<TMP_Text>();
                dateText.text = reader.GetString(5);

                yield return null;
            }
            reader.Close();

            command = new MySqlCommand("SELECT * FROM replies WHERE question_id = " + questionId, DBManager.connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                GameObject reply = Instantiate(ReplyPrefab, fullQuestionInfParent);

                TMP_Text reply_id = reply.transform.Find("ReplyId-text").GetComponent<TMP_Text>();
                reply_id.text = reader.GetString(0);
                TMP_Text questionText = reply.transform.Find("Question-text").GetComponent<TMP_Text>();
                questionText.text = reader.GetString(2);
                TMP_Text usernameText = reply.transform.Find("Username-text").GetComponent<TMP_Text>();
                usernameText.text = reader.GetString(3);
                TMP_Text dateText = reply.transform.Find("Date-text").GetComponent<TMP_Text>();
                dateText.text = reader.GetString(5);

                replies.Add(reply);

                ResizeQuestionParent(fullQuestionInfParent, replies);

                yield return null;
            }
            reader.Close();

            DBManager.connection.Close();
        }
    }

    public void RestartForum()
    {
        for (int i = 0; i < questions.Count; i++)
            Destroy(questions[i]);
        
        questions.Clear();

        try
        {
            StartCoroutine(LoadQuestionsInUnity("SELECT * FROM questions ORDER BY id ASC LIMIT 30"));
        }
        catch (TimeoutException ex)
        {
            Debug.Log("Тайм-аут истек. Период ожидания, прошедший до завершения операции, или сервер не отвечает." + ex);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (MySqlException ex)
        {
            Debug.Log("Ошибка подключения к базе данных: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (Exception ex)
        {
            Debug.Log("Ошибка подключения к базе данных: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }

        UIManager.instance.restartForumBtn.SetActive(false);
    }

    public void RestartReplies()
    {
        for (int i = 2; i < replies.Count; i++)
            Destroy(replies[i]);

        replies.Clear();

        try
        {
            StartCoroutine(LoadFullQuestionInfInUnity(questionIdText.text));
        }
        catch (TimeoutException ex)
        {
            Debug.Log("Тайм-аут истек. Период ожидания, прошедший до завершения операции, или сервер не отвечает." + ex);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (MySqlException ex)
        {
            Debug.Log("Ошибка подключения к базе данных: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }
        catch (Exception ex)
        {   
            Debug.Log("Ошибка подключения к базе данных: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }

        UIManager.instance.restartForumBtn.SetActive(false);
    }

    public void CloseFullQuestionInfInUnity()
    {
        replyText.text = "";

        for (int i = 2; i < replies.Count; i++)
            Destroy(replies[i]);
        
        replies.Clear();

        ResizeQuestionParent(fullQuestionInfParent, replies);

        fullQuestionInfParent.anchoredPosition = startFullQuestionInfParent;
    }

    public void SendQuestion()
    {
        string tl = titleDropdown.options[titleDropdown.value].text;
        string tx = questionField.text;
        if (tx.Length < 50)
        {
            errorSendQuestionPanelText.text = "Мин. длина сообщения 50 символов!";
        }
        else
        {
            if ((DateTime.UtcNow - DBManager.lastWriteTime).TotalSeconds < DBManager.writeInterval)
            {
                errorSendQuestionPanelText.text = "Только 1 сообщение в 5 мин!";
            }
            else 
            {
                messageController.CreatQuestion(ref tl, ref tx);
            }
        }
    }

    public void SendReply(GameObject reply)
    {
        int qId = Convert.ToInt32(reply.transform.Find("QuestionId-text").GetComponent<TMP_Text>().text);
        string tx = replyText.text;
        if (tx.Length < 50)
        {
            UIManager.instance.errorSendReplyPanelText.text = "Мин. длина сообщения 50 символов!";
            UIManager.instance.OpenErrorSendReplyPanel();
        }
        else
        {
            if ((DateTime.UtcNow - DBManager.lastWriteTime).TotalSeconds < DBManager.writeInterval)
            {
                UIManager.instance.errorSendReplyPanelText.text = "Доступно только 1 сообщение в 5 мин!";
                UIManager.instance.OpenErrorSendReplyPanel();
            }
            else
            {
                messageController.CreatReply(ref qId, ref tx);
            }
        }
    }

    public void ResizeQuestionParent(RectTransform panelParent, List<GameObject> objects)
    {
        // Получаем текущий размер panelParent
        RectTransform panelParentTransform = panelParent.transform as RectTransform;
        Vector2 panelParentSize = panelParentTransform.sizeDelta;

        // Получаем общий размер всех панелей сообщений, включая добавленные
        float newHeght = 0;
        foreach (GameObject obj in objects)
        {
            RectTransform objTransform = obj.transform as RectTransform;
            newHeght += objTransform.rect.height + 20;
        }

        // Устанавливаем новый размер для panelParent
        panelParentSize.y = newHeght;
        panelParentTransform.sizeDelta = panelParentSize;

        // Устанавливаем позицию всех новых сообщений в соответствии с их новым порядком
        for (int i = 0; i < objects.Count; i++)
        {
            RectTransform objTransform = objects[i].transform as RectTransform;
            objTransform.anchoredPosition = new Vector2(0, -i * objTransform.rect.height);
        }
    }

    private void OnDestroy()
    {
        foreach (GameObject question in questions)
        {
            Destroy(question);
        }
        foreach (GameObject reply in replies)
        {
            Destroy(reply);
        }
        DBManager.connection.Close();
    }
}
