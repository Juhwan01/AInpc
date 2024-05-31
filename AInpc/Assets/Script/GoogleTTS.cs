using System.Collections;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;
using System;

public class GoogleTTS : MonoBehaviour
{
    private static GoogleTTS instance;

    //TTS에서 사용할 오디오 소스
    private AudioSource mAudio;

    //문자열을 계속 바꾸기에 빌더를 사용한다.
    private StringBuilder mStrBuilder;

    //구글 TTS를 이용할 오리지널 앞 주소
    private string mPrefixURL = "https://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen=32&client=tw-ob&q=";

    // 다른 클래스에서 인스턴스를 생성하지 못하도록 private 생성자를 만듭니다.
    private GoogleTTS() { }

    // 외부에서 접근할 수 있는 인스턴스를 반환하는 정적 메서드를 만듭니다.
    public static GoogleTTS Instance
    {
        get
        {
            // 인스턴스가 없는 경우에만 생성합니다.
            if (instance == null)
            {
                // GameObject를 생성하여 GoogleTTS 컴포넌트를 추가하고, 인스턴스를 할당합니다.
                GameObject go = new GameObject("GoogleTTS");
                instance = go.AddComponent<GoogleTTS>();
                instance.Initialize();
                DontDestroyOnLoad(go); // Scene이 변경되어도 유지되도록 설정합니다.
            }
            return instance;
        }
    }

    // 초기화 메서드
    private void Initialize()
    {
        mAudio = gameObject.AddComponent<AudioSource>();
        mStrBuilder = new StringBuilder();
    }

    //외부에서 호출되며 문자열, 언어를 받아 코루틴을 실행시킵니다.
    public void RunTTS(string text, SystemLanguage language = SystemLanguage.English)
    {
        Debug.Log(text);
        StartCoroutine(DownloadTheAudio(text, language));
    }

    //오디오를 다운로드 받습니다.
    private IEnumerator DownloadTheAudio(string text, SystemLanguage language = SystemLanguage.English)
    {
        const int maxLength = 100;  // Google TTS에서 허용하는 최대 길이
        int currentIndex = 0;       // 현재 처리 중인 텍스트 인덱스

        while (currentIndex < text.Length)
        {
            // 처리할 텍스트 조각을 계산합니다.
            int length = Math.Min(maxLength, text.Length - currentIndex);
            string partText = text.Substring(currentIndex, length);
            currentIndex += length;

            // 현재 조각에 대한 오디오 다운로드 및 재생을 요청합니다.
            yield return StartCoroutine(DownloadAndPlayAudio(partText, language));

            // 다음 오디오가 재생되기 전에 현재 오디오 재생이 끝날 때까지 대기합니다.
            while (mAudio.isPlaying)
            {
                yield return null;
            }
        }
    }

    private IEnumerator DownloadAndPlayAudio(string text, SystemLanguage language)
    {
        mStrBuilder.Clear();

        // 접두사 URL을 추가합니다.
        mStrBuilder.Append(mPrefixURL);

        // TTS로 변환할 텍스트를 URL 인코딩합니다.
        mStrBuilder.Append(WWW.EscapeURL(text));
        mStrBuilder.Replace('\n', '.');

        // 언어 태그를 추가합니다.
        mStrBuilder.Append("&tl=");

        // 언어 코드를 식별하고 추가합니다.
        switch (language)
        {
            case SystemLanguage.Korean:
                mStrBuilder.Append("ko-KR");
                break;
            case SystemLanguage.English:
            default:
                mStrBuilder.Append("en-GB");
                break;
        }

        // UnityWebRequest를 사용하여 오디오 클립을 가져옵니다.
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(mStrBuilder.ToString(), AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            // 연결 오류를 확인합니다.
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // 오디오 클립을 가져오고 재생을 시도합니다.
                try
                {
                    mAudio.clip = DownloadHandlerAudioClip.GetContent(www);
                    mAudio.Play();
                }
                catch (Exception ex)
                {
                    Debug.LogError("오디오 클립 처리 중 오류 발생: " + ex.Message);
                }
            }
        }
    }


}
