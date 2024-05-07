using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.IO;
using System;
using TMPro;
using UnityEngine.Windows;
using DG.Tweening;
using SFB;

public class Client : MonoBehaviour
{
    public static Client instance;

    public string hostIP;
    public string hostPort;    
    public string execFilePath;
    public string clientName;
    public string clientIP;
    public string clientMAC;
    public bool socketReady;
    public bool autoStart;
    public GameObject screenDebug;

    // ���� �� ����
    public TextMeshProUGUI txtClientIP;
    public TextMeshProUGUI txtClientMAC;
    public GameObject connStateOn;
    public GameObject connStateOff;

    // ���α׷� ����
    public TextMeshProUGUI txtFilePath;
    public Button btnFileSelect;

    // ����
    public TMP_InputField inputField_EquipName;
    public TMP_InputField inputField_HostIP;
    public Toggle toggle_AutoStart;
    public Button btnConnToServer;
    public Button btnSaveSettings;
    public Button btnResetSettings;

    TcpClient socket;
    NetworkStream stream;
    StreamWriter writer;
    StreamReader reader;
    WaitForSeconds wait1 = new WaitForSeconds(1f);
    WaitForSeconds wait3 = new WaitForSeconds(3f);
    Coroutine clientInfoSend = null;

    bool isConn;

    private void Awake()
    {
        instance = this;

        StartCoroutine(IClientGetInfos());
    }

    // ���Ͽ� ����
    public void ConnectToServer()
    {
        // �̹� ����Ǿ��ٸ� �Լ� ����
        if (socketReady)
        {
            return;
        }

        // �⺻ ȣ��Ʈ/ ��Ʈ��ȣ
        string ip = hostIP == string.Empty ? "127.0.0.1" : hostIP;
        int port = hostPort == string.Empty ? 7777 : int.Parse(hostPort);

        // ���� ����
        try
        {
            socket = new TcpClient(ip, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            socketReady = true;

            if (socketReady)
            {
                ConfigManager.instance.UpdateIsFirst(false);
                ConfigManager.instance.UpdateIsServer(false);
                ConfigManager.instance.UpdateIsClient(true);
                ConfigManager.instance.UpdateAutoStart(autoStart);
                ConfigManager.instance.UpdateHostIP(hostIP);
                ConfigManager.instance.UpdateClientIP(clientIP);
                ConfigManager.instance.UpdateClientMAC(clientMAC);
                ConfigManager.instance.UpdateClientName(clientName);
                ConfigManager.instance.UpdateFilePath(execFilePath);

                FirstSetManager.Instance.screenClient.SetActive(true);
                FirstSetManager.Instance.screenWelcome.SetActive(false);

                if(clientInfoSend == null)
                    clientInfoSend = StartCoroutine(IClientInfoSend());

                txtFilePath.text = execFilePath;
                inputField_EquipName.text = clientName;
                inputField_HostIP.text = hostIP;
                toggle_AutoStart.isOn = autoStart;
                
                btnFileSelect.onClick.RemoveAllListeners();
                btnFileSelect.onClick.AddListener(() =>
                {
                    string tempPath = FirstSetManager.Instance.WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", Application.dataPath, "", true));
                    if (!string.IsNullOrEmpty(tempPath))
                    {
                        execFilePath = tempPath;
                        txtFilePath.text = execFilePath;
                    }
                    ConfigManager.instance.UpdateFilePath(execFilePath);
                });

                btnSaveSettings.onClick.RemoveAllListeners();
                btnSaveSettings.onClick.AddListener(() =>
                {
                    TextMeshProUGUI btntxt = btnSaveSettings.gameObject.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
                    if (socketReady && socket != null && socket.Connected && ConfigManager.instance.LoadHostIP() != inputField_HostIP.text)
                    {
                        btntxt.text = "���� ������ ���� �������ּ���";
                        btntxt.DOColor(new Color32(240, 94, 51, 255), 1)
                            .OnComplete(() => { btntxt.text = "����"; btntxt.color = new Color32(255, 255, 255, 255); });  // �ִϸ��̼� �Ϸ� �� �ؽ�Ʈ ����
                        btntxt.transform.DOShakePosition(1, strength: new Vector3(10, 0, 0), vibrato: 10, randomness: 90, snapping: false, fadeOut: true);

                    }
                    else if (inputField_HostIP.text == string.Empty || inputField_EquipName.text == string.Empty)
                    {
                        btntxt.text = "�Է� ������ �ٽ� Ȯ���ϼ���";
                        btntxt.DOColor(new Color32(240, 94, 51, 255), 1)
                            .OnComplete(() => { btntxt.text = "����"; btntxt.color = new Color32(255, 255, 255, 255); });  // �ִϸ��̼� �Ϸ� �� �ؽ�Ʈ ����
                        btntxt.transform.DOShakePosition(1, strength: new Vector3(10, 0, 0), vibrato: 10, randomness: 90, snapping: false, fadeOut: true);
                    }
                    else
                    {
                        hostIP = inputField_HostIP.text;
                        clientName = inputField_EquipName.text;
                        autoStart = toggle_AutoStart.isOn;
                        ConfigManager.instance.UpdateAutoStart(autoStart); // �ڵ����� �� ����
                        //ControlManager.Instance.SetAutoStart(autoStart); // �ڵ����� ����(����� ��Ȱ��ȭ)
                        ConfigManager.instance.UpdateHostIP(hostIP);
                        ConfigManager.instance.UpdateClientName(clientName);

                        btntxt.text = "����Ǿ����ϴ�";
                        btntxt.DOColor(new Color32(116, 178, 8, 255), 1)
                            .OnComplete(() => { btntxt.text = "����"; btntxt.color = new Color32(255, 255, 255, 255); });  // �ִϸ��̼� �Ϸ� �� �ؽ�Ʈ ����
                    }
                });

                btnConnToServer.onClick.RemoveAllListeners();
                btnConnToServer.onClick.AddListener(() =>
                {
                    TextMeshProUGUI btntxt = btnConnToServer.gameObject.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
                    if (socketReady && socket != null && socket.Connected)
                    {
                        CloseSocket();
                    }
                    else
                    {
                        ConnectToServer();
                    }
                });

                btnResetSettings.onClick.RemoveAllListeners();
                btnResetSettings.onClick.AddListener(() =>
                {
                    CloseSocket();
                    ConfigManager.instance.ResetConfig();

                    TextMeshProUGUI btntxt = btnResetSettings.gameObject.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
                    btntxt.text = "�ʱ�ȭ �Ϸ�";
                    btntxt.DOColor(new Color32(240, 94, 51, 255), 1);  // �ִϸ��̼� �Ϸ� �� �ؽ�Ʈ ����
                    btntxt.transform.DOShakePosition(1, strength: new Vector3(10, 0, 0), vibrato: 10, randomness: 90, snapping: false, fadeOut: true);

                    Application.Quit();
                });
            }            
        }
        catch (Exception e)
        {
            socketReady = false;
            FirstSetManager.Instance.txtError.text = $"���Ͽ��� : {e.Message}";
            FirstSetManager.Instance.txtError.DOColor(new Color32(240, 94, 51, 255), 0.5f);
            FirstSetManager.Instance.txtError.transform.DOShakePosition(0.5f, strength: new Vector3(10, 0, 0), vibrato: 10, randomness: 90, snapping: false, fadeOut: true);
            Chat.instance.ShowMessage($"���Ͽ��� : {e.Message}");
        }
    }


    void Update()
    {
        if (socketReady && stream.DataAvailable)
        {
            string data = reader.ReadLine();
            if (data != null)
                OnIncomingData(data);
        }

        // ���� ��Ʈ��Ű�� �齽���ð� ���ÿ� ���ȴ��� Ȯ��
        if (UnityEngine.Input.GetKey(KeyCode.LeftControl) && UnityEngine.Input.GetKeyDown(KeyCode.Backslash))
        {
            // ���� ������Ʈ�� Ȱ��ȭ ���¸� ���
            screenDebug.SetActive(!screenDebug.activeSelf);
        }
    }

    void OnIncomingData(string data)
    {
        if (data == "%NAME")
        {
            clientName = clientName == string.Empty ? $"Guest{UnityEngine.Random.Range(1000, 10000)}" : clientName;
            Send($"&NAME|{clientName}");
            return;
        }
        else if(data.Contains("%CMDPLAY"))
        {
            // ���õ� Ŭ���̾�Ʈ�� ���α׷� ����
            string[] parts = data.Split('|');
            if (parts[1].Contains(clientMAC))
                ControlManager.Instance.RunProcessByPath(execFilePath);
        }
        else if (data.Contains("%CMDSTOP"))
        {
            // ���õ� Ŭ���̾�Ʈ�� ���α׷� ����
            string[] parts = data.Split('|');
            if (parts[1].Contains(clientMAC))
                ControlManager.Instance.KillProcessByPath(execFilePath);
        }
        else if (data.Contains("%CMDON"))
        {
            // ���õ� Ŭ���̾�Ʈ�� PC ON
            string[] parts = data.Split('|');
            if (parts[1].Contains(clientMAC))
                ControlManager.Instance.KillProcessByPath(execFilePath);
        }
        else if (data.Contains("%CMDOFF"))
        {
            // ���õ� Ŭ���̾�Ʈ�� PC OFF
            string[] parts = data.Split('|');
            if (parts[1].Contains(clientMAC))
                ControlManager.Instance.TurnOffPC();
        }
        Chat.instance.ShowMessage(data);
    }

    void Send(string data)
    {
        if (!socketReady) return;

        writer.WriteLine(data);
        writer.Flush();
    }

    public void OnSendButton(TMP_InputField SendInput)
    {
#if (UNITY_EDITOR || UNITY_STANDALONE)
        if (!UnityEngine.Input.GetButtonDown("Submit")) return;
        SendInput.ActivateInputField();
#endif
        if (SendInput.text.Trim() == "") return;

        string message = SendInput.text;
        SendInput.text = "";
        Send(message);
    }


    void OnApplicationQuit()
    {
        CloseSocket();
    }

    void CloseSocket()
    {
        if (!socketReady) return;

        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
        if (clientInfoSend != null)
        {
            StopCoroutine(IClientInfoSend());
            clientInfoSend = null;
        }
    }

    public IEnumerator IClientInfoSend()
    {
        if (socketReady)
        {
            yield return new WaitForSeconds(3);
            while (true)
            {
                Send($"&INFO|{clientName}|{clientIP}|{clientMAC}|{execFilePath}");
                yield return wait1;
            }            
        }
    }

    public IEnumerator IClientGetInfos()
    {
        TextMeshProUGUI btntxt = btnConnToServer.gameObject.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        yield return new WaitForSeconds(1);
        while (true)
        {
            //Debug.Log(socketReady);
            //if (socket != null && !socket.Connected)
            //{
            //    ConnectToServer();
            //    Debug.Log($"���� ���� �õ���");
            //}

            if (socketReady)
            {                
                try
                {
                    socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                }
                catch (Exception ex)
                {
                    CloseSocket();
                }
            }            

            if (socketReady && socket != null && socket.Connected)
            {
                btntxt.text = "�������";                
                connStateOn.SetActive(true);
                connStateOff.SetActive(false);
            }
            else
            {
                btntxt.text = "�����ϱ�";
                connStateOn.SetActive(false);
                connStateOff.SetActive(true);
            }
            txtClientIP.text = FirstSetManager.Instance.GetIPAddress();
            txtClientMAC.text = FirstSetManager.Instance.GetMacAddress();
            yield return wait3;
        }
    }

    public void CloseDebugScreen()
    {
        screenDebug.SetActive(false);
    }
}