using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System;
using System.IO;
using Debug = UnityEngine.Debug;
using UnityEngine.UI;
using TMPro;

public class WOLManager : MonoBehaviour
{
    // ��� ��������� PC���� ���� ��Ʈ��ũ���� �����ؾ� ��
    // ����� ��� PC���� MAC Address�� �˾ƾ� ��
    // ����� ��� PC���� ���� IP�� DHCP�� �������Ѿ� ��
    public byte[] m_MacAddress; // �������� ������ PC�� MAC �ּ��Դϴ�. 6�ڸ� ������ �ּҸ� �����մϴ�.
    public Button btnTurnOnPC;
    public Button btnTurnOffPC;
    public TMP_InputField inputfield_MacAddr;
    public static WOLManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        btnTurnOnPC.onClick.RemoveAllListeners();
        btnTurnOnPC.onClick.AddListener(() =>
        {
            string macAddr = inputfield_MacAddr.text;
            byte[] macBytes = ParseMacAddress(macAddr);
            if (macBytes != null)
            {
                TurnOnPC(macBytes);
            }
            else
            {
                Debug.LogError("Invalid MAC address format.");
            }
        });

        btnTurnOffPC.onClick.RemoveAllListeners();
        btnTurnOffPC.onClick.AddListener(() =>
        {
            string macAddr = inputfield_MacAddr.text;
            byte[] macBytes = ParseMacAddress(macAddr);
            if (macBytes != null)
            {
                TurnOffPC(macBytes);
            }
            else
            {
                Debug.LogError("Invalid MAC address format.");
            }
        });
    }
    
    // TurnOnPC �޼ҵ�� �־��� MAC �ּҸ� ����Ͽ� ��Ʈ��ũ�� ���� PC�� �Ѵ� Magic Packet�� �����ϴ�.
    void TurnOnPC(byte[] macAddress)
    {
        UdpClient client = new UdpClient();
        client.Connect(IPAddress.Broadcast, 40000); // UDP�� ���� ��Ʈ��ũ�� ��� ����̽��� ��ε�ĳ��Ʈ�մϴ�.

        byte[] packet = new byte[17 * 6]; // Magic Packet ����

        for (int i = 0; i < 6; i++) // ��Ŷ�� ù 6����Ʈ�� FF�� �����մϴ�.
        {
            packet[i] = 0xFF;
        }

        for (int i = 1; i <= 16; i++) // MAC �ּҸ� 16�� �ݺ��Ͽ� ��Ŷ�� �߰��մϴ�.
        {
            for (int j = 0; j < 6; j++)
            {
                packet[i * 6 + j] = macAddress[j];
            }
        }
        client.Send(packet, packet.Length); // ��Ŷ�� �����մϴ�.
        client.Close();
    }

    // TurnOffPC �޼ҵ�� �־��� MAC �ּҸ� ������� IP �ּҸ� ��ȸ�ϰ�, �ش� IP �ּ��� PC�� �����ŵ�ϴ�.
    void TurnOffPC(byte[] macAddress)
    {
        string macHex = BitConverter.ToString(macAddress).Replace("-", "");
        string ipAddress = GetIPAddressFromMAC(macHex); // MAC �ּҷκ��� IP �ּҸ� ��ȸ�մϴ�.

        if (!string.IsNullOrEmpty(ipAddress))
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/c shutdown /m \\\\{ipAddress} /s /f /t 0",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            Process process = new Process() { StartInfo = startInfo };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Debug.Log(output);
        }
        else
        {
            Debug.LogError("IP address not found for the given MAC address.");
        }
    }

    // GetIPAddressFromMAC �޼ҵ�� ARP ���̺��� ��ȸ�Ͽ� MAC �ּҿ� �ش��ϴ� IP �ּҸ� ã���ϴ�.
    string GetIPAddressFromMAC(string macAddress)
    {
        Process process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = "/c arp -a"; // ARP ���̺� ��ȸ ���
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        using (StringReader reader = new StringReader(output))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains(macAddress))
                {
                    string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        return parts[0]; // ã�� IP �ּҸ� ��ȯ�մϴ�.
                    }
                }
            }
        }
        return null; // MAC �ּҿ� �ش��ϴ� IP �ּҸ� ã�� ���� ��� null�� ��ȯ�մϴ�.
    }

    // ���ڿ� ������ MAC �ּҸ� ����Ʈ �迭�� ��ȯ�ϴ� �޼ҵ� �߰�
    byte[] ParseMacAddress(string macAddress)
    {
        try
        {
            // MAC �ּ� ������ �´��� Ȯ���ϰ� ��ȯ�մϴ�.
            string[] macBytes = macAddress.Split('-');
            if (macBytes.Length == 6)
            {
                byte[] result = new byte[6];
                for (int i = 0; i < 6; i++)
                {
                    result[i] = Convert.ToByte(macBytes[i], 16);
                }
                return result;
            }
            else
            {
                return null; // ��ȿ���� ���� MAC �ּ� �����̸� null ��ȯ
            }
        }
        catch (Exception)
        {
            return null; // ��ȯ �� ���� �߻� �� null ��ȯ
        }
    }
}
