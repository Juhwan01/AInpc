using System.Collections;
using WebSocketSharp;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;

public class ServerController2 : MonoBehaviour
{
    // Singleton 패턴을 위한 static 인스턴스 변수
    private static ServerController2 instance = null;

    // Singleton 인스턴스를 반환하는 프로퍼티
    public static ServerController2 Instance
    {
        get
        {
            // 인스턴스가 null이면 새로운 인스턴스를 생성
            if (instance == null)
            {
                instance = new ServerController2();
            }
            return instance;
        }
    }

    // API의 기본 URL
    private string baseUrl = "http://127.0.0.1:8000";

    // 서버 요청에 사용될 DTO (Data Transfer Object) 클래스
    public class RequestDTO
    {
        public string question; // 서버에 전송될 질문 문자열
    }

    // 외부에서 질문을 전송하기 위한 메서드
    public void sendQuestion(string text)
    {
        StartCoroutine(ask(text)); // 코루틴 호출하여 질문을 전송
    }

    // 질문을 서버에 전송하는 코루틴
    IEnumerator ask(string text)
    {
        // DTO 객체 생성 및 JSON 직렬화
        RequestDTO requestDTO = new RequestDTO { question = text };
        string jsonData = JsonConvert.SerializeObject(requestDTO);

        // POST 요청을 생성
        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}/request", "POST"))
        {
            // JSON 데이터를 바이트 배열로 변환
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

            // 요청의 업로드 핸들러와 다운로드 핸들러 설정
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 요청 헤더 설정
            request.SetRequestHeader("Content-Type", "application/json");

            // 요청을 전송하고 응답을 기다림
            yield return request.SendWebRequest();

            // 요청 결과에 따라 처리
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                // 오류 발생 시 오류 메시지 출력
                Debug.LogError($"Error: {request.error}");
            }
            else
            {
                // 응답 데이터 처리
                string responseText = request.downloadHandler.text;
                GoogleTTS.Instance.RunTTS(responseText); // 응답 텍스트를 음성으로 변환
            }
        }
    }
}
