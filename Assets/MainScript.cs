using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Audio;
using MidiParser;


public class MainScript : MonoBehaviour
{

    public List<List<int>> chronology ;
    public int i;
    public int N;
    public AudioClip[] sounds;
    public List<AudioSource> audioSources;

   

    public void Start()
    {
        i=0;
        N = 10;
        audioSources = new List<AudioSource>();
        for (int k = 0; k < N; k++)
        {
            GameObject audioObject = new GameObject("AudioObject" + k);
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSources.Add(audioSource);
        }   
        chronology = GetChronology();
        sounds = Resources.LoadAll<AudioClip>("Sounds");

    }


    public void KeyPressed()
    {
        List<int> events = new List<int>(chronology[i]);
        foreach(int n in events)
        {
            Debug.Log(n);
            Playnote(n);
        }
        i++;
    }

    public void Playnote(int n)
    {   
        int k = (n+10)%89;
        bool full=true;
        for(int c=0;c<N;c++)
        {
            if(!audioSources[c].isPlaying)
            {
               full = false;
               audioSources[c].clip = sounds[k];
               audioSources[c].Play();
               break; 
            }
        }
        Debug.Log(!full);
        if(full)
        {
               audioSources[0].clip = sounds[k];
               audioSources[0].Play();
        }
    }

    
    private List<List<int>> GetChronology()
    {
        List<List<int>> chronology = new List<List<int>>();
        List<int> events = new List<int>();
        int t_1 = 0;
        int t = 0;
        var midiFile = new MidiFile("Assets/for_elise_by_beethoven.mid");
        foreach (var track in midiFile.Tracks)
            {
                foreach (var midiEvent in track.MidiEvents)
                {
                    if (midiEvent.MidiEventType == MidiEventType.NoteOn)
                    {
                        t = midiEvent.Time;
                        if (t_1 != t && events.Count!=0)
                        {
                            List<int> events_f = new List<int>(events);
                            chronology.Add(events_f);
                            events.Clear();
                        }
                        var note = midiEvent.Note;
                        events.Add(note);
                        t_1=t;
                    }
                }
            }
        return chronology;
    }
}
