using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

// Google Cloud Text-to-Speech API를 사용하는 클래스
public class GoogleCloudTTS : MonoBehaviour
{
    private static GoogleCloudTTS instance;
    private AudioSource audioSource;

    // Google Cloud Text-to-Speech API URL 및 API 키 (이 키를 발급받은 키로 대체해야 함)
    private string apiURL = "https://texttospeech.googleapis.com/v1/text:synthesize?key=AIzaSyDTtsY9MN8IHLJdwuTYjbJS06gd145E40g";

    // Singleton instance
    public static GoogleCloudTTS Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GoogleCloudTTS");
                instance = go.AddComponent<GoogleCloudTTS>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // 텍스트를 음성으로 변환하는 메서드
    public void RunTTS(string text, string languageCode = "en-US")
    {
        StartCoroutine(SendRequestToGoogleCloudTTS(text, languageCode));
    }

    // Google Cloud Text-to-Speech API에 요청을 보내는 코루틴
    private IEnumerator SendRequestToGoogleCloudTTS(string text, string languageCode)
    {
        // 요청 JSON 본문 구성
        var requestBody = new
        {
            input = new { text = text },
            voice = new { languageCode = languageCode, ssmlGender = "NEUTRAL" },
            audioConfig = new { audioEncoding = "LINEAR16" }
        };

        var jsonBody = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest www = new UnityWebRequest(apiURL, "POST"))
        {
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            // 요청 전송
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                // 응답으로부터 오디오 데이터 추출
                GoogleCloudTTSResponse response = JsonUtility.FromJson<GoogleCloudTTSResponse>(www.downloadHandler.text);
                byte[] audioData = Convert.FromBase64String(response.audioContent);
                PlayAudioClip(audioData);
            }
        }
    }

    // byte[] 데이터를 AudioClip으로 변환하고 재생
    private void PlayAudioClip(byte[] audioData)
    {
        // 오디오 데이터를 float 배열로 변환하는 사용자 정의 메서드가 필요합니다.
        // 아래는 예시 코드이며 실제로는 오디오 데이터에 맞는 처리가 필요합니다.
        float[] audioDataFloat = ConvertByteToFloat(audioData);
        AudioClip audioClip = AudioClip.Create("TTS Sound", audioDataFloat.Length, 1, 16000, false);
        audioClip.SetData(audioDataFloat, 0);
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    // byte 배열을 float 배열로 변환하는 메서드
    private float[] ConvertByteToFloat(byte[] byteArray)
    {
        float[] floatArr = new float[byteArray.Length / 2];

        for (int i = 0; i < floatArr.Length; i++)
        {
            // BitConverter를 사용하여 16비트 샘플로부터 하나의 float 값을 생성
            floatArr[i] = BitConverter.ToInt16(byteArray, i * 2) / 32768.0f;
        }

        return floatArr;
    }

    // Google Cloud Text-to-Speech API 응답을 위한 클래스
    [Serializable]
    private class GoogleCloudTTSResponse
    {
        public string audioContent; // 오디오 콘텐츠가 Base64 인코딩된 문자열로 반환됩니다.
    }
}

