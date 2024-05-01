using System.Collections;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;

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
        StartCoroutine(DownloadTheAudio(text, language));
    }

    //오디오를 다운로드 받습니다.
    private IEnumerator DownloadTheAudio(string text, SystemLanguage language = SystemLanguage.English)
    {
        mStrBuilder.Clear();

        //텍스트 앞 Origin URL
        mStrBuilder.Append(mPrefixURL);

        //TTS로 변환할 텍스트
        mStrBuilder.Append(text);
        mStrBuilder.Replace('\n', '.');

        //언어 인식을 위한 태그 추가 &tl=
        mStrBuilder.Append("&tl=");

        //언어 식별
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

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(mStrBuilder.ToString(), AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                mAudio.clip = DownloadHandlerAudioClip.GetContent(www);
                mAudio.Play();
            }
        }
    }
}
