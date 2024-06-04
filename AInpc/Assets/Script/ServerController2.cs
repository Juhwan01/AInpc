using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class ServerController2 : MonoBehaviour
{
    // Singleton 패턴을 위한 static 인스턴스 변수
    private static ServerController2 instance;

    // Singleton 인스턴스를 반환하는 프로퍼티
    public static ServerController2 Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("ServerController2 instance is null. Make sure the script is attached to an active GameObject in the scene.");
            }
            return instance;
        }
    }

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // API의 기본 URL
    private string baseUrl = "http://127.0.0.1:8000";

    // 서버 요청에 사용될 DTO (Data Transfer Object) 클래스
    public class RequestDTO
    {
        public string question; // 서버에 전송될 질문 문자열
    }
    public class CheckDTO
    {
        public string question1; // 서버에 전송될 질문 문자열
        public string question2; // 서버에 전송될 질문 문자열
    }

    // 외부에서 질문을 전송하기 위한 메서드
    public void sendQuestion(string text)
    {
        if (text == null)
        {
            Debug.LogError("Text is null.");
            return;
        }

        StartCoroutine(ask("알고리즘이 뭐야?")); // 코루틴 호출하여 질문을 전송
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

                if (GoogleTTS.Instance != null)
                {
                    GoogleTTS.Instance.RunTTS(responseText); // 응답 텍스트를 음성으로 변환
                    StartCoroutine(checkScore(responseText,"주인공은 보경입니다"));
                }
                else
                {
                    Debug.LogError("GoogleTTS instance is null. Make sure it is properly initialized.");
                }
            }
        }
    }
    IEnumerator checkScore(string text1,string text2)
    {
        // DTO 객체 생성 및 JSON 직렬화
        CheckDTO checkDTO = new CheckDTO { question1 = text1, question2 = text2};
        string jsonData = JsonConvert.SerializeObject(checkDTO);

        // POST 요청을 생성
        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}/check", "POST"))
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
                Debug.Log(responseText);
            }
        }
    }
}
