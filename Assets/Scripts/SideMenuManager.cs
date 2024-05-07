using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class SideMenuManager : MonoBehaviour
{
    public Button btnSelectFile;
    public Button btnStateInfo;
    public Button btnSettings;
    public GameObject screenSelectFile;
    public GameObject screenStateInfo;
    public GameObject screenSettings;

    public void Awake()
    {
        btnSelectFile.onClick.RemoveAllListeners();
        btnStateInfo.onClick.RemoveAllListeners();
        btnSettings.onClick.RemoveAllListeners();

        btnSelectFile.onClick.AddListener(() =>
        {
            btnSelectFile.gameObject.transform.Find("Selected").gameObject.SetActive(true);
            btnStateInfo.gameObject.transform.Find("Selected").gameObject.SetActive(false);
            btnSettings.gameObject.transform.Find("Selected").gameObject.SetActive(false);
            screenSelectFile.SetActive(true);
            screenStateInfo.SetActive(false);
            screenSettings.SetActive(false);
        });

        btnStateInfo.onClick.AddListener(() =>
        {
            btnSelectFile.gameObject.transform.Find("Selected").gameObject.SetActive(false);
            btnStateInfo.gameObject.transform.Find("Selected").gameObject.SetActive(true);
            btnSettings.gameObject.transform.Find("Selected").gameObject.SetActive(false);
            screenSelectFile.SetActive(false);
            screenStateInfo.SetActive(true);
            screenSettings.SetActive(false);
        });

        btnSettings.onClick.AddListener(() =>
        {
            btnSelectFile.gameObject.transform.Find("Selected").gameObject.SetActive(false);
            btnStateInfo.gameObject.transform.Find("Selected").gameObject.SetActive(false);
            btnSettings.gameObject.transform.Find("Selected").gameObject.SetActive(true);
            screenSelectFile.SetActive(false);
            screenStateInfo.SetActive(false);
            screenSettings.SetActive(true);
        });

        btnSelectFile.gameObject.transform.Find("Selected").gameObject.SetActive(false);
        btnStateInfo.gameObject.transform.Find("Selected").gameObject.SetActive(true);
        btnSettings.gameObject.transform.Find("Selected").gameObject.SetActive(false);
        screenSelectFile.SetActive(false);
        screenStateInfo.SetActive(true);
        screenSettings.SetActive(false);
    }
}
