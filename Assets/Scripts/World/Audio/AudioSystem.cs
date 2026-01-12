using Addrs;
using Foundations;
using UnityEngine;

namespace World.Audio
{
    public class AudioSystem : MonoBehaviourSingleton<AudioSystem>
    {
        public AudioSource[] audioSources;

        public void PlayBGM(AudioClip clip)
        {
            audioSources[0].clip = clip;
            audioSources[0].Play();
        }

        public void PlayBGM(string path)
        {
            PlayBGM(GetClip(path));
        }

        public void PlayClip(string path,bool is_loop)
        {
            for(int i = 1; i < audioSources.Length; i++)
            {
                if (audioSources[i].isPlaying)
                {
                    continue;
                }

                audioSources[i].clip = GetClip(path);
                audioSources[i].Play();
                audioSources[i].loop = is_loop;

                break;
            }
        }

        public void SetClipPitch(string path,float pitch)
        {
            for (int i = 1; i < audioSources.Length; i++)
            {
                if (!audioSources[i].isPlaying)
                    continue;

                if (audioSources[i].clip.name == GetClip(path).name)
                {
                    audioSources[i].pitch = pitch;
                }
            }
        }

        public void StopClip(string path)
        {
            for (int i = 1; i < audioSources.Length; i++)
            {
                if (!audioSources[i].isPlaying)
                    continue;

                if (audioSources[i].clip.name == GetClip(path).name)
                {
                    audioSources[i].Stop();
                    audioSources[i].pitch = 1;
                }
            }
        }

        public void PlayOneShot(string path)
        {
            if (path == null)
                return;
            PlayOneShot(GetClip(path));
        }

        public void PlayOneShot(AudioClip clip)
        {
            if (clip == null)
                return;

            audioSources[0].PlayOneShot(clip);
        }

        public AudioClip GetClip(string path)
        {
            Addressable_Utility.try_load_asset<AudioClip>(path, out var result);
            if(result == null)
            {
                Debug.LogWarning($"{path} 路径的音频为空");
            }
            return result;
        }
    }
}
