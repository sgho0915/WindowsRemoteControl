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
            Chat.instance.ShowMessage($"������ {port}���� ���۵Ǿ����ϴ�.");
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
            // Ŭ���̾�Ʈ�� ������ ������ֳ�?
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                disconnectList.Add(c);
                continue;
            }
            // Ŭ���̾�Ʈ�κ��� üũ �޽����� �޴´�
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

            //Debug.Log($"�����:{c.clientName}");
        }

        foreach (ServerClient c in disconnectList)
        {
            //Debug.Log($"����ȵ�:{c.clientName}");
        }

        for (int i = 0; i < disconnectList.Count - 1; i++)
        {
            Broadcast($"{disconnectList[i].clientName} ������ ���������ϴ�", clients);

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

        // �޽����� ����� ��ο��� ����
        Broadcast("%NAME", new List<ServerClient>() { clients[clients.Count - 1] });
    }

    // Ŭ���̾�Ʈ�κ��� ������ ����
    void OnIncomingData(ServerClient c, string data)
    {
        if (data.Contains("&NAME"))
        {
            c.clientName = data.Split('|')[1];
            Broadcast($"{c.clientName}�� ����Ǿ����ϴ�", clients);
            Chat.instance.ShowMessage($"{c.clientName}�� ����Ǿ����ϴ�");
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

                // ServerManager�� UpdateClientDetails ȣ��
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

    // Ŭ���̾�Ʈ�鿡�� ��ε�ĳ������
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
                Chat.instance.ShowMessage($"���� ���� : {e.Message}�� Ŭ���̾�Ʈ���� {c.clientName}");
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