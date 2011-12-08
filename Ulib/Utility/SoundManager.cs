/*******************************************************
 * Copywrite 2011 Not So Indie Games
 * Orginal Author: Tyler Steele
 * Contact Email: Tyler@ShadesofGames.com
 * 
 * 
 * 
 * Sound Manager: This class is met to provide a high level 
 * API for unity's sound engine. Supporting Sound Queues,
 * Sound playing with delagate callbacks.  
 *******************************************************/
//TODO
/* Add stopping to the queues
 * 
 * 
 * 
 */


using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public delegate void SoundCallBack();

public class SoundManager : ComponentSingleton<SoundManager>
{
    #region Helper Classes
    private class SoundQueue
    {
        private AudioSource source;
        private string[] sounds;
        private string library;
        private int current;
        private bool loop;
        private float volume;
        public bool IsCompleted;
        public  SoundQueue(AudioSource source, string[] sounds, string lib= "default", bool loop = false, float volume = 0.5f)
        {
            this.source = source;
            this.sounds = sounds;
            this.loop = loop;
            this.volume = volume;
            library = lib;
        }

        public void Start()
        {
            PlaySoundWithSource(source,sounds[0],library,false,volume);
        }

        public void Update()
        {
            if(source.isPlaying && !loop)return;
            current++;
            if(current<sounds.Length)
            {
                PlaySoundWithSource(source, sounds[current],library,false,volume);
            }
            else if(loop)
            {
                current = 0;
                PlaySoundWithSource(source, sounds[current], library, false, volume);
            }
            else
            {
                IsCompleted = true;
                Destroy(source);
            }
        }
    }

    #endregion

    #region Public Members
    private Dictionary<string, AudioClip>  musicFiles;

    /// <summary>
    /// This is a sound library you can load sounds into and unload them as well. 
    /// The library created for the Effects folder is called default and will automatically be used. 
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
    private int prevPriorityChannelsPlaying;
    private float[] musicVolumes;
    private List<AudioSource> priorityChannels = new List<AudioSource>(),
                              gameObjectChannels = new List<AudioSource>();

    private List<SoundQueue> soundQueues2D = new List<SoundQueue>();
    private List<SoundQueue> soundQueues3D = new List<SoundQueue>();

    private Dictionary<AudioSource, Delegate> callbackAudioSource = new Dictionary<AudioSource, Delegate>();
    private Dictionary<AudioSource, Delegate> callback3DAudioSource = new Dictionary<AudioSource, Delegate>(); 
    
  
    #endregion

    #region Initilization

    public void Init(bool loadOnStart, bool dontDestroy, int numMusicChannels = 2, int numSoundChannels = 6, string soundDirectory = "Sounds")
    {
        base.Init();
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
        musicVolumes = new float[numMusicChannels];

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

    #endregion

    #region Sounds

    /*
    * 2D sound functions
    */
    public static void Play2DSound(string name, float volume = .5f,string library = "default", bool loop = false,bool fadeMusic = false, float fadeTo = 0, float fadeTime = 0)
    {
        try
        {
            var channel = Instance.GetFirstUnusedSoundChannel();
            channel.clip = (Instance.soundLibrary[library.ToLower()][name.ToLower()]);
            channel.loop = loop;
            channel.volume = volume;
            channel.Play();


            if (fadeMusic) FadeAllMusic(fadeTo, fadeTime);
        }
        catch (Exception e)
        {
            
            Debug.LogError("There was an error playing sound " + name);
            Debug.Log(e.GetType() + " Error  Message: " +e.Message);
        }
       
    }

    public static void Play2DSoudWithCallback(string name, Delegate callBack, float volume = .5f, string library = "default", bool loop = false,bool fadeMusic = false, float fadeTo = 0, float fadeTime = 0)
    {
        var channel = Instance.GetFirstUnusedSoundChannel();
        channel.clip = (Instance.soundLibrary[library.ToLower()][name.ToLower()]);
        channel.volume = volume;
        channel.Play();

        Instance.callbackAudioSource.Add(channel,callBack);
        if (fadeMusic) FadeAllMusic(fadeTo, fadeTime);
    }

    public static void Play2DSoundQueue(string[] names, float volume = .5f, string library = "default", bool loop = false)
    {
        Debug.Log(names);
        Instance.soundQueues2D.Add(new SoundQueue(Instance.gameObject.AddComponent<AudioSource>(), names, library,loop,volume));
        Instance.soundQueues2D[Instance.soundQueues2D.Count-1].Start();
    }

   /*
    * 3D sound functions
    */

    public static void Play3DSoundWithCallback(GameObject source, string soundName, Delegate callBack ,string library = "default", bool loop = false, float volume = .5f)
    {
        AudioSource audioSource = source.GetComponent<AudioSource>();

        if (!audioSource)
            audioSource = source.AddComponent<AudioSource>();


        Instance.callback3DAudioSource.Add(audioSource,callBack);
        PlaySoundWithSource(audioSource, soundName, library, loop, volume);

    }

    public static void Play3DSound(GameObject source, string soundName, string library = "default", bool loop = false, float volume = .5f)
    {
        AudioSource audioSource = source.GetComponent<AudioSource>();

        if (!audioSource)
            audioSource = source.AddComponent<AudioSource>();
            

        Instance.gameObjectChannels.Add(audioSource);

        PlaySoundWithSource(audioSource, soundName,library, loop, volume);
            
    }

    public static void Play3DSoundQueue(string[] names, GameObject source, float volume = .5f, string library = "default", bool loop = false)
    {
        Debug.Log(names);
        Instance.soundQueues3D.Add(new SoundQueue(source.AddComponent<AudioSource>(), names, library, loop,volume));
        Instance.soundQueues3D[Instance.soundQueues3D.Count - 1].Start();
    }

    //Just a small helper function
    private static void PlaySoundWithSource(AudioSource src, string clipName, string lib, bool loop = false, float volume = .5f)
    {
        Debug.Log("Playing Sound: " + clipName + "From Library: " + lib);
        src.clip = Instance.soundLibrary[lib.ToLower()][clipName.ToLower()];
        src.volume = volume;
        src.loop = loop;
        src.Play();
    }

    #endregion

    #region Music

        public static void PlayMusic(string name, float volume = .5f, bool loop = false)
        {

            Debug.Log("Playing Music");
            AudioSource scr = Instance.GetFirstUnusedMusicChannel();
            scr.clip = Instance.musicFiles[name.ToLower()];
            scr.volume = volume;
            scr.loop = loop;
            scr.Play();

        }

        public static void PlayMusicCrossFade(string name, float volume, float time)
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
    
        public static void PauseMusic(string name)
        {
            AudioSource scr = Instance.GetFirstUnusedMusicChannel();
            scr.clip = Instance.musicFiles[name.ToLower()];
            scr.Pause();
        }

        public static void FadeAllMusic(float volume, float time)
        {
            Debug.Log("Fadingout");

            for (int index = 0; index < Instance.GetMusicChannelsPlaying().Length; index++)
            {
                Debug.Log("Fading out channel: " + index);
                Instance.musicVolumes[index] = Instance.GetMusicChannelsPlaying()[index].volume;
                FadeAudioSourceTo(ref Instance.GetMusicChannelsPlaying()[index], time, volume);
            }
        }

    #endregion
  
    #region Remove Functions

    public void RemoveSoundLibrary(string libraryName)
    {
        if(soundLibrary.ContainsKey(libraryName))
            soundLibrary.Remove(libraryName);
        else
            Debug.LogError("No Library found with name: " + libraryName);
        
    }

        #endregion  

    #region Private Functions

    #region Load Functions

    private void LoadSounds(string Library = "default", string Directory = "Effects")
    {
        
        Object[] sound = Resources.LoadAll(SoundDirectory + "/" + Directory);
        soundLibrary.Add(Library,new Dictionary<string, AudioClip>());
        foreach (Object o in sound)
        {
            Debug.Log("Loading Sound :" + o.name);
            soundLibrary[Library].Add(o.name.ToLower(), o as AudioClip);
        }

        
    }

    private void LoadMusic()
    {
        Object[] music = Resources.LoadAll(SoundDirectory + "/Music");
        Debug.Log("Loading: " + music.Length + " files");
        foreach (Object o in music)
        {
            Debug.Log("Loading music file: " + o.name);
            musicFiles.Add(o.name.ToLower(), o as AudioClip);

        }

    }


        #endregion

    #region Helpers

    private AudioSource GetFirstUnusedSoundChannel()
    {
        foreach (var sc in soundChannels.Where(sc => !sc.isPlaying))
            return sc;
      
        Debug.LogError("No Channels found");
        return null;
    }

    private AudioSource GetFirstUnusedMusicChannel()
    {
        foreach (var sc in musicChannels.Where(sc => !sc.isPlaying))
            return sc;

        Debug.LogError("No Channels found");
        return null;
    }

    private AudioSource GetMusicChannelPlaying(string clipName)
    {
        foreach (var sc in musicChannels.Where(sc => sc.isPlaying && sc.name == clipName))
            return sc;

        Debug.LogError("No channel found playing: " + clipName);
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

        Debug.LogError("No Channels found");
        return null;
    }
    #endregion

    #region Update

    private void Update()
    {
        Update3DChannels();
        UpdatePriorityChannels();
        Update2DCallbacks();
        Update3DCallbacks();
        Update2DQueues();
        Update3DQueues();
    }

    private void Update3DChannels()
    {
        for (int i = 0; i < gameObjectChannels.Count; i++)
        {
            if(gameObjectChannels[i].isPlaying)continue;

            Destroy(gameObjectChannels[i]);
            gameObjectChannels.Remove(gameObjectChannels[i]);
        }
    }

    private void UpdatePriorityChannels()
    {
            int current = 0;

        for (int index = 0; index < priorityChannels.Count; index++)
        {
            AudioSource t = priorityChannels[index];
            if (t.isPlaying)
            {
                current++;
                //Debug.Log(current);
                continue;
            }
            t.clip = null;
            priorityChannels.Remove(priorityChannels[index]);
            index--;
        }

        if (prevPriorityChannelsPlaying != current && current == 0)
        {
            Debug.Log("No More sound channels fading music back in");
            for (int index = 0; index < Instance.GetMusicChannelsPlaying().Length; index++)
            {
                FadeAudioSourceTo(ref Instance.GetMusicChannelsPlaying()[index], 2.0f, musicVolumes[index]);
            }
        }
        prevPriorityChannelsPlaying = current;
    }

    private void Update3DCallbacks()
    {
        if (callback3DAudioSource.Count == 0) return;
        List<AudioSource> completed = new List<AudioSource>();
        foreach (KeyValuePair<AudioSource, Delegate> pair in callback3DAudioSource)
        {
            if (pair.Key.isPlaying) continue;
            completed.Add(pair.Key);
            pair.Value.DynamicInvoke();
            completed.Add(pair.Key);
        }

        foreach (var audioSource in completed)
        {
            callbackAudioSource.Remove(audioSource);
        }
    }

    private void Update2DCallbacks()
    {
        if (callbackAudioSource.Count == 0) return;
        List<AudioSource> completed = new List<AudioSource>();
        foreach(KeyValuePair<AudioSource, Delegate> pair in callbackAudioSource)
        {
            if (pair.Key.isPlaying) continue;
            completed.Add(pair.Key);
            pair.Value.DynamicInvoke();
            completed.Add(pair.Key);
        }

        foreach (var audioSource in completed)
        {
                callbackAudioSource.Remove(audioSource);
        }
    }

    private void Update2DQueues()
    {
        for (int index = 0; index < soundQueues2D.Count; index++)
        {
            soundQueues2D[index].Update();

            if (!soundQueues2D[index].IsCompleted) return;

            soundQueues2D.Remove(soundQueues2D[index]);
            index--;
        }
    }

    private void Update3DQueues()
    {
        for (int index = 0; index < soundQueues3D.Count; index++)
        {
            soundQueues3D[index].Update();

            if (!soundQueues3D[index].IsCompleted) return;

            soundQueues3D.Remove(soundQueues3D[index]);
            index--;
        }
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
            Debug.Log("Fading source out");
            iTween.AudioTo(source.gameObject, iTween.Hash("audiosource", source, "time", time, "volume", fadeTo, "easetype", iTween.EaseType.linear, "oncomplete", "StopOnFadeOut", "oncompleteparams", source, "oncompletetarget", source.gameObject));
        }

        private static void CrossFadeChannels(AudioSource to,AudioSource from, float time, float maxVolume, float minVolume)
        {
            FadeAudioSourceTo(ref to, time, maxVolume);
            FadeAudioSourceOut(ref from, time, minVolume);
        }
    #endregion
    #endregion
}
