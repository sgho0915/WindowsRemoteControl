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
    // ��� ��������� PC���� ���� ��Ʈ��ũ���� �����ؾ� ��
    // ����� ��� PC���� MAC Address�� �˾ƾ� ��
    // ����� ��� PC���� ���� IP�� DHCP�� �������Ѿ� ��
    public byte[] m_MacAddress; // �������� ������ PC�� MAC �ּ��Դϴ�. 6�ڸ� ������ �ּҸ� �����մϴ�.
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

    #region ���� PC ���� ON / OFF
    //// TurnOnPC �޼ҵ�� �־��� MAC �ּҸ� ����Ͽ� ��Ʈ��ũ�� ���� PC�� �Ѵ� Magic Packet�� �����ϴ�.
    //void TurnOnPC(byte[] macAddress)
    //{
    //    UdpClient client = new UdpClient();
    //    client.Connect(IPAddress.Broadcast, 40000); // UDP�� ���� ��Ʈ��ũ�� ��� ����̽��� ��ε�ĳ��Ʈ�մϴ�.

    //    byte[] packet = new byte[17 * 6]; // Magic Packet ����

    //    for (int i = 0; i < 6; i++) // ��Ŷ�� ù 6����Ʈ�� FF�� �����մϴ�.
    //    {
    //        packet[i] = 0xFF;
    //    }

    //    for (int i = 1; i <= 16; i++) // MAC �ּҸ� 16�� �ݺ��Ͽ� ��Ŷ�� �߰��մϴ�.
    //    {
    //        for (int j = 0; j < 6; j++)
    //        {
    //            packet[i * 6 + j] = macAddress[j];
    //        }
    //    }
    //    client.Send(packet, packet.Length); // ��Ŷ�� �����մϴ�.
    //    client.Close();
    //}

    //WoL���� ���Ǵ� ��Ŷ�� ���� ��Ŷ�̶�� �Ҹ���.
    //���� ��Ŷ�� ���� 6����Ʈ�� 0xFF(256) �ڿ� 48��Ʈ(6����Ʈ)�� �̷���� ��ī���� MAC �ּҰ� 16�� �ݺ��Ǿ� ���´�.
    //�̷��� ��ü 102����Ʈ�� ������ ���� ��Ŷ�� �����Ѵ�.
    public void SendWOL()
    {
        byte[] macBytes = ParseMacAddress("806D9703738D");
        byte[] packet = new byte[17 * 6];

        // ���� ��Ŷ�� ù 6����Ʈ�� 0xFF
        for (int i = 0; i < 6; i++)
        {
            packet[i] = 0xFF;
        }

        // ���� 16�� �ݺ��Ͽ� MAC �ּҸ� ��Ŷ�� �߰�
        for (int i = 1; i <= 16; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                packet[i * 6 + j] = macBytes[j];
            }
        }



        // ���� ���� �� ��Ŷ ����
        using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            sock.EnableBroadcast = true;
            sock.SendTo(packet, new IPEndPoint(IPAddress.Broadcast, 9));
        }
    }

    // MAC �ּ� ���ڿ��� ����Ʈ �迭�� ��ȯ (������ ���� ����)
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

    // �ý����� �˴ٿ��ϴ� �޼ҵ�
    public void TurnOffPC()
    {
        try
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd.exe", "/c shutdown /s /f /t 0") // ��� ����
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

    #region ���� PC ���� ���α׷� ���� / ����
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

        // ���ڿ� ����: �յ� ���� �� ���� ���� ����
        executablePath = executablePath.Trim();

        Debug.Log($"Kill executablePath2 : {executablePath}");

        // ���� �̸� ���� �õ�
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
                StandardOutputEncoding = System.Text.Encoding.UTF8  // UTF-8 ���ڵ� ���
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

    #region ������ ���� �� �ڵ� ����
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
        // ���� ���� �Ǵ� �ٸ� ������κ��� ���� ������ ��θ� �ε�
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
