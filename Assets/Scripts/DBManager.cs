using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

public class DBManager : MonoBehaviour
{
	/*"server=vh330.timeweb.ru;username=ch66717_android;password=12345678qaz123;database=ch66717_android;port=3306;"*//*"server=localhost;port=3306;username=root;database=Android-App"*/
	// "Server=31.135.32.233;Port=3306;Uid=step;Pwd=1234;Database=step"


	public static MySqlConnection connection = new MySqlConnection("Server=31.135.32.233;Port=3306;Uid=step;Pwd=1234;Database=step");
	public static DateTime lastWriteTime = DateTime.MinValue;
	public static readonly int writeInterval = 5 * 60;

    public void AddQuestion(ref string title, ref string body, ref string username, ref string userId, ref DateTime createdAt)
	{
		try
		{
			using (connection)
			{
				connection.Open();

				MySqlCommand cmd = new MySqlCommand("INSERT INTO questions (title, body, username, user_id, created_at) VALUES (@title, @body, @username, @userId, @createdAt)", connection);
				cmd.Parameters.AddWithValue("@title", title);
				cmd.Parameters.AddWithValue("@body", body);
				cmd.Parameters.AddWithValue("@username", username);
				cmd.Parameters.AddWithValue("@userId", userId);
				cmd.Parameters.AddWithValue("@createdAt", createdAt);
				try
				{
					cmd.ExecuteNonQuery();
					Debug.Log("Вопрос отправлен успешно!");
					UIManager.instance.errorSendReplyPanelText.text = "";
					lastWriteTime = DateTime.UtcNow;

					UIManager.instance.Cancel();
					UIManager.instance.errorSendReplyPanelText.text = "Вопрос отправлен успешно! (обновите страницу)";
					UIManager.instance.OpenErrorSendReplyPanel();
					UIManager.instance.restartForumBtn.SetActive(true);
				}
				catch (Exception ex)
				{
					Debug.LogError("Error adding message: " + ex.Message);
				}
				finally { connection.Close(); }
			}
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

	public void AddReply(ref int questionId, ref string body, ref string username, ref string userId, ref DateTime createdAt)
	{
		try
		{
			using (connection)
			{
				connection.Open();

				MySqlCommand cmd = new MySqlCommand("INSERT INTO replies (question_id, body, username, user_id, created_at) VALUES (@questionId, @body, @username, @userId, @createdAt)", connection);
				cmd.Parameters.AddWithValue("@questionId", questionId);
				cmd.Parameters.AddWithValue("@body", body);
				cmd.Parameters.AddWithValue("@username", username);
				cmd.Parameters.AddWithValue("@userId", userId);
				cmd.Parameters.AddWithValue("@createdAt", createdAt);
				try
				{
					cmd.ExecuteNonQuery();
					Debug.Log("Ответ отправлен успешно!");
					ForumManager.instance.replyText.text = "";
					UIManager.instance.errorSendReplyPanelText.text = "Ваш ответ успешно отправлен! (обновите страницу)";
					UIManager.instance.OpenErrorSendReplyPanel();
					lastWriteTime = DateTime.UtcNow;
					UIManager.instance.restartRepliesBtn.SetActive(true);
				}
				catch (Exception ex)
				{
					Debug.LogError("Error adding reply: " + ex.Message);
				}
				finally { connection.Close(); }
			}
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

    public void AddNote(ref string header, ref string text, ref string userId, ref DateTime createdAt)
    {
		try
		{
			using (connection)
			{
				connection.Open();

				MySqlCommand cmd = new MySqlCommand("INSERT INTO notes (header, text, userId, createdAt) VALUES (@header, @text, @userId, @createdAt)", connection);
				cmd.Parameters.AddWithValue("@header", header);
				cmd.Parameters.AddWithValue("@text", text);
				cmd.Parameters.AddWithValue("@userId", userId);
				cmd.Parameters.AddWithValue("@createdAt", createdAt);
				try
				{
					cmd.ExecuteNonQuery();
					Debug.Log("Заметка создана успешно!");
					NoteManager.instance.headerField.text = "";
					NoteManager.instance.inputTextField.text = "";
					NoteManager.instance.errorText.text = "";
					UIManager.instance.Cancel();
					UIManager.instance.errorSendReplyPanelText.text = "Заметка создана успешно! (обновите страницу)";
					UIManager.instance.OpenErrorSendReplyPanel();
					UIManager.instance.restartNotesBtn.SetActive(true);
				}
				catch (Exception ex)
				{
					Debug.LogError("Error adding reply: " + ex.Message);
				}
				finally
				{
					connection.Close();
				}
			}
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

    private void OnDestroy()
    {
		connection.Close();
    }
}
