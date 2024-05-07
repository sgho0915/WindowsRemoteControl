using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System;
using System.IO;
using Debug = UnityEngine.Debug;
using UnityEngine.UI;
using TMPro;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Collections;
using UnityEditor;
using System.Text;
using System.Net.Mail;

public class ControlManager : MonoBehaviour
{
    // 모든 원격제어될 PC들은 동일 네트워크망에 존재해야 함
    // 제어될 모든 PC들의 MAC Address를 알아야 함
    // 제어될 모든 PC들의 내부 IP를 DHCP로 고정시켜야 함
    public byte[] m_MacAddress; // 원격으로 제어할 PC의 MAC 주소입니다. 6자리 물리적 주소를 저장합니다.
    public Button btnTurnOnPC;
    public Button btnTurnOffPC;
    public TMP_InputField inputfield_MacAddr;
    public static ControlManager Instance;

    private void Awake()
    {
        Instance = this;

        //btnTurnOnPC.onClick.RemoveAllListeners();
        //btnTurnOnPC.onClick.AddListener(() =>
        //{
        //    string macAddr = inputfield_MacAddr.text;
        //    byte[] macBytes = ParseMacAddress(macAddr);
        //    if (macBytes != null)
        //    {
        //        TurnOnPC(macBytes);
        //    }
        //    else
        //    {
        //        Debug.LogError("Invalid MAC address format.");
        //    }
        //});

        //btnTurnOffPC.onClick.RemoveAllListeners();
        //btnTurnOffPC.onClick.AddListener(() =>
        //{
        //    string macAddr = inputfield_MacAddr.text;
        //    byte[] macBytes = ParseMacAddress(macAddr);
        //    if (macBytes != null)
        //    {
        //        TurnOffPC(macBytes);
        //    }
        //    else
        //    {
        //        Debug.LogError("Invalid MAC address format.");
        //    }
        //});
    }

    #region 원격 PC 전원 ON / OFF
    //// TurnOnPC 메소드는 주어진 MAC 주소를 사용하여 네트워크를 통해 PC를 켜는 Magic Packet을 보냅니다.
    //void TurnOnPC(byte[] macAddress)
    //{
    //    UdpClient client = new UdpClient();
    //    client.Connect(IPAddress.Broadcast, 40000); // UDP를 통해 네트워크의 모든 디바이스에 브로드캐스트합니다.

    //    byte[] packet = new byte[17 * 6]; // Magic Packet 생성

    //    for (int i = 0; i < 6; i++) // 패킷의 첫 6바이트는 FF로 설정합니다.
    //    {
    //        packet[i] = 0xFF;
    //    }

    //    for (int i = 1; i <= 16; i++) // MAC 주소를 16번 반복하여 패킷에 추가합니다.
    //    {
    //        for (int j = 0; j < 6; j++)
    //        {
    //            packet[i * 6 + j] = macAddress[j];
    //        }
    //    }
    //    client.Send(packet, packet.Length); // 패킷을 전송합니다.
    //    client.Close();
    //}

    //WoL에서 사용되는 패킷은 매직 패킷이라고 불린다.
    //매직 패킷은 먼저 6바이트의 0xFF(256) 뒤에 48비트(6바이트)로 이루어진 랜카드의 MAC 주소가 16번 반복되어 나온다.
    //이렇게 전체 102바이트의 데이터 매직 패킷을 구성한다.
    public void SendWOL()
    {
        byte[] macBytes = ParseMacAddress("806D9703738D");
        byte[] packet = new byte[17 * 6];

        // 마법 패킷의 첫 6바이트는 0xFF
        for (int i = 0; i < 6; i++)
        {
            packet[i] = 0xFF;
        }

        // 이후 16번 반복하여 MAC 주소를 패킷에 추가
        for (int i = 1; i <= 16; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                packet[i * 6 + j] = macBytes[j];
            }
        }



        // 소켓 생성 및 패킷 전송
        using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            sock.EnableBroadcast = true;
            sock.SendTo(packet, new IPEndPoint(IPAddress.Broadcast, 9));
        }
    }

    // MAC 주소 문자열을 바이트 배열로 변환 (하이픈 없는 형식)
    private byte[] ParseMacAddress(string mac)
    {
        byte[] macBytes = new byte[6];
        for (int i = 0; i < 6; i++)
        {
            string hex = mac.Substring(i * 2, 2);
            macBytes[i] = Convert.ToByte(hex, 16);
        }
        return macBytes;
    }

    // 시스템을 셧다운하는 메소드
    public void TurnOffPC()
    {
        try
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd.exe", "/c shutdown /s /f /t 0") // 즉시 종료
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process proc = new Process { StartInfo = procStartInfo };
            proc.Start();
            string result = proc.StandardOutput.ReadToEnd();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in shutting down the system: " + ex.Message);
        }
    }
    #endregion

    #region 원격 PC 응용 프로그램 실행 / 종료
    public void RunProcessByPath(string executablePath)
    {
        if (string.IsNullOrEmpty(executablePath))
        {
            Debug.LogError("Run executablePath is null or empty");
            return;
        }

        try
        {
            Process process = new Process();
            process.StartInfo.FileName = executablePath.TrimEnd('\n');
            Debug.Log($"Run executablePath : {process.StartInfo.FileName}");
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }
        catch (Exception e)
        {
        }
    }

    public void KillProcessByPath(string executablePath)
    {
        if (string.IsNullOrEmpty(executablePath))
        {
            Debug.LogError("Kill executablePath is null or empty");
            return;
        }

        // 문자열 정제: 앞뒤 공백 및 개행 문자 제거
        executablePath = executablePath.Trim();

        Debug.Log($"Kill executablePath2 : {executablePath}");

        // 파일 이름 추출 시도
        try
        {
            string executableName = Path.GetFileName(executablePath);
            Debug.Log($"Kill executableName : {executableName}");

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/c taskkill /IM \"{executableName}\" /F",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8  // UTF-8 인코딩 명시
            };

            using (Process process = new Process() { StartInfo = startInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                Debug.Log($"Command output: {output}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error retrieving file name: {ex.Message}");
        }
    }    
    #endregion

    #region 윈도우 부팅 시 자동 실행
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    private void MinimizeWindow()
    {
        ShowWindow(GetActiveWindow(), 2); // 2 is SW_MINIMIZE
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    public void SetAutoStart(bool enable)
    {
        string appName = "Remote_Control";
        // 구성 파일 또는 다른 방법으로부터 실행 파일의 경로를 로드
        string appPath = GetModuleFileName();

        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
        {
            if (enable)
            {
                key.SetValue(appName, $"\"{appPath}\"");
            }
            else
            {
                key.DeleteValue(appName, false);
            }
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint GetModuleFileName(IntPtr hModule, System.Text.StringBuilder lpFilename, int nSize);
    private string GetModuleFileName()
    {
        System.Text.StringBuilder buffer = new System.Text.StringBuilder(260);
        GetModuleFileName(IntPtr.Zero, buffer, buffer.Capacity);
        return buffer.ToString();
    }
    #endregion
}
