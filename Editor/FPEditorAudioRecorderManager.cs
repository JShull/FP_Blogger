
namespace FuzzPhyte.Blogger.Editor
{
    using UnityEngine;
    using UnityEditor;

    [InitializeOnLoad]
    public static class FPEditorAudioRecorderManager
    {
        public static readonly FPEditorAudioRecorder Instance;

        static FPEditorAudioRecorderManager()
        {
            Instance = new FPEditorAudioRecorder();
            UnityEngine.Debug.Log("FPEditorAudioRecorder initialized and held in memory.");
        }
    }
}
