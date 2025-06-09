namespace FuzzPhyte.Blogger.Editor
{
    using UnityEngine;
    using System.IO;
    using FuzzPhyte.Utility;
    public class FPEditorAudioRecorder
    {
        private AudioClip clip;
        private int sampleRate = 44100;
        public string SelectedDevice { get; set; } = null;
        public  bool IsRecording => SelectedDevice != null && Microphone.IsRecording(SelectedDevice);

        public AudioClip StartRecording(int lengthSec = 300)
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.LogWarning("No microphone detected.");
                return null ;
            }
            if (string.IsNullOrEmpty(SelectedDevice))
            {
                SelectedDevice = Microphone.devices[0];
            }
           
            clip = Microphone.Start(SelectedDevice, false, lengthSec, sampleRate);
            return clip;
        }

        public  void StopAndSave(string filePath)
        {
            if (clip == null || !Microphone.IsRecording(SelectedDevice))
            {
                Debug.LogError($"Clip was null or the microphone status: {Microphone.IsRecording(SelectedDevice)} was false");
                return;
            }

            Microphone.End(SelectedDevice);
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Debug.LogWarning($"Creating a directory at: {directory}");
            }
            //var newClip = FP_SavWav.TrimSilence(clip, roundedMinutes);
            FP_SavWav.Save(filePath, clip); //save trimmed
            clip = null;

            Debug.Log($"🎤 Saved: {filePath}");
        }

    }
}
