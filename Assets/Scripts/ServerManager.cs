using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerManager : MonoBehaviour
{
    public static ServerManager instance;

    // 클라이언트 정보를 저장하는 딕셔너리
    Dictionary<string, Dictionary<string, string>> clientDetails = new Dictionary<string, Dictionary<string, string>>();
    // 클라이언트 GameObject 인스턴스를 저장하는 딕셔너리
    Dictionary<string, GameObject> clientInstances = new Dictionary<string, GameObject>();
    Coroutine updateRoutine = null;
    WaitForSeconds wait1 = new WaitForSeconds(1);
    public GameObject clientPrefab;
    public Transform clientsScrollVeiwContent;

    Color colorGreen = new Color(116 / 255f, 178 / 255f, 8 / 255f, 1);
    Color colorDarkGray = new Color(45 / 255f, 45 / 255f, 45 / 255f, 1);
    Color colorGray = new Color(153 / 255f, 153 / 255f, 153 / 255f, 1);

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        LoadClientDetails(); // 서버 시작 시 클라이언트 정보 로드

        foreach (var client in clientDetails)
        {
            Debug.Log($"Client ID: {client.Key}");
            foreach (var detail in client.Value)
            {
                Debug.Log($"{detail.Key}: {detail.Value}");
            }
        }
    }

    private void OnApplicationQuit()
    {
        SaveClientDetails(); // 서버 종료 시 클라이언트 정보 저장
    }

    public void UpdateClientDetails(string clientId, Dictionary<string, string> receivedDetails)
    {
        if (clientDetails.ContainsKey(clientId))
        {
            // 클라이언트가 이미 존재하면, 변경된 정보만 업데이트
            foreach (var detail in receivedDetails)
            {
                if (!clientDetails[clientId].ContainsKey(detail.Key) || !clientDetails[clientId][detail.Key].Equals(detail.Value))
                {
                    clientDetails[clientId][detail.Key] = detail.Value;
                    BroadcastChange(clientId, detail.Key, detail.Value);
                }
            }
        }
        else
        {
            // 새 클라이언트이면, 정보 추가
            clientDetails.Add(clientId, receivedDetails);
            BroadcastNewClient(clientId);
        }

        SaveClientDetails();
    }

    void BroadcastChange(string clientId, string key, string value)
    {
        Debug.Log($"Client {clientId} updated {key} to {value}");
    }

    void BroadcastNewClient(string clientId)
    {
        Debug.Log($"New client {clientId} connected with details {clientDetails[clientId]}");
    }

    // 클라이언트 정보 저장
    void SaveClientDetails()
    {
        string json = JsonConvert.SerializeObject(clientDetails);
        File.WriteAllText("clientDetails.json", json);        
        //IUpdateClientsUI(clientDetails);

        if(updateRoutine == null)
        {
            updateRoutine = StartCoroutine(UpdateUI());
        }
    }

    // 클라이언트 정보 로드
    void LoadClientDetails()
    {
        if (File.Exists("clientDetails.json"))
        {
            string json = File.ReadAllText("clientDetails.json");
            clientDetails = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
            Debug.Log("Client details loaded.");
            IUpdateClientsUI(clientDetails);
        }
        else
        {
            Debug.Log("No client details to load.");
        }
    }

    public void IUpdateClientsUI(Dictionary<string, Dictionary<string, string>> clientDetails)
    {
        foreach (var client in clientDetails)
        {
            if (!clientInstances.ContainsKey(client.Key))
            {
                // 새 클라이언트 인스턴스 생성
                GameObject clientInstance = Instantiate(clientPrefab, clientsScrollVeiwContent);
                clientInstance.name = "Client_" + client.Value["IP"];
                clientInstances.Add(client.Key, clientInstance);
            }

            // UI 업데이트
            GameObject instance = clientInstances[client.Key];
            Outline outline = instance.GetComponent<Outline>();
            Image background = instance.GetComponent<Image>();
            Toggle toggle = instance.transform.Find("Toggle").GetComponent<Toggle>();
            TextMeshProUGUI txtName = instance.transform.Find("txtEquipName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI txtIP = instance.transform.Find("ClientInfo/txtClientIP").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI txtMAC = instance.transform.Find("ClientInfo/txtClientMAC").GetComponent<TextMeshProUGUI>();

            txtName.text = client.Value["Name"];
            txtIP.text = client.Value["IP"];
            txtMAC.text = client.Value["MAC"];

            // 현재 클라이언트가 연결되어 있는지 확인

            if (!Server.instance.serverStarted)
                return;

            ServerClient serverClient = Server.instance.clients.FirstOrDefault(c => c.clientIP == client.Value["IP"]);
            if (serverClient != null)
            {
                outline.effectColor = colorGreen;
                background.color = Color.white;
                toggle.interactable = true;
            }
            else
            {
                outline.effectColor = colorDarkGray;
                background.color = colorGray;
                toggle.interactable = false;
            }
        }
    }

    IEnumerator UpdateUI()
    {
        while (true)
        {
            IUpdateClientsUI(clientDetails);
            yield return wait1;
        }
    }
}