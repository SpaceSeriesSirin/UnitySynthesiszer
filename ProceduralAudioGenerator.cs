using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class ProceduralAudioGenerator : MonoBehaviour
{
    // 定义波形类型的枚举
    public enum WaveType
    {
        Sine,       // 正弦波
        Square,     // 方波
        Triangle,   // 三角波
        Sawtooth,   // 锯齿波
        Noise       // 噪音（白噪声）
    }

    [Header("Audio Settings")]
    [Tooltip("选择波形类型")]
    public WaveType waveType = WaveType.Sine;

    [Range(20f, 20000f)]
    [Tooltip("频率 (Hz)")]
    public float frequency = 440f; // 默认A4音高

    [Range(0f, 1f)]
    [Tooltip("音量增益 (0-1)")]
    public float gain = 0.1f; // 默认音量小一点，防止爆音

    // 内部变量用于波形计算
    private double phase;
    private double increment;
    private double sampleRate;
    private System.Random random;

    void Awake()
    {
        // 获取当前的采样率 (通常是 44100 或 48000)
        sampleRate = AudioSettings.outputSampleRate;
        random = new System.Random();
        
        // 确保AudioSource存在并播放，否则OnAudioFilterRead不会运行
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = true;
        audioSource.spatialBlend = 0; // 设为2D声音，方便测试
        audioSource.Stop(); // 停止播放任何AudioClip
        audioSource.Play(); // 播放（即使没有Clip，也会触发FilterRead）
    }

    /// <summary>
    /// Unity的音频处理回调函数。
    /// 注意：此函数运行在音频线程上，而非主线程。
    /// </summary>
    /// <param name="data">音频缓冲区数组（包含所有通道的数据）</param>
    /// <param name="channels">通道数（1=单声道, 2=立体声）</param>
    void OnAudioFilterRead(float[] data, int channels)
    {
        // 计算每个采样点的相位增量
        // 频率 / 采样率 = 每次采样需要前进的周期比例
        increment = frequency / sampleRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            // 更新相位
            phase += increment;
            
            // 保持相位在 0 到 1 之间循环，防止数值过大导致精度丢失
            if (phase > 1) phase -= 1;

            float value = 0f;

            // 根据选择的波形计算当前的振幅值 (-1 到 1)
            switch (waveType)
            {
                case WaveType.Sine:
                    // 正弦波: sin(2 * pi * phase)
                    value = (float)Math.Sin(phase * 2 * Math.PI);
                    break;

                case WaveType.Square:
                    // 方波: 前半周期为1，后半周期为-1
                    value = (phase < 0.5) ? 1f : -1f;
                    break;

                case WaveType.Triangle:
                    // 三角波: 线性上升再线性下降
                    // 这里的算法产生从 -1 到 1 的三角波
                    if (phase < 0.5)
                        value = (float)(4.0 * phase - 1.0);
                    else
                        value = (float)(-4.0 * (phase - 0.5) + 1.0);
                    break;

                case WaveType.Sawtooth:
                    // 锯齿波: 线性上升，然后瞬间掉落
                    // 从 -1 线性增加到 1
                    value = (float)(2.0 * phase - 1.0);
                    break;

                case WaveType.Noise:
                    // 噪音: -1 到 1 之间的随机数
                    value = (float)(random.NextDouble() * 2.0 - 1.0);
                    break;
            }

            // 应用音量增益
            value *= gain;

            // 将计算出的值填充到所有通道中 (例如左耳和右耳)
            // 如果你想做立体声平移，可以在这里分别设置 data[i] 和 data[i+1]
            for (int c = 0; c < channels; c++)
            {
                data[i + c] = value;
            }
        }
    }
}