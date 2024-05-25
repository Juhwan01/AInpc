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
        // GoogleTTS 클래스의 인스턴스를 가져와서 변수에 할당
        googleTTS = GoogleTTS.Instance;
    }
    public void answer(string text) 
    {
        // R 키가 눌렸을 때 실행
        try
        {
            Process psi = new Process();
            psi.StartInfo.FileName = "C:/Anaconda3/envs/llm/Scripts/ipython3.exe";
            // 가상환경의 Python 실행 파일 경로

            psi.StartInfo.Arguments = $"{Application.streamingAssetsPath}/AI-Assistant.py \"{text}\"";



            // 실행할 Python 파일 경로 및 입력 값 전달

            psi.StartInfo.CreateNoWindow = true;
            // 새로운 창을 생성하지 않음

            psi.StartInfo.UseShellExecute = false;
            // 셸을 사용하지 않음

            psi.StartInfo.RedirectStandardOutput = true;
            psi.StartInfo.RedirectStandardError = true;
            // 표준 출력 및 오류를 리다이렉트

            psi.OutputDataReceived += (sender, args) =>
            {
                // 받아온 데이터가 print문에 의한 것인지 확인
                if (!string.IsNullOrEmpty(args.Data) && !args.Data.StartsWith("Traceback") && !args.Data.StartsWith("File") && !args.Data.StartsWith("  "))
                {
                    // UTF-8로 디코딩
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

            UnityEngine.Debug.Log("[알림] .py 파일 실행");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("[알림] 에러 발생: " + e.Message);
        }
    }
}
