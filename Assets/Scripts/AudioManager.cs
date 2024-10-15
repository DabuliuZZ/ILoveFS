using UnityEngine;
using Mirror;
using System.Collections;

public class AudioManager : NetworkBehaviour
{
    // 单例实例
    public static AudioManager Instance { get; private set; }

    // 音频源组件
    public AudioSource audioSource;

    // 在检查器中分配音频剪辑数组
    public AudioClip[] audioClips;

    // 背景音乐源组件
    public AudioSource bgmSource;

    // 在检查器中分配背景音乐剪辑数组
    public AudioClip[] bgmClips;

    // 淡入淡出时间
    public float fadeDuration = 1.0f;

    void Awake()
    {
        // 实现单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        // 如果需要在场景切换时保留
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(audioSource);
        DontDestroyOnLoad(bgmSource);
    }

    // 调用此方法在所有客户端播放音频，添加了音量参数
    public void PlayAudioClip(int clipIndex, float volume)
    {
        if (isServer)
        {
            // 如果是服务器，直接调用客户端Rpc
            RpcPlayAudioClip(clipIndex, volume);
        }
        else
        {
            // 如果是客户端，发送命令到服务器
            CmdPlayAudioClip(clipIndex, volume);
        }
    }

    // 本地播放音效
    public void PlayAudioClipLocal(int clipIndex, float volume)
    {
        if (audioSource != null && clipIndex >= 0 && clipIndex < audioClips.Length)
        {
            // 在本地播放音频，使用指定的音量
            audioSource.PlayOneShot(audioClips[clipIndex], volume);
        }
    }

    // 本地播放BGM，添加淡入淡出功能
    public void PlayBGMLocal(int bgmIndex, float volume)
    {
        if (bgmSource != null && bgmIndex >= 0 && bgmIndex < bgmClips.Length)
        {
            if (bgmSource.isPlaying)
            {
                // 如果当前有BGM在播放，执行淡出淡入
                StartCoroutine(FadeOutAndIn(bgmIndex, volume));
            }
            else
            {
                // 没有BGM在播放，直接淡入新BGM
                StartCoroutine(FadeIn(bgmIndex, volume));
            }
        }
    }

    // 客户端向服务器发送命令，添加了音量参数
    [Command(requiresAuthority = false)]
    void CmdPlayAudioClip(int clipIndex, float volume)
    {
        // 服务器接收到命令，调用客户端Rpc
        RpcPlayAudioClip(clipIndex, volume);
    }

    // 服务器向所有客户端广播，添加了音量参数
    [ClientRpc]
    void RpcPlayAudioClip(int clipIndex, float volume)
    {
        PlayAudioClipLocal(clipIndex, volume);
    }

    // 调用此方法在所有客户端播放背景音乐
    public void PlayBGM(int bgmIndex, float volume)
    {
        if (isServer)
        {
            RpcPlayBGM(bgmIndex, volume);
        }
        else
        {
            CmdPlayBGM(bgmIndex, volume);
        }
    }

    // 客户端向服务器发送播放BGM的命令
    [Command(requiresAuthority = false)]
    void CmdPlayBGM(int bgmIndex, float volume)
    {
        RpcPlayBGM(bgmIndex, volume);
    }

    // 服务器向所有客户端广播播放BGM
    [ClientRpc]
    void RpcPlayBGM(int bgmIndex, float volume)
    {
        PlayBGMLocal(bgmIndex, volume);
    }

    // 调用此方法在所有客户端停止背景音乐
    public void StopBGM()
    {
        if (isServer)
        {
            RpcStopBGM();
        }
        else
        {
            CmdStopBGM();
        }
    }

    // 客户端向服务器发送停止BGM的命令
    [Command(requiresAuthority = false)]
    void CmdStopBGM()
    {
        RpcStopBGM();
    }

    // 服务器向所有客户端广播停止BGM
    [ClientRpc]
    void RpcStopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            StartCoroutine(FadeOutAndStop());
        }
    }

    // 淡出当前BGM并淡入新的BGM
    IEnumerator FadeOutAndIn(int newBgmIndex, float newVolume)
    {
        float startVolume = bgmSource.volume;

        // 淡出
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }
        bgmSource.volume = 0;
        bgmSource.Stop();

        // 切换到新的BGM
        bgmSource.clip = bgmClips[newBgmIndex];
        bgmSource.Play();

        // 淡入
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0, newVolume, t / fadeDuration);
            yield return null;
        }
        bgmSource.volume = newVolume;
    }

    // 仅淡入新的BGM
    IEnumerator FadeIn(int bgmIndex, float volume)
    {
        bgmSource.clip = bgmClips[bgmIndex];
        bgmSource.volume = 0;
        bgmSource.loop = true;
        bgmSource.Play();

        // 淡入
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0, volume, t / fadeDuration);
            yield return null;
        }
        bgmSource.volume = volume;
    }

    // 淡出当前BGM并停止
    IEnumerator FadeOutAndStop()
    {
        float startVolume = bgmSource.volume;

        // 淡出
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }
        bgmSource.volume = 0;
        bgmSource.Stop();
    }
}
