#if UNITY_EDITOR
using UnityEditor;
#endif

using CoolTools.Attributes;
using UnityEngine;

namespace CoolTools.Data
{
    [CreateAssetMenu(menuName = "Game/Audio Event")]
    public class AudioInstance : ScriptableObject
    {
        [SerializeField] private AudioClip[] _clips;
        [SerializeField] private HideFlags _hideFlags;
        
        [IntervalVector2(0f, 1f)] 
        [SerializeField] private Vector2 _volume = new (0.75f, 1f);

        [IntervalVector2(0.1f, 3f)] 
        [SerializeField] private Vector2 _pitch = new(0.9f, 1.1f);

        private AudioSource _source;
        
        public void Play(AudioSource source)
        {
            if (_clips.Length == 0) return;
            
            var clip = _clips[Random.Range(0, _clips.Length)];
            
            source.clip = clip;
            source.volume = Random.Range(_volume.x, _volume.y);
            source.pitch = Random.Range(_pitch.x, _pitch.y);
            
            source.clip = clip;
            source.Play();
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(AudioInstance))]
    public class SoundProfileEditor : Editor
    {
        private AudioSource _previewSource;
        
        private void OnEnable()
        {
            _previewSource = EditorUtility
                .CreateGameObjectWithHideFlags("Preview", HideFlags.None, typeof(AudioSource)).GetComponent<AudioSource>();
        }
        
        private void OnDisable()
        {
            DestroyImmediate(_previewSource.gameObject);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var profile = (AudioInstance) target;
            if (GUILayout.Button("Play"))
            {
                // Find the AudioListener
                // var listener = FindAnyObjectByType<AudioListener>();
                profile.Play(_previewSource);
            }
        }
    }
#endif
}