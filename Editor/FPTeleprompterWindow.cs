namespace FuzzPhyte.Blogger.Editor
{
    using System.Collections.Generic;
    using FuzzPhyte.Blogger;
    using FuzzPhyte.Utility.Editor;
    using UnityEditor;
    using UnityEngine;
    using System.IO;
    using System;
    public class FPTeleprompterWindow:EditorWindow
    {
        private List<TeleprompterSection> sections;
        private int currentIndex;
        private Vector2 scrollPos;
        private int fontSize = 16;
        private Color textColor = Color.white;
        private Color backgroundColor = Color.black;

        private GUIStyle bodyStyle;
        private GUIStyle titleStyle;
        private GUIStyle subsectionStyle;
        private GUIStyle categoryStyle;

        private DateTime sectionStartTime;
        private DateTime overallStartTime;
        private DateTime audioStartTime;
        private TimeSpan audioElapsed => DateTime.Now - audioStartTime;
        
        private TimeSpan totalPausedDuration = TimeSpan.Zero;
        private DateTime pauseStartTime;
        private bool overallRunning = false;
        private bool isWindowFocused = true;
        private bool isRecording = false;
        private FPEditorAudioRecorder audioRecorder=>FPEditorAudioRecorderManager.Instance;
        [MenuItem("FuzzPhyte/Blogger/Teleprompter")]
        public static void ShowWindow() => GetWindow<FPTeleprompterWindow>("Teleprompter");

        private void OnEnable()
        {
            InitStyles();
            EditorApplication.update += UpdateTimers;
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateTimers;
        }

        private void OnFocus()
        {
            isWindowFocused = true;
        }

        private void OnLostFocus()
        {
            isWindowFocused = false;
        }

        private void InitStyles()
        {
            var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

            bodyStyle = new GUIStyle(skin.label)
            {
                wordWrap = true,
                fontSize = fontSize,
                normal = { textColor = textColor },
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(4, 4, 4, 4)
            };

            titleStyle = new GUIStyle(skin.label)
            {
                wordWrap = true,
                fontSize = fontSize + 8,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textColor }
                
            };

            subsectionStyle = new GUIStyle(skin.label)
            {
                wordWrap = true,
                fontSize = fontSize + 4,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textColor }
                
            };

            categoryStyle = new GUIStyle(skin.label)
            {
                wordWrap = true,
                fontSize = fontSize + 2,
                fontStyle = FontStyle.Italic,
                normal = { textColor = textColor }
            };
        }

        private void UpdateTimers()
        {
            if (!isWindowFocused || !overallRunning) return;
            Repaint();
        }

        private void OnGUI()
        {
            GUILayout.Label("🎙 Select Microphone", EditorStyles.boldLabel);
            #region Microphone Selection
            string[] devices = Microphone.devices;
            int selectedDeviceIndex = Array.IndexOf(devices, audioRecorder.SelectedDevice);
            if (selectedDeviceIndex < 0) selectedDeviceIndex = 0;

            int newIndex = EditorGUILayout.Popup("Microphone", selectedDeviceIndex, devices);
            if (newIndex != selectedDeviceIndex)
            {
                audioRecorder.SelectedDevice = devices[newIndex];
            }
            FP_Utility_Editor.DrawUILine(isRecording ? Color.red : FP_Utility_Editor.TextActiveColor);
            #endregion
            GUILayout.Space(4);
            GUILayout.Label("🎤 Audio Recorder", EditorStyles.boldLabel);
            
            #region Recording Layout
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = isRecording ? Color.red : GUI.backgroundColor;
            if (GUILayout.Button("Start Recording")&&!isRecording)
            {
                audioRecorder.StartRecording();
                audioStartTime = DateTime.Now;
                isRecording = true;
                GUI.backgroundColor = Color.white;
            }
            
            if (GUILayout.Button("Stop & Save"))
            {
                string outputPath = Path.Combine(Application.dataPath, "TeleprompterRecordings", $"Section_{currentIndex}_{DateTime.Now:yyyyMMdd_HHmmss}.wav");
                var elapsedTime = (float)audioElapsed.TotalSeconds*0.0166667f;
                audioRecorder.StopAndSave(outputPath);
                AssetDatabase.Refresh(); // Refresh the asset database if saved inside Assets
                isRecording = false;
            }
            EditorGUILayout.EndHorizontal();
            #endregion
            GUILayout.Space(8);
            FP_Utility_Editor.DrawUILine(isRecording ? Color.red : FP_Utility_Editor.TextActiveColor);
            #region UI Body Text Updates
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("A+", GUILayout.Width(40)))
            {
                fontSize = Mathf.Min(fontSize + 2, 40);
                InitStyles();
            }
            if (GUILayout.Button("A-", GUILayout.Width(40)))
            {
                fontSize = Mathf.Max(fontSize - 2, 8);
                InitStyles();
            }
            textColor = EditorGUILayout.ColorField("Text Color", textColor);
            backgroundColor = EditorGUILayout.ColorField("BG Color", backgroundColor);
            InitStyles();
            EditorGUILayout.EndHorizontal();
            #endregion
            #region Markdown Text Body
            
            if (GUILayout.Button("Load Markdown File"))
            {
                string path = EditorUtility.OpenFilePanel("Select Markdown File", "", "md");
                if (!string.IsNullOrEmpty(path))
                {
                    string content = File.ReadAllText(path);
                    sections = FPMarkdownParser.ParseSections(content);
                    currentIndex = 0;
                    sectionStartTime = DateTime.Now;
                    overallStartTime = DateTime.Now;
                    totalPausedDuration = TimeSpan.Zero;
                    overallRunning = true;
                }
            }

            if (sections == null || sections.Count == 0)
            {
                EditorGUILayout.HelpBox("No content loaded.", MessageType.Info);
                return;
            }
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            #region Teleprompt Body
            Rect textRect = EditorGUILayout.BeginVertical();
            EditorGUI.DrawRect(textRect, backgroundColor);
            GUILayout.Space(8);

            EditorGUILayout.LabelField(sections[currentIndex].Title, titleStyle);

            if (!string.IsNullOrWhiteSpace(sections[currentIndex].Subsection))
            {
                GUILayout.Label(sections[currentIndex].Subsection, subsectionStyle);
                GUILayout.Space(6);
            }
                
            if (!string.IsNullOrWhiteSpace(sections[currentIndex].Category))
            {
                GUILayout.Label(sections[currentIndex].Category, categoryStyle);
                GUILayout.Space(4);
            }
            GUILayout.Space(8);
            EditorGUILayout.LabelField(sections[currentIndex].Body, bodyStyle);
                        
            EditorGUILayout.EndVertical();
            #endregion
            EditorGUILayout.EndScrollView();
            #endregion
            #region Proggress Bar & Timers
            float progress = (sections.Count > 1) ? (float)(currentIndex + 1) / sections.Count : 1f;
            Rect progressRect = GUILayoutUtility.GetRect(18, 18, "TextField");
            EditorGUI.ProgressBar(progressRect, progress, $"Progress: {currentIndex + 1}/{sections.Count}");
            GUILayout.Space(10);

            TimeSpan sectionElapsed = DateTime.Now - sectionStartTime;
            TimeSpan overallElapsed = overallRunning ? (DateTime.Now - overallStartTime - totalPausedDuration) : (pauseStartTime - overallStartTime - totalPausedDuration);

            EditorGUILayout.LabelField("Section Time:", FormatTime(sectionElapsed));
            EditorGUILayout.LabelField("Overall Time:", FormatTime(overallElapsed));
            #endregion
            #region Timer UI
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start"))
            {
                if (!overallRunning)
                {
                    totalPausedDuration += DateTime.Now - pauseStartTime;
                    overallRunning = true;
                }
            }
            if (GUILayout.Button("Pause"))
            {
                if (overallRunning)
                {
                    pauseStartTime = DateTime.Now;
                    overallRunning = false;
                }
            }
            if (GUILayout.Button("Reset"))
            {
                overallStartTime = DateTime.Now;
                sectionStartTime = DateTime.Now;
                totalPausedDuration = TimeSpan.Zero;
                overallRunning = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = currentIndex > 0;
            if (GUILayout.Button("Previous"))
            {
                currentIndex--;
                sectionStartTime = DateTime.Now;
            }
            GUI.enabled = currentIndex < sections.Count - 1;
            if (GUILayout.Button("Next"))
            {
                currentIndex++;
                sectionStartTime = DateTime.Now;
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            if (isRecording)
            {
                EditorGUILayout.LabelField("Audio Time:", FormatTime(audioElapsed));

                if (audioElapsed.TotalMinutes >= 5)
                {
                    string outputPath = Path.Combine(Application.dataPath, "TeleprompterRecordings", $"Section_{currentIndex}_{DateTime.Now:yyyyMMdd_HHmmss}.wav");
                    int roundedMinutes = Mathf.Max(1, Mathf.RoundToInt((float)audioElapsed.TotalMinutes));
                    audioRecorder.StopAndSave(outputPath);
                    AssetDatabase.Refresh();
                    isRecording = false;
                }
            }
            #endregion
        }

        private string FormatTime(TimeSpan time)
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);
        }
    }
}
