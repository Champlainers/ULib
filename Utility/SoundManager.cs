/*******************************************************
 * Copywrite 2011 Not So Indie Games
 * Orginal Author: Tyler Steele
 * Contact Email: Tyler@ShadesofGames.com
 * 
 * 
 * 
 * Build player: 
 *******************************************************/

using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;


public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;

    public static SoundManager Instance
    {
        get { return instance ?? (instance = new GameObject(typeof(SoundManager).ToString()).AddComponent<SoundManager>()); }
    }
    #region Public Members
    private Dictionary<string, AudioClip>  musicFiles;

    /// <summary>
    /// This is a sound library you can load sounds into and unload them as well. 
    /// The library created for the Effects folder is called default and will auto matically be used. 
    /// </summary>
    private Dictionary<string, Dictionary<string, AudioClip>> soundLibrary; 

    public int NumSoundChannels, 
               NumMusicChannels;

    public string SoundDirectory;
    #endregion

    #region Private Members
    private AudioSource[] soundChannels,
                          musicChannels;

    private bool initilized;
    #endregion

    public void Initilize(bool loadOnStart, bool dontDestroy, int numMusicChannels = 2, int numSoundChannels = 6, string soundDirectory = "Sounds")
    {
        if (initilized)
        {
            Debug.LogWarning("Sound Manager already initilized you cannot have more than one sound manager in the scene");
            return;
        }

        initilized = true;
        soundLibrary = new Dictionary<string, Dictionary<string, AudioClip>>();
        musicFiles = new Dictionary<string, AudioClip>();
        NumMusicChannels = numMusicChannels;
        NumSoundChannels = numSoundChannels;
        SoundDirectory = soundDirectory;
        soundChannels = new AudioSource[NumSoundChannels];
        musicChannels = new AudioSource[NumMusicChannels]; 


        for (int i = 0; i < NumSoundChannels; i++)
            soundChannels[i] = gameObject.AddComponent<AudioSource>();

        for (int i = 0; i < NumMusicChannels; i++)
            musicChannels[i] = gameObject.AddComponent<AudioSource>();

        if (dontDestroy)
            DontDestroyOnLoad(this);

        //if (!loadOnStart) return;

        LoadSounds();
        LoadMusic();
    }


    #region Load Functions

    private void LoadSounds(string Library = "default", string Directory = "Effects")
    {
        
        Object[] sound = Resources.LoadAll(SoundDirectory + "/" + Directory);
        soundLibrary.Add(Library,new Dictionary<string, AudioClip>());
        foreach (Object o in sound)
        {
           
            soundLibrary[Library].Add(o.name.ToLower(), o as AudioClip);
        }

        
    }

    private void LoadMusic()
    {
        Object[] music = Resources.LoadAll(SoundDirectory + "/Music");
        Debug.Log("Loading: " + music.Length + " files");
        foreach (Object o in music)
        {
          
            musicFiles.Add(o.name.ToLower(), o as AudioClip);

        }

    }


    #endregion

    #region RemoveFunctions

    public void RemoveSoundLibrary(string libraryName)
    {
        if(soundLibrary.ContainsKey(libraryName))
            soundLibrary.Remove(libraryName);
        else
            Debug.LogError("No Library found with name: " + libraryName);
        
    }

    #endregion

    #region Helpers

    private AudioSource GetFirstUnusedSoundChannel()
    {
        foreach (var sc in soundChannels.Where(sc => !sc.isPlaying))
            return sc;
      
        Debug.LogError("No Useable Channels found");
        return null;
    }

    private AudioSource GetFirstUnusedMusicChannel()
    {
        foreach (var sc in musicChannels.Where(sc => !sc.isPlaying))
            return sc;

        Debug.LogError("No Useable Channels found");
        return null;
    }

    private AudioSource GetMusicChannelPlaying(string clipName)
    {
        foreach (var sc in musicChannels.Where(sc => sc.isPlaying && sc.name == clipName))
            return sc;

        Debug.LogError("No Useable Channels found");
        return null;
    }

    private AudioSource[] GetMusicChannelsPlaying()
    {
        List<AudioSource> channelsPlaying = new List<AudioSource>();
        channelsPlaying.AddRange(musicChannels.Where(sc => sc.isPlaying));

        return channelsPlaying.ToArray();
       
    }

    private AudioSource GetMusicChannelPlaying()
    {
        foreach (var sc in musicChannels.Where(sc => sc.isPlaying))
            return sc;

        Debug.LogError("No Useable Channels found");
        return null;
    }
    #endregion

    #region Fades
    private void StopOnFadeOut(AudioSource source)
    {
        Debug.Log("Stopping Playback of: " + source.clip.name);
        source.Stop();
        source.clip = null;
    }

    private static void FadeAudioSourceTo(ref AudioSource source, float time, float fadeTo)
    {
        iTween.AudioTo(source.gameObject,iTween.Hash("audiosource",source,"time",time, "volume",fadeTo,"easetype",iTween.EaseType.linear));
    }

    private static void FadeAudioSourceOut(ref AudioSource source, float time, float fadeTo)
    {
        iTween.AudioTo(source.gameObject, iTween.Hash("audiosource", source, "time", time, "volume", fadeTo, "easetype", iTween.EaseType.linear, "oncomplete", "StopOnFadeOut", "oncompleteparams", source, "oncompletetarget", source.gameObject));
    }

    private static void CrossFadeChannels(AudioSource to,AudioSource from, float time, float maxVolume, float minVolume)
    {
        FadeAudioSourceTo(ref to, time, maxVolume);
        FadeAudioSourceOut(ref from, time, minVolume);
    }
#endregion

    #region Sounds
    public static void PlaySound(string name, string library = "default")
    {
        try
        {
            Instance.GetFirstUnusedSoundChannel().PlayOneShot(Instance.soundLibrary[library.ToLower()][name.ToLower()]);       
        }
        catch (Exception e)
        {
            
           Debug.LogError("There was an error playing sound " + name);
            Debug.Log(e.GetType() + " Error  Message: " +e.Message);
        }
       
    }

    public static void PlaySound(string name, float volume, string library = "default")
    {
        Debug.Log(Instance);
        try
        {
            Instance.GetFirstUnusedSoundChannel().PlayOneShot(Instance.soundLibrary[library.ToLower()][name.ToLower()], volume);
        }
        catch (Exception e)
        {

            Debug.LogError("There was an error playing sound " + name);
            Debug.Log(e.GetType() + " Error  Message: " + e.Message);
        }

    }
#endregion

    #region Music
    public static void PlayMusic(string name)
    {
        //if (Instance.musicChannels.Any(musicChannel => musicChannel.clip != null && musicChannel.clip.name == name))return; 
        AudioSource scr = Instance.GetFirstUnusedMusicChannel();
        scr.clip = Instance.musicFiles[name.ToLower()];
        scr.Play();
       
    }

    public static void PauseMusic(string name)
    {
        AudioSource scr = Instance.GetFirstUnusedMusicChannel();
        scr.clip = Instance.musicFiles[name.ToLower()];
        scr.Pause();
    }

    public static void PlayMusic(string name,float volume)
    {
        //if (Instance.musicChannels.Any(musicChannel => musicChannel.clip != null && musicChannel.clip.name == name)) return;


        AudioSource scr = Instance.GetFirstUnusedMusicChannel();
        scr.clip = Instance.musicFiles[name.ToLower()];
        scr.volume = volume;
        scr.Play();
    }

    public static void PlayMusicCrossFade(string name, float volume,float time)
    {
       // if (Instance.musicChannels.Any(musicChannel => musicChannel.clip != null && musicChannel.clip.name == name)) return;

        AudioSource scr = Instance.GetFirstUnusedMusicChannel();
        if(scr == null)return;
        scr.clip = Instance.musicFiles[name.ToLower()];
        scr.volume = .25f;
       
        AudioSource crossFadeTarget = Instance.GetMusicChannelPlaying(); 
        scr.Play();
        if (crossFadeTarget != null)
        {
            Debug.Log("CrossFading From" + crossFadeTarget.clip.name + " To " + scr.name);
            CrossFadeChannels(scr, crossFadeTarget, time, volume, 0.0f);
        }
        else
        {
            Debug.Log("Fading " + scr.clip.name + " To " + volume);
            FadeAudioSourceTo(ref scr, time, volume);
        }
    }

    public static void PlayMusicCrossFade(string name, string crossFadeTargetName,float volume, float time)
    {
        // if (Instance.musicChannels.Any(musicChannel => musicChannel.clip != null && musicChannel.clip.name == name)) return;

        AudioSource scr = Instance.GetFirstUnusedMusicChannel();
        if (scr == null) return;
        scr.clip = Instance.musicFiles[name.ToLower()];
        scr.volume = .25f;
       
        AudioSource crossFadeTarget = Instance.GetMusicChannelPlaying();
        scr.Play();
        if (crossFadeTarget != null)
        {
            Debug.Log("CrossFading From" + crossFadeTarget.clip.name + " To " + scr.name);
            CrossFadeChannels(scr, crossFadeTarget, time, volume, 0.0f);
        }
        else
        {
            Debug.Log("Fading "+ scr.clip.name + " To " + volume);
            FadeAudioSourceTo(ref scr, time, volume);
        }
    }

    public static void PlayMusicCrossFadeAll(string name, float volume, float time)
    { 
        AudioSource scr = Instance.GetFirstUnusedMusicChannel();
        if (scr == null) return;
        scr.clip = Instance.musicFiles[name.ToLower()];
        scr.volume = .25f;
        FadeAudioSourceTo(ref scr,time,volume);
        for (int index = 0; index < Instance.GetMusicChannelsPlaying().Length; index++)
        {
            FadeAudioSourceOut(ref Instance.GetMusicChannelsPlaying()[index], time, 0.0f);
        }
        scr.Play();
    }
#endregion
}
