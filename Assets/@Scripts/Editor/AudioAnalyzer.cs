using UnityEngine;

public class AudioAnalyzer : MonoBehaviour
{
    public AudioClip audioClip;
    private float[] samples;
    private int sampleRate;

    void Start()
    {
        // ����� ������ �ε��մϴ�
        samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);
        sampleRate = audioClip.frequency;

        // FFT �м�
        AnalyzeAudio(samples);
    }

    void AnalyzeAudio(float[] samples)
    {
        int fftSize = 1024;
        float[] fftResult = new float[fftSize];
        for (int i = 0; i < samples.Length - fftSize; i += fftSize)
        {
            // FFT �м��� ���� ���� ����
            System.Array.Copy(samples, i, fftResult, 0, fftSize);

            // ���⼭ FFT �м��� �ϰ�, ������(Pitch)�� �����մϴ�.
            // ��: FFT �˰����� ���� ���ļ� �м�
            float pitch = GetPitchFromFFT(fftResult);
            Debug.Log("Pitch: " + pitch);
        }
    }

    float GetPitchFromFFT(float[] fftResult)
    {
        // FFT ������� �� ���� �����ϴ� �ڵ� ����
        // ��: �ִ� ���ļ� ���� ���� �߿��� �� ����
        return 0f;
    }
}
