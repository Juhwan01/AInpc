using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class GPTcustom : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // R Ű�� ������ �� ����
            try
            {
                Process psi = new Process();
                psi.StartInfo.FileName = "C:/Anaconda3/envs/llm/python.exe";
                // ����ȯ���� Python ���� ���� ���

                psi.StartInfo.Arguments = Application.streamingAssetsPath + "/AI-Assistant.py";
                // ������ Python ���� ��� (Ȯ���� ����)

                psi.StartInfo.CreateNoWindow = true;
                // ���ο� â�� �������� ����

                psi.StartInfo.UseShellExecute = false;
                // ���� ������� ����

                psi.StartInfo.RedirectStandardOutput = true;
                psi.StartInfo.RedirectStandardError = true;
                // ǥ�� ��� �� ������ �����̷�Ʈ

                psi.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log(args.Data);
                psi.ErrorDataReceived += (sender, args) => UnityEngine.Debug.LogError(args.Data);

                psi.Start();
                psi.BeginOutputReadLine();
                psi.BeginErrorReadLine();

                UnityEngine.Debug.Log("[�˸�] .py ���� ����");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("[�˸�] ���� �߻�: " + e.Message);
            }
        }
    }
}
