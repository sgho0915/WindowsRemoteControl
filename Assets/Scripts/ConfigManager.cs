using System;
using UnityEngine;
using System.IO;

[Serializable]
public class ConfigData
{
    public bool IsFirst;
    public bool IsServer;
    public bool IsClient;
    public string HostIP;
    public string FilePath;
    public string ClientIP;
    public string ClientMAC;
    public string ClientName;
    public bool AutoStart;
}

public class ConfigManager : MonoBehaviour
{
    // ConfigurationManager ��ũ��Ʈ�� ���� ���� ������Ʈ�� ÷���� �� �ֽ��ϴ�. ���� �����͸� ����, ������Ʈ �Ǵ� �ε��Ϸ��� SaveConfig, UpdateConfig, LoadConfig �޼ҵ带 ����ϸ� �˴ϴ�. ���� ��� ������ ������Ʈ�Ϸ��� ������ ���� ȣ���� �� �ֽ��ϴ�:
    // ConfigData newConfig = new ConfigData { IsFirst = false, IsServer = true, ... };
    // configurationManager.UpdateConfig(newConfig);

    public static ConfigManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        LoadConfig();
    }

    public void SaveConfig(ConfigData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText("config.json", json);
        //Debug.Log($"������ ����Ǿ����ϴ� : {json}");
    }

    public ConfigData LoadConfig()
    {
        if (File.Exists("config.json"))
        {
            string json = File.ReadAllText("config.json");
            ConfigData data = JsonUtility.FromJson<ConfigData>(json);
            //Debug.Log($"������ �ε�Ǿ����ϴ� : {json}");
            return data;
        }
        else
        {
            Debug.Log("���� ������ ã�� �� �����ϴ�. �� ������ �����մϴ�.");
            ConfigData newData = new ConfigData { IsFirst = true, IsServer = false, IsClient = false, AutoStart = false, HostIP = "", ClientIP = "", ClientMAC = "", ClientName = "", FilePath = "" };
            SaveConfig(newData);
            return newData;
        }
    }

    // ���� �ʱ�ȭ
    public void ResetConfig()
    {
        ConfigData newData = new ConfigData { IsFirst = true, IsServer = false, IsClient = false, AutoStart = false, HostIP = "", ClientIP = "", ClientMAC = "", ClientName = "", FilePath = "" };
        SaveConfig(newData);
    }

    #region ù ���� IsFirst
    public void UpdateIsFirst(bool newBool)
    {
        ConfigData currentConfig = LoadConfig();
        currentConfig.IsFirst = newBool;
        SaveConfig(currentConfig);
    }
    public bool LoadIsFirst()
    {
        ConfigData currentConfig = LoadConfig();
        return currentConfig.IsFirst;
    }
    #endregion

    #region �����ΰ�? IsServer
    public void UpdateIsServer(bool newBool)
    {
        ConfigData currentConfig = LoadConfig();
        currentConfig.IsServer = newBool;
        SaveConfig(currentConfig);
    }
    public bool LoadIsServer()
    {
        ConfigData currentConfig = LoadConfig();
        return currentConfig.IsServer;
    }
    #endregion

    #region Ŭ���̾�Ʈ�ΰ�? IsClient
    public void UpdateIsClient(bool newBool)
    {
        ConfigData currentConfig = LoadConfig();
        currentConfig.IsClient = newBool;
        SaveConfig(currentConfig);
    }
    public bool LoadIsClient()
    {
        ConfigData currentConfig = LoadConfig();
        return currentConfig.IsClient;
    }
    #endregion

    #region ���ý� �ڵ� ���� AutoStart
    public void UpdateAutoStart(bool newBool)
    {
        ConfigData currentConfig = LoadConfig();
        currentConfig.AutoStart = newBool;
        SaveConfig(currentConfig);
    }
    public bool LoadAutoStart()
    {
        ConfigData currentConfig = LoadConfig();
        return currentConfig.AutoStart;
    }
    #endregion

    #region ���� IP HostIP
    public void UpdateHostIP(string newString)
    {
        ConfigData currentConfig = LoadConfig();
        currentConfig.HostIP = newString;
        SaveConfig(currentConfig);
    }
    public string LoadHostIP()
    {
        ConfigData currentConfig = LoadConfig();
        return currentConfig.HostIP;
    }
    #endregion

    #region Ŭ���̾�Ʈ IP ClientIP
    public void UpdateClientIP(string newString)
    {
        ConfigData currentConfig = LoadConfig();
        currentConfig.ClientIP = newString;
        SaveConfig(currentConfig);
    }
    public string LoadClientIP()
    {
        ConfigData currentConfig = LoadConfig();
        return currentConfig.ClientIP;
    }
    #endregion

    #region Ŭ���̾�Ʈ MAC ClientMAC
    public void UpdateClientMAC(string newString)
    {
        ConfigData currentConfig = LoadConfig();
        currentConfig.ClientMAC = newString;
        SaveConfig(currentConfig);
    }
    public string LoadClientMAC()
    {
        ConfigData currentConfig = LoadConfig();
        return currentConfig.ClientMAC;
    }
    #endregion

    #region Ŭ���̾�Ʈ��(=����) ClientName
    public void UpdateClientName(string newString)
    {
        ConfigData currentConfig = LoadConfig();
        currentConfig.ClientName = newString;
        SaveConfig(currentConfig);
    }
    public string LoadClientName()
    {
        ConfigData currentConfig = LoadConfig();
        return currentConfig.ClientName;
    }
    #endregion

    #region Ŭ���̾�Ʈ ���� ���� ��� FilePath
    public void UpdateFilePath(string newString)
    {
        ConfigData currentConfig = LoadConfig();
        currentConfig.FilePath = newString;
        SaveConfig(currentConfig);
    }
    public string LoadFilePath()
    {
        ConfigData currentConfig = LoadConfig();
        return currentConfig.FilePath;
    }
    #endregion
}
