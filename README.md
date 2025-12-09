# Unity Procedural Audio Generator

A lightweight, pure C# script for generating real-time audio waveforms in Unity. This tool creates sound mathematically without requiring any external audio files (`.mp3`, `.wav`), acting as a basic software oscillator / synthesizer.

## 🎵 Features

*   **5 Waveform Types:** Sine, Square, Triangle, Sawtooth, and White Noise.
*   **Real-time Control:** Adjust Frequency (Pitch) and Gain (Volume) during runtime.
*   **Zero Assets:** No audio clips required; sound is generated via CPU.
*   **Stereo Support:** Automatically handles multi-channel output.

## 🚀 Installation & Setup

1.  **Create the Script:**
    *   In your Unity Project window, right-click and select **Create > C# Script**.
    *   Name the script `ProceduralAudioGenerator`.
2.  **Paste the Code:**
    *   Open the script and paste the provided C# code into it.
3.  **Scene Setup:**
    *   Create an empty GameObject in your scene (Right-click Hierarchy > **Create Empty**).
    *   Drag and drop the `ProceduralAudioGenerator` script onto this GameObject.
    *   *Note: An `AudioSource` component will be added automatically.*
4.  **Play:**
    *   Press the Unity **Play** button. You should hear a sine wave immediately.

## 🎛 Usage

Once the script is attached, you can control the sound via the **Inspector** window:

*   **Wave Type:** Select the shape of the sound wave.
    *   *Sine:* Smooth, pure tone (whistle-like).
    *   *Square:* Harsh, retro "8-bit" game sound.
    *   *Triangle:* Softer than square, brighter than sine (flute-like).
    *   *Sawtooth:* Sharp and buzzy (string/brass synthesis).
    *   *Noise:* Random static (TV snow/wind).
*   **Frequency (Hz):** Controls the pitch. Standard A4 is 440Hz.
*   **Gain (0-1):** Controls the volume.
    *   ⚠️ **Warning:** Square and Sawtooth waves are naturally louder and sharper than Sine waves. Keep the gain low (around 0.1 - 0.2) when testing to protect your ears and speakers.

## 🧠 Technical Principles

This script utilizes Unity's low-level audio callback: `OnAudioFilterRead`.

### 1. The Audio Thread
Unlike standard `Update()` loops which run once per frame (e.g., 60 times a second), `OnAudioFilterRead` runs on a dedicated audio thread. It is called whenever the audio system needs more data to send to the speakers.

### 2. Digital Signal Processing (DSP)
The function receives an empty array (`data`) that represents the audio buffer. We fill this array with mathematical values ranging from **-1.0 to 1.0**.

### 3. Phase Calculation
To generate a continuous pitch, we track the **Phase** of the wave.
*   **Sample Rate:** Usually 44,100 Hz or 48,000 Hz.
*   **Increment:** How much the phase advances per sample step.
    $$ \text{Increment} = \frac{\text{Frequency}}{\text{Sample Rate}} $$
*   **Phase Accumulation:** For every sample, we add the increment to the current phase. When the phase exceeds 1.0, we subtract 1.0 to loop it back to the start.

### 4. Waveform Mathematics
Based on the current `phase` (which is always between 0.0 and 1.0), we calculate the amplitude:

| Wave Type | Logic / Formula | Description |
| :--- | :--- | :--- |
| **Sine** | $\sin(Phase \times 2\pi)$ | Basic trigonometric oscillation. |
| **Square** | If $Phase < 0.5$ output $1$, else $-1$ | Binary state. High for half the cycle, low for half. |
| **Sawtooth** | $2 \times Phase - 1$ | Linear ramp from -1 to 1, then drops instantly. |
| **Triangle** | Linear interpolation | Ramps up from -1 to 1, then down from 1 to -1. |
| **Noise** | `Random.NextDouble()` | Ignores phase. Returns a random value for every sample. |

## ⚠️ Important Notes

1.  **Thread Safety:**
    The `OnAudioFilterRead` method runs on a dedicated audio thread, separate from Unity's main thread (where `Update` and `Start` run).
    *   **Implication:** Be careful when modifying variables from the main thread while the audio thread is reading them.
    *   **Best Practice:** For simple atomic types like `float` (frequency, gain) and `enums`, direct modification is usually safe for this type of script. However, if you expand this to use complex data structures (like Lists or Arrays that change size), you must use thread locking (`lock`) to prevent crashes or race conditions.

2.  **Spatial Blend (2D vs 3D):**
    The script sets `spatialBlend` to `0` (2D Sound) by default in the `Awake()` method.
    *   **2D Sound:** The sound plays at the same volume regardless of where the camera is located.
    *   **3D Sound:** If you want the sound to be positional (fading out as you move away), change the `spatialBlend` slider on the **AudioSource** component to `1` and adjust the **Min/Max Distance** settings.
