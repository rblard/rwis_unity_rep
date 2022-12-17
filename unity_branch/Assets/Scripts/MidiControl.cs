using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System.Linq;
public class MidiControl : MonoBehaviour
{
    public MidiFilePlayer midiFilePlayer;
    public MidiStreamPlayer midiStreamPlayer;
    private int midiEventListIndex;
    private List<MPTKEvent> midiEventList;
    private HashSet<KeyCode> keysToCheck = new HashSet<KeyCode>((KeyCode[])System.Enum.GetValues(typeof(KeyCode)));
    private bool endLock;
    // Start is called before the first frame update
    void Start()
    {
        midiFilePlayer.MPTK_MidiName = "bach";
        MidiLoad midiLoad = midiFilePlayer.MPTK_Load();
        midiEventList = midiLoad.MPTK_ReadMidiEvents();
        
        // Remove the event except for NoteOn
        Debug.Log("On Start: Number of MPTKEvent is " + midiEventList.Count);
        midiEventList.RemoveAll(item=> item.Command != MPTKCommand.NoteOn);
        Debug.Log("On Start: Number of MPTKEvent after remove is " + midiEventList.Count);
        // Debug.Log(midiEventList[0]);

        // Unify the Velocity & Duration
        foreach(MPTKEvent midievent in midiEventList){
            midievent.Velocity = 95;
            midievent.Duration = -1;
        }
        
        // Debug.Log(nameof(MPTKCommand.NoteOn));
        midiEventListIndex = 0;

        // Touch Settings:
        Input.multiTouchEnabled = true;
        Input.simulateMouseWithTouches = true;
        endLock = true;
    }

    // Update is called once per frame
    void Update()
    {   
        if (Input.touchCount > 0)
        {   
            int touchCount = Input.touchCount;
            foreach(Touch touch in Input.touches){

                // Touch touch = Input.GetTouch(0);

                // Current Event
                MPTKEvent CurrentEvent = midiEventList[midiEventListIndex];

                if (endLock){
                    endLock = false;
                    midiStreamPlayer.MPTK_StartMidiStream();
                    midiStreamPlayer.MPTK_PlayEvent(CurrentEvent);
                }                

                Debug.Log(touch.phase);
                Debug.Log(CurrentEvent);

                if(touch.phase == TouchPhase.Ended){  
                    midiStreamPlayer.MPTK_StartMidiStream();
                    CurrentEvent.Command = MPTKCommand.NoteOff;
                    midiStreamPlayer.MPTK_PlayEvent(CurrentEvent);
                    midiEventListIndex += 1;
                    endLock = true;
                }
            }
        }

        // if (Input.anyKeyDown)
        // {   
        //     // Testing on triggering a sequence of MPTKEvent
        //     midiStreamPlayer.MPTK_StartMidiStream();
        //     midiStreamPlayer.MPTK_PlayEvent(midiEventList[midiEventListIndex]);
        //     // Debug.Log(midiEventList[midiEventListIndex]);
        //     midiEventListIndex += 1;  
        // }
        

    }
}
