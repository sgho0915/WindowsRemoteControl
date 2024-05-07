using UnityEngine;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System;

public class AutoStartMinimized : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private void Start()
    {
        //// �ּ�ȭ�� ����
        //MinimizeWindow();

        //// ���� �� �ڵ� ���� ����
        //SetAutoStart(true);
    }

    private void MinimizeWindow()
    {
        ShowWindow(GetActiveWindow(), 2); // 2 is SW_MINIMIZE
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    public void SetAutoStart(bool enable)
    {
        string appName = "Remote_Control_Client";
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
}
