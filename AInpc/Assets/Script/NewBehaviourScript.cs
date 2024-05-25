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
	// �޾ƿ� ���� �����ϰ� �����ϱ� ���� JSON ����
	[Serializable]
	public class VoiceRecognize
	{
		public string text;
	}

	// ����� ���(Kor)�� �� �ڿ� ����
	string url = "https://naveropenapi.apigw.ntruss.com/recog/v1/stt?lang=Kor";

	private IEnumerator PostVoice(string url, byte[] data)
	{
		// request ����
		WWWForm form = new WWWForm();
		UnityWebRequest request = UnityWebRequest.Post(url, form);

		// ��û ��� ����
		request.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", "q7z8866vrf");
		request.SetRequestHeader("X-NCP-APIGW-API-KEY", "0cYhSHD81OXuvKApkqQvx9u4sAesPKAZe9KxalTu");
		request.SetRequestHeader("Content-Type", "application/octet-stream");

		// �ٵ� ó�������� ��ģ Audio Clip data�� �Ǿ���
		request.uploadHandler = new UploadHandlerRaw(data);

		// ��û�� ���� �� response�� ���� ������ ���
		yield return request.SendWebRequest();

		// ���� response�� ����ִٸ� error
		if (request == null)
		{
			Debug.LogError(request.error);
		}
		else
		{
			// json ���·� ���� {"text":"�νİ��"}
			string message = request.downloadHandler.text;
			VoiceRecognize voiceRecognize = JsonUtility.FromJson<VoiceRecognize>(message);

			Debug.Log("Voice Server responded: " + voiceRecognize.text);
			GPTcustom.Instance.answer(voiceRecognize.text);
			// Voice Server responded: �νİ��
		}
	}

	// ��ư�� OnPointerDown �� �� ȣ��
	public void startRecording()
	{
		Debug.Log("start recording");
		_recording = Microphone.Start(_microphoneID, false, _recordingLengthSec, _recordingHZ);
	}
	// ��ư�� OnPointerUp �� �� ȣ��
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

			// ������ audioclip api ������ ����
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

		// audio clip�� �������� file stream�� �߰�(��ũ ���� �Լ� ����)
		WavUtility.WriteFileHeader(ref stream, fileSize);
		WavUtility.WriteFileFormat(ref stream, audioClip.channels, audioClip.frequency, bitDepth);
		WavUtility.WriteFileData(ref stream, audioClip, bitDepth);

		// stream�� array���·� �ٲ�
		byte[] bytes = stream.ToArray();

		return bytes;
	}

}


