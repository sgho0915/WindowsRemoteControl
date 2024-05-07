using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Net;
using System.Net.NetworkInformation;

public class FirstSetManager : MonoBehaviour
{
    public static FirstSetManager Instance { get; private set; }

    [Header("Screens")]
    public GameObject screenWelcome;
    public GameObject screenSelectType;
    public GameObject screenSetClient;
    public GameObject screenClient;
    public GameObject screenServer;

    [Header("Screen Welcome")]
    public Button btnStart;

    [Header("Screen SelectType")]
    public Button btnServer;
    public Button btnClient;

    [Header("Screen FirstSetClient")]
    public GameObject screenSelectProgram;
    public GameObject screenConnServer;
    public TextMeshProUGUI txtError;
    public Button btnFindProgram; // 제어 프로그램 찾기 버튼
    public TextMeshProUGUI txtSelectedFilePath; // 제어 프로그램 
    public Button btnNext; // 서버 연결 이동 버튼
    public string selectedFilePath = string.Empty;

    [Header("Client Setting Elements")]
    public TMP_InputField inputField_ServerIP;
    public TMP_InputField inputField_EquipName;
    public Button btnConn; // 서버 연결 이동 버튼
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (ConfigManager.instance.LoadIsFirst() == true)
        {
            screenClient.SetActive(false);
            screenServer.SetActive(false);
            screenWelcome.SetActive(true);
        }
        else
        {
            if (ConfigManager.instance.LoadIsServer() == true)
            {
                // 초기 세팅 마친 서버면 메인화면 진입
                Server.instance.ServerCreate();
                screenServer.SetActive(true);
                screenWelcome.SetActive(false);
                return;
            }
            else if (ConfigManager.instance.LoadIsClient() == true)
            {
                // 초기 세팅 마친 클라이언트면 메인화면 진입
                Client.instance.hostPort = 7777.ToString();
                if (ConfigManager.instance.LoadIsFirst() == true)
                {
                    Client.instance.hostIP = ConfigManager.instance.LoadHostIP();
                    Client.instance.execFilePath = ConfigManager.instance.LoadFilePath();
                    Client.instance.clientName = ConfigManager.instance.LoadClientName();
                    Client.instance.autoStart = ConfigManager.instance.LoadAutoStart();
                }
                Client.instance.clientIP = GetIPAddress();
                Client.instance.clientMAC = GetMacAddress();
                Client.instance.ConnectToServer();
                screenClient.SetActive(true);
                screenWelcome.SetActive(false);
            }
        }
    }

    public void GotoSelectType()
    {
        screenWelcome.SetActive(false);
        screenSelectType.SetActive(true);

        btnServer.onClick.RemoveAllListeners();
        btnClient.onClick.RemoveAllListeners();

        btnServer.onClick.AddListener(() =>
        {
            Server.instance.ServerCreate();
            ConfigManager.instance.UpdateIsFirst(false);
            ConfigManager.instance.UpdateIsServer(true);
            ConfigManager.instance.UpdateIsClient(false);

            screenServer.SetActive(true);
            screenWelcome.SetActive(false);
        });
        btnClient.onClick.AddListener(() =>
        {
            screenSelectType.SetActive(false);
            screenSetClient.SetActive(true);
            screenSelectProgram.SetActive(true);
            SelectFile();
        });
    }

    public void SelectFile()
    {
        btnFindProgram.onClick.RemoveAllListeners();
        btnFindProgram.onClick.AddListener(() =>
        {
            selectedFilePath = WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", Application.dataPath, "", true));
            txtSelectedFilePath.text = selectedFilePath;
        });

        btnNext.onClick.RemoveAllListeners();
        btnNext.onClick.AddListener(() =>
        {
            if(selectedFilePath == string.Empty)
            {
                txtError.text = "프로그램이 선택되지 않았습니다.";
                txtError.DOColor(new Color32(240, 94, 51, 255), 0.5f);
                txtError.transform.DOShakePosition(0.5f, strength: new Vector3(10, 0, 0), vibrato: 10, randomness: 90, snapping: false, fadeOut: true);
            }
            else
            {
                txtError.text = "";
                ConnectToServer();
            }            
        });
    }

    public void ConnectToServer()
    {
        screenSelectProgram.SetActive(false);
        screenConnServer.SetActive(true);

        inputField_ServerIP.onValueChanged.AddListener((value) =>
        {
            Client.instance.hostIP = value;
        });
        inputField_EquipName.onValueChanged.AddListener((value) =>
        {
            Client.instance.clientName = value;
        });

        btnConn.onClick.RemoveAllListeners();
        btnConn.onClick.AddListener(() =>
        {
            if(inputField_ServerIP.text.Length == 0 || inputField_EquipName.text.Length == 0)
            {
                txtError.text = "정보를 올바르게 입력해주세요.";
                txtError.DOColor(new Color32(240, 94, 51, 255), 0.5f);
                txtError.transform.DOShakePosition(0.5f, strength: new Vector3(10, 0, 0), vibrato: 10, randomness: 90, snapping: false, fadeOut: true);
            }
            else
            {
                Client.instance.hostPort = 7777.ToString();
                Client.instance.execFilePath = selectedFilePath;                
                Client.instance.clientIP = GetIPAddress();
                Client.instance.clientMAC = GetMacAddress();
                Client.instance.ConnectToServer();
            }
        });
    }

    public string GetIPAddress()
    {
        string hostName = Dns.GetHostName(); // Get the host name
        IPHostEntry host = Dns.GetHostEntry(hostName);
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString(); // Returns the first IPv4 address found
            }
        }
        return "No IPv4 address found";
    }

    public string GetMacAddress()
    {
        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            // Only consider Ethernet network interfaces for MAC address
            if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet && nic.OperationalStatus == OperationalStatus.Up)
            {
                return nic.GetPhysicalAddress().ToString();
            }
        }
        return "No Ethernet NIC found";
    }

    public string WriteResult(string[] paths)
    {
        if (paths.Length == 0)
        {
            return null;
        }

        selectedFilePath = "";
        foreach (var p in paths)
        {
            selectedFilePath += p + "\n";
        }

        return selectedFilePath;
    }
}
