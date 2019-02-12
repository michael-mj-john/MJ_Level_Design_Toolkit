/*
 * UCSC Level Design Toolkit
 * 
 * James Zolyak
 * jameszolyak@gmail.com
 * 
 * This script handles the audio controls for playing, pausing and stoping of Fabric audio.
 * 
 * Released under MIT Open Source License
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public static bool isPlaying = false;

    private static void playAudio(string eventName){
        //AUDIO: without position
        Fabric.EventManager.Instance.PostEvent(eventName);
    }

    private static void playAudioWithPosition(string eventName, GameObject ob){
        //AUDIO: with position
        Fabric.EventManager.Instance.PostEvent(eventName, ob);
    }

    private static void playAudioWithPositionNotify(string eventName, GameObject ob, Fabric.OnEventNotify notify){
        //AUDIO: with position and a callback
        Fabric.EventManager.Instance.PostEventNotify(eventName, ob, notify);
    }

    public static bool FabricLoaded { get { return Fabric.EventManager.Instance; } }


    public static void PlaySound(string n){
        TryLoadFabric();

        if (FabricLoaded){
            playAudio(n);
        }
    }

    public static void PlaySound(string n, GameObject ob){
        TryLoadFabric();
        if (FabricLoaded)
        {
            playAudioWithPosition(n, ob);
        }
    }

    public static void PlaySoundNotify(string n, GameObject ob, Fabric.OnEventNotify notify){
        TryLoadFabric();
        if (FabricLoaded)
        {
            playAudioWithPositionNotify(n, ob, notify);
            isPlaying = true;
        }
    }

    public static void StopSound(string n){
        Fabric.EventManager.Instance.PostEvent(n, Fabric.EventAction.StopSound);
    }

    public static void StopAllSounds(string n){
        Fabric.EventManager.Instance.PostEvent(n, Fabric.EventAction.StopAll);
    }

    public static void UpdateTimelineParameter(string eventName, string pName, float pValue, GameObject ob){
        Fabric.EventManager.Instance.SetParameter(eventName, pName, pValue, ob);
    }

    public static bool PauseSound(string n)
    {
        Fabric.EventManager.Instance.PostEvent(n, Fabric.EventAction.PauseSound);
        return true;
    }

    public static bool UnpauseSound(string n)
    {
        Fabric.EventManager.Instance.PostEvent(n, Fabric.EventAction.UnpauseSound);
        return true;
    }

    public static void Notify(Fabric.EventNotificationType type, string n, object info, GameObject ob)
    {
        if (type == Fabric.EventNotificationType.OnAudioComponentStopped)
        {
            isPlaying = false;
        }
    }

    public static void TryLoadFabric()
    {
        if (FabricLoaded)
        { // || Application.isLoadingLevel) {
            return;
        }
        //UnityEngine.SceneManagement.SceneManager.LoadScene("Audio", UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }
}
