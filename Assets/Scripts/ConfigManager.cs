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
    // ConfigurationManager 스크립트를 씬의 게임 오브젝트에 첨부할 수 있습니다. 구성 데이터를 저장, 업데이트 또는 로드하려면 SaveConfig, UpdateConfig, LoadConfig 메소드를 사용하면 됩니다. 예를 들어 구성을 업데이트하려면 다음과 같이 호출할 수 있습니다:
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
        //Debug.Log($"설정이 저장되었습니다 : {json}");
    }

    public ConfigData LoadConfig()
    {
        if (File.Exists("config.json"))
        {
            string json = File.ReadAllText("config.json");
            ConfigData data = JsonUtility.FromJson<ConfigData>(json);
            //Debug.Log($"설정이 로드되었습니다 : {json}");
            return data;
        }
        else
        {
            Debug.Log("설정 파일을 찾을 수 없습니다. 새 파일을 생성합니다.");
            ConfigData newData = new ConfigData { IsFirst = true, IsServer = false, IsClient = false, AutoStart = false, HostIP = "", ClientIP = "", ClientMAC = "", ClientName = "", FilePath = "" };
            SaveConfig(newData);
            return newData;
        }
    }

    // 설정 초기화
    public void ResetConfig()
    {
        ConfigData newData = new ConfigData { IsFirst = true, IsServer = false, IsClient = false, AutoStart = false, HostIP = "", ClientIP = "", ClientMAC = "", ClientName = "", FilePath = "" };
        SaveConfig(newData);
    }

    #region 첫 실행 IsFirst
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

    #region 서버인가? IsServer
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

    #region 클라이언트인가? IsClient
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

    #region 부팅시 자동 실행 AutoStart
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

    #region 서버 IP HostIP
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

    #region 클라이언트 IP ClientIP
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

    #region 클라이언트 MAC ClientMAC
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

    #region 클라이언트명(=장비명) ClientName
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

    #region 클라이언트 실행 파일 경로 FilePath
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
