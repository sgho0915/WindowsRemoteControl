using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using TMPro;

public class Server : MonoBehaviour
{
    public static Server instance;

    //public TMP_InputField PortInput;
    public string hostPort;

    public List<ServerClient> clients;
    public List<ServerClient> disconnectList;

    TcpListener server;
    public bool serverStarted;

    private void Awake()
    {
        instance = this;
    }

    public void ServerCreate()
    {
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();

        try
        {
            int port = hostPort == string.Empty ? 7777 : int.Parse(hostPort);
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            StartListening();
            serverStarted = true;
            Chat.instance.ShowMessage($"서버가 {port}에서 시작되었습니다.");
        }
        catch (Exception e)
        {
            Chat.instance.ShowMessage($"Socket error: {e.Message}");
        }
    }

    void Update()
    {
        if (!serverStarted) return;

        foreach (ServerClient c in clients)
        {
            // 클라이언트가 여전히 연결되있나?
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                disconnectList.Add(c);
                continue;
            }
            // 클라이언트로부터 체크 메시지를 받는다
            else
            {
                NetworkStream s = c.tcp.GetStream();
                if (s.DataAvailable)
                {
                    string data = new StreamReader(s, true).ReadLine();
                    if (data != null)
                        OnIncomingData(c, data);
                }
            }

            //Debug.Log($"연결됨:{c.clientName}");
        }

        foreach (ServerClient c in disconnectList)
        {
            //Debug.Log($"연결안됨:{c.clientName}");
        }

        for (int i = 0; i < disconnectList.Count - 1; i++)
        {
            Broadcast($"{disconnectList[i].clientName} 연결이 끊어졌습니다", clients);

            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
        }
    }



    public bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);

                return true;
            }
            else
                return false;
        }
        catch
        {
            return false;
        }
    }

    void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));
        StartListening();

        // 메시지를 연결된 모두에게 보냄
        Broadcast("%NAME", new List<ServerClient>() { clients[clients.Count - 1] });
    }

    // 클라이언트로부터 데이터 수신
    void OnIncomingData(ServerClient c, string data)
    {
        if (data.Contains("&NAME"))
        {
            c.clientName = data.Split('|')[1];
            Broadcast($"{c.clientName}이 연결되었습니다", clients);
            Chat.instance.ShowMessage($"{c.clientName}이 연결되었습니다");
            return;
        }
        else if (data.Contains("&INFO"))
        {
            string[] parts = data.Split('|');
            if (parts.Length == 5)
            {
                string clientId = parts[3];
                c.clientName = parts[1];
                c.clientIP = parts[2];
                c.clientMAC = parts[3];
                c.clientFilePath = parts[4];
                Dictionary<string, string> details = new Dictionary<string, string>
                {
                    {"Name", parts[1]},
                    {"IP", parts[2]},
                    {"MAC", parts[3]},
                    {"Path", parts[4]}
                };

                // ServerManager의 UpdateClientDetails 호출
                ServerManager.instance.UpdateClientDetails(clientId, details);

                Chat.instance.ShowMessage($"{data}");
            }
        }
        else
        {
            Broadcast($"{c.clientName} : {data}", clients);
            Chat.instance.ShowMessage($"{c.clientName} : {data}");
        }        
    }

    // 클라이언트들에게 브로드캐스팅함
    public void Broadcast(string data, List<ServerClient> cl)
    {
        foreach (var c in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Chat.instance.ShowMessage($"쓰기 에러 : {e.Message}를 클라이언트에게 {c.clientName}");
            }
        }
    }
}


public class ServerClient
{
    public TcpClient tcp;
    public string clientName;
    public string clientIP;
    public string clientMAC;
    public string clientFilePath;

    public ServerClient(TcpClient clientSocket)
    {
        clientName = "Guest";
        clientIP = "000.000.000.000";
        clientMAC = "ABCDEFGHIJKL";
        clientFilePath = "Not Define";
        tcp = clientSocket;
    }
}