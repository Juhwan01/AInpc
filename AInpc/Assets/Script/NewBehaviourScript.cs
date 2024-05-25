using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;



public class NewBehaviourScript : MonoBehaviour
{
	private string _microphoneID = null;
	private AudioClip _recording = null;
	private int _recordingLengthSec = 15;
	private int _recordingHZ = 22050;

	private void Start()
	{
		_microphoneID = Microphone.devices[0];
	}
	// 받아온 값에 간편하게 접근하기 위한 JSON 선언
	[Serializable]
	public class VoiceRecognize
	{
		public string text;
	}

	// 사용할 언어(Kor)를 맨 뒤에 붙임
	string url = "https://naveropenapi.apigw.ntruss.com/recog/v1/stt?lang=Kor";

	private IEnumerator PostVoice(string url, byte[] data)
	{
		// request 생성
		WWWForm form = new WWWForm();
		UnityWebRequest request = UnityWebRequest.Post(url, form);

		// 요청 헤더 설정
		request.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", "q7z8866vrf");
		request.SetRequestHeader("X-NCP-APIGW-API-KEY", "0cYhSHD81OXuvKApkqQvx9u4sAesPKAZe9KxalTu");
		request.SetRequestHeader("Content-Type", "application/octet-stream");

		// 바디에 처리과정을 거친 Audio Clip data를 실어줌
		request.uploadHandler = new UploadHandlerRaw(data);

		// 요청을 보낸 후 response를 받을 때까지 대기
		yield return request.SendWebRequest();

		// 만약 response가 비어있다면 error
		if (request == null)
		{
			Debug.LogError(request.error);
		}
		else
		{
			// json 형태로 받음 {"text":"인식결과"}
			string message = request.downloadHandler.text;
			VoiceRecognize voiceRecognize = JsonUtility.FromJson<VoiceRecognize>(message);

			Debug.Log("Voice Server responded: " + voiceRecognize.text);
			GPTcustom.Instance.answer(voiceRecognize.text);
			// Voice Server responded: 인식결과
		}
	}

	// 버튼을 OnPointerDown 할 때 호출
	public void startRecording()
	{
		Debug.Log("start recording");
		_recording = Microphone.Start(_microphoneID, false, _recordingLengthSec, _recordingHZ);
	}
	// 버튼을 OnPointerUp 할 때 호출
	public void stopRecording()
	{
		if (Microphone.IsRecording(_microphoneID))
		{
			Microphone.End(_microphoneID);

			Debug.Log("stop recording");
			if (_recording == null)
			{
				Debug.LogError("nothing recorded");
				return;
			}
			// audio clip to byte array
			byte[] byteData = getByteFromAudioClip(_recording);

			// 녹음된 audioclip api 서버로 보냄
			StartCoroutine(PostVoice(url, byteData));
		}
		return;
	}
	private byte[] getByteFromAudioClip(AudioClip audioClip)
	{
		MemoryStream stream = new MemoryStream();
		const int headerSize = 44;
		ushort bitDepth = 16;

		int fileSize = audioClip.samples * WavUtility.BlockSize_16Bit + headerSize;

		// audio clip의 정보들을 file stream에 추가(링크 참고 함수 선언)
		WavUtility.WriteFileHeader(ref stream, fileSize);
		WavUtility.WriteFileFormat(ref stream, audioClip.channels, audioClip.frequency, bitDepth);
		WavUtility.WriteFileData(ref stream, audioClip, bitDepth);

		// stream을 array형태로 바꿈
		byte[] bytes = stream.ToArray();

		return bytes;
	}

}


