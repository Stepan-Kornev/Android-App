using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class MessageController : MonoBehaviour
{
    private DBManager dbManager;
    public static MessageController instance;

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
        dbManager = gameObject.AddComponent<DBManager>();
    }

    public void CreatQuestion(ref string title, ref string body)
    {
        DateTime utcNow = DateTime.UtcNow;
        string username = AuthManager.auth.CurrentUser.DisplayName;
        string userId = AuthManager.auth.CurrentUser.UserId;
        dbManager.AddQuestion(ref title, ref body, ref username, ref userId, ref utcNow);
    }

    public void CreatReply(ref int questionId, ref string body)
    {
        DateTime utcNow = DateTime.UtcNow;
        string username = AuthManager.auth.CurrentUser.DisplayName;
        string userId = AuthManager.auth.CurrentUser.UserId;
        dbManager.AddReply(ref questionId, ref body, ref username, ref userId, ref utcNow);
    }

    public void CreatNote(ref string heading, ref string text)
    {
        DateTime utcNow = DateTime.UtcNow;
        string userId = AuthManager.auth.CurrentUser.UserId;
        dbManager.AddNote(ref heading, ref text, ref userId, ref utcNow);
    }
}
