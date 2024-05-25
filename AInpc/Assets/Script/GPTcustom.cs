using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class GPTcustom : MonoBehaviour
{
    private static GPTcustom instance = null;
    public static GPTcustom Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GPTcustom();
            }
            return instance;
        }
    }
    private GoogleTTS googleTTS;

    void Start()
    {
        // GoogleTTS Ŭ������ �ν��Ͻ��� �����ͼ� ������ �Ҵ�
        googleTTS = GoogleTTS.Instance;
    }
    public void answer(string text) 
    {
        // R Ű�� ������ �� ����
        try
        {
            Process psi = new Process();
            psi.StartInfo.FileName = "C:/Anaconda3/envs/llm/Scripts/ipython3.exe";
            // ����ȯ���� Python ���� ���� ���

            psi.StartInfo.Arguments = $"{Application.streamingAssetsPath}/AI-Assistant.py \"{text}\"";



            // ������ Python ���� ��� �� �Է� �� ����

            psi.StartInfo.CreateNoWindow = true;
            // ���ο� â�� �������� ����

            psi.StartInfo.UseShellExecute = false;
            // ���� ������� ����

            psi.StartInfo.RedirectStandardOutput = true;
            psi.StartInfo.RedirectStandardError = true;
            // ǥ�� ��� �� ������ �����̷�Ʈ

            psi.OutputDataReceived += (sender, args) =>
            {
                // �޾ƿ� �����Ͱ� print���� ���� ������ Ȯ��
                if (!string.IsNullOrEmpty(args.Data) && !args.Data.StartsWith("Traceback") && !args.Data.StartsWith("File") && !args.Data.StartsWith("  "))
                {
                    // UTF-8�� ���ڵ�
                    byte[] decodedBytes = System.Convert.FromBase64String(args.Data);
                    string decodedText = System.Text.Encoding.UTF8.GetString(decodedBytes);

                    UnityEngine.Debug.Log(decodedText);
                    GoogleTTS.Instance.RunTTS(decodedText);
                }
                else
                    UnityEngine.Debug.Log(args.Data);
            };
            //psi.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log(args.Data);
            //psi.OutputDataReceived += (sender, args) => GoogleTTS.Instance.RunTTS(args.Data);
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
