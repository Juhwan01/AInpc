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
            // R 키가 눌렸을 때 실행
            try
            {
                Process psi = new Process();
                psi.StartInfo.FileName = "C:/Anaconda3/envs/llm/python.exe";
                // 가상환경의 Python 실행 파일 경로

                psi.StartInfo.Arguments = Application.streamingAssetsPath + "/AI-Assistant.py";
                // 실행할 Python 파일 경로 (확장자 포함)

                psi.StartInfo.CreateNoWindow = true;
                // 새로운 창을 생성하지 않음

                psi.StartInfo.UseShellExecute = false;
                // 셸을 사용하지 않음

                psi.StartInfo.RedirectStandardOutput = true;
                psi.StartInfo.RedirectStandardError = true;
                // 표준 출력 및 오류를 리다이렉트

                psi.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log(args.Data);
                psi.ErrorDataReceived += (sender, args) => UnityEngine.Debug.LogError(args.Data);

                psi.Start();
                psi.BeginOutputReadLine();
                psi.BeginErrorReadLine();

                UnityEngine.Debug.Log("[알림] .py 파일 실행");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("[알림] 에러 발생: " + e.Message);
            }
        }
    }
}
