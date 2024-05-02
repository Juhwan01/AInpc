using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

// Google Cloud Text-to-Speech API�� ����ϴ� Ŭ����
public class GoogleCloudTTS : MonoBehaviour
{
    private static GoogleCloudTTS instance;
    private AudioSource audioSource;

    // Google Cloud Text-to-Speech API URL �� API Ű (�� Ű�� �߱޹��� Ű�� ��ü�ؾ� ��)
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

    // �ؽ�Ʈ�� �������� ��ȯ�ϴ� �޼���
    public void RunTTS(string text, string languageCode = "en-US")
    {
        StartCoroutine(SendRequestToGoogleCloudTTS(text, languageCode));
    }

    // Google Cloud Text-to-Speech API�� ��û�� ������ �ڷ�ƾ
    private IEnumerator SendRequestToGoogleCloudTTS(string text, string languageCode)
    {
        // ��û JSON ���� ����
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

            // ��û ����
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                // �������κ��� ����� ������ ����
                GoogleCloudTTSResponse response = JsonUtility.FromJson<GoogleCloudTTSResponse>(www.downloadHandler.text);
                byte[] audioData = Convert.FromBase64String(response.audioContent);
                PlayAudioClip(audioData);
            }
        }
    }

    // byte[] �����͸� AudioClip���� ��ȯ�ϰ� ���
    private void PlayAudioClip(byte[] audioData)
    {
        // ����� �����͸� float �迭�� ��ȯ�ϴ� ����� ���� �޼��尡 �ʿ��մϴ�.
        // �Ʒ��� ���� �ڵ��̸� �����δ� ����� �����Ϳ� �´� ó���� �ʿ��մϴ�.
        float[] audioDataFloat = ConvertByteToFloat(audioData);
        AudioClip audioClip = AudioClip.Create("TTS Sound", audioDataFloat.Length, 1, 16000, false);
        audioClip.SetData(audioDataFloat, 0);
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    // byte �迭�� float �迭�� ��ȯ�ϴ� �޼���
    private float[] ConvertByteToFloat(byte[] byteArray)
    {
        float[] floatArr = new float[byteArray.Length / 2];

        for (int i = 0; i < floatArr.Length; i++)
        {
            // BitConverter�� ����Ͽ� 16��Ʈ ���÷κ��� �ϳ��� float ���� ����
            floatArr[i] = BitConverter.ToInt16(byteArray, i * 2) / 32768.0f;
        }

        return floatArr;
    }

    // Google Cloud Text-to-Speech API ������ ���� Ŭ����
    [Serializable]
    private class GoogleCloudTTSResponse
    {
        public string audioContent; // ����� �������� Base64 ���ڵ��� ���ڿ��� ��ȯ�˴ϴ�.
    }
}

