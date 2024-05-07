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
    // 모든 원격제어될 PC들은 동일 네트워크망에 존재해야 함
    // 제어될 모든 PC들의 MAC Address를 알아야 함
    // 제어될 모든 PC들의 내부 IP를 DHCP로 고정시켜야 함
    public byte[] m_MacAddress; // 원격으로 제어할 PC의 MAC 주소입니다. 6자리 물리적 주소를 저장합니다.
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
    
    // TurnOnPC 메소드는 주어진 MAC 주소를 사용하여 네트워크를 통해 PC를 켜는 Magic Packet을 보냅니다.
    void TurnOnPC(byte[] macAddress)
    {
        UdpClient client = new UdpClient();
        client.Connect(IPAddress.Broadcast, 40000); // UDP를 통해 네트워크의 모든 디바이스에 브로드캐스트합니다.

        byte[] packet = new byte[17 * 6]; // Magic Packet 생성

        for (int i = 0; i < 6; i++) // 패킷의 첫 6바이트는 FF로 설정합니다.
        {
            packet[i] = 0xFF;
        }

        for (int i = 1; i <= 16; i++) // MAC 주소를 16번 반복하여 패킷에 추가합니다.
        {
            for (int j = 0; j < 6; j++)
            {
                packet[i * 6 + j] = macAddress[j];
            }
        }
        client.Send(packet, packet.Length); // 패킷을 전송합니다.
        client.Close();
    }

    // TurnOffPC 메소드는 주어진 MAC 주소를 기반으로 IP 주소를 조회하고, 해당 IP 주소의 PC를 종료시킵니다.
    void TurnOffPC(byte[] macAddress)
    {
        string macHex = BitConverter.ToString(macAddress).Replace("-", "");
        string ipAddress = GetIPAddressFromMAC(macHex); // MAC 주소로부터 IP 주소를 조회합니다.

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

    // GetIPAddressFromMAC 메소드는 ARP 테이블을 조회하여 MAC 주소에 해당하는 IP 주소를 찾습니다.
    string GetIPAddressFromMAC(string macAddress)
    {
        Process process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = "/c arp -a"; // ARP 테이블 조회 명령
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
                        return parts[0]; // 찾은 IP 주소를 반환합니다.
                    }
                }
            }
        }
        return null; // MAC 주소에 해당하는 IP 주소를 찾지 못한 경우 null을 반환합니다.
    }

    // 문자열 형식의 MAC 주소를 바이트 배열로 변환하는 메소드 추가
    byte[] ParseMacAddress(string macAddress)
    {
        try
        {
            // MAC 주소 형식이 맞는지 확인하고 변환합니다.
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
                return null; // 유효하지 않은 MAC 주소 형식이면 null 반환
            }
        }
        catch (Exception)
        {
            return null; // 변환 중 오류 발생 시 null 반환
        }
    }
}
