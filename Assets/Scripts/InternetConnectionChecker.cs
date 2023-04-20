using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net.NetworkInformation;
using System;

public class InternetConnectionChecker : MonoBehaviour
{
    public static InternetConnectionChecker instance; 

    public float checkInterval = 5f;

    private bool stopChecking = false;
    public bool connectionAvailable = false;


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
        StartChecking();
    }

    private void Update()
    {
        if (stopChecking)
        {
            UIManager.instance.ErrorInternetConnectionPanel.SetActive(true);
            UIManager.instance.blockBtn.SetActive(true);
        }
        if (connectionAvailable)
        {
            UIManager.instance.ErrorInternetConnectionPanel.SetActive(false);
            UIManager.instance.blockBtn.SetActive(false);
        }
    }

    private void CheckInternetConnectionThread()
    {
        while (!stopChecking)
        {
            try
            {
                System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                PingReply reply = ping.Send("8.8.8.8"); // отправляем ICMP-пакеты на сервер Google DNS

                if (reply.Status == IPStatus.Success)
                {
                    Debug.Log("Подключение к интернету доступно"); // удаленный хост доступен, подключение к Интернету есть
                    connectionAvailable = true;
                }
                else
                {
                    Debug.Log("Подключение к интернету отсутствует"); // удаленный хост недоступен, подключение к Интернету отсутствует
                    connectionAvailable = false;
                    StopChecking();
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Подключение к интернету отсутствует: " + ex.Message);
                connectionAvailable = false;
                StopChecking();
            }

            Thread.Sleep((int)(checkInterval * 1000));
        }
    }

    public void StartChecking()
    {
        stopChecking = false;

        // Создаем новый поток для выполнения проверки подключения
        Thread checkThread = new Thread(CheckInternetConnectionThread);
        checkThread.Start();
    }

    public void StopChecking()
    {
        // Устанавливаем флаг, указывающий, что проверку нужно остановить
        stopChecking = true;
    }

    // Вызываем метод StopChecking() при выходе из приложения
    void OnApplicationQuit()
    {
        StopChecking();
    }

}
