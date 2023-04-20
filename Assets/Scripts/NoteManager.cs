using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using MySql.Data.MySqlClient;
using System.Data;

public class NoteManager : MonoBehaviour
{
    public static NoteManager instance;

    private MySqlCommand command;
    private MySqlDataReader reader;

    public GameObject notePrefab;
    public RectTransform noteParent;

    private List<GameObject> notes = new List<GameObject>();

    [Header("Create new note panel")]
    public TMP_InputField headerField;
    public TMP_InputField inputTextField;
    public TMP_Text errorText;

    [Header("Full note inf panel")]
    public GameObject fullNoteInfPanel;
    public TMP_Text topicText;
    public TMP_Text noteText;

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

    public void SendNote()
    {
        string header = headerField.text;
        string text = inputTextField.text;

        if (header == "")
            errorText.text = "Не введен заголовок!";
        else if (text == "")
            errorText.text = "Не введен текст заметки!";
        else
            MessageController.instance.CreatNote(ref header, ref text);
    }

    public IEnumerator LoadNotesInUnity()
    {
        foreach (GameObject note in notes)
        {
            Destroy(note);
        }
        notes.Clear();

        using (DBManager.connection)
        {
            if (DBManager.connection.State == ConnectionState.Open)
                DBManager.connection.Close();

            DBManager.connection.Open();

            command = new MySqlCommand("SELECT * FROM notes WHERE userId = @userId ORDER BY id", DBManager.connection);
            command.Parameters.AddWithValue("@userId", AuthManager.auth.CurrentUser.UserId);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                GameObject note = Instantiate(notePrefab, noteParent);

                TMP_Text idText = note.transform.Find("NoteId-text").GetComponent<TMP_Text>();
                idText.text = reader.GetString(0);
                TMP_Text topicText = note.transform.Find("Topic-text").GetComponent<TMP_Text>();
                topicText.text = reader.GetString(1);
                TMP_Text dateText = note.transform.Find("Date-text").GetComponent<TMP_Text>();
                dateText.text = reader.GetString(4).Split()[0];

                Button button = note.transform.Find("OpenFullNote-button").GetComponent<Button>();
                button.onClick.AddListener(() => UIManager.instance.OpenFullNotePanel(note));

                notes.Add(note);

                ForumManager.instance.ResizeQuestionParent(noteParent, notes);

                yield return null;
            }
            reader.Close();

            DBManager.connection.Close();
        }
    }

    public IEnumerator LoadFullNoteInfInUnity(string noteId)
    {
        using (DBManager.connection)
        {
            DBManager.connection.Open();

            command = new MySqlCommand("SELECT * FROM notes WHERE id = " + noteId, DBManager.connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                topicText.text = reader.GetString(1);

                TMP_Text text = fullNoteInfPanel.transform.Find("Items").Find("NoteText-panel").Find("Note-text").GetComponent<TMP_Text>();
                text.text = reader.GetString(2);

                yield return null;
            }
            reader.Close();

            DBManager.connection.Close();
        }
    }

    public void RestartNotes()
    {
        for (int i = 0; i < notes.Count; i++)
            Destroy(notes[i]);

        notes.Clear();

        try
        {
            StartCoroutine(LoadNotesInUnity());
        }
        catch (Exception ex)
        {
            Debug.Log("Ошибка подключения к базе данных: " + ex.Message);
            InternetConnectionChecker.instance.StartChecking();
        }

        UIManager.instance.restartForumBtn.SetActive(false);
    }
}
