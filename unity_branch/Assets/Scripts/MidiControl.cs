using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;

public class MidiControl : MonoBehaviour
{
    public MidiFilePlayer midiFilePlayer;
    public MidiStreamPlayer midiStreamPlayer;
    private int midiEventListIndex;
    private List<MPTKEvent> midiEventList;
    // Start is called before the first frame update
    void Start()
    {
        midiFilePlayer.MPTK_MidiName = "bach";
        MidiLoad midiLoad = midiFilePlayer.MPTK_Load();
        midiEventList = midiLoad.MPTK_ReadMidiEvents();
        
        // Remove the event except for NoteOn
        Debug.Log(midiEventList.Count);
        midiEventList.RemoveAll(item=> item.Command != MPTKCommand.NoteOn);
        Debug.Log(midiEventList.Count);
        Debug.Log(midiEventList[0]);

        // Unify the Velocity
        foreach(MPTKEvent midievent in midiEventList){
            midievent.Velocity = 95;
        }

        // Debug.Log(nameof(MPTKCommand.NoteOn));
        midiEventListIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {   


            // Multiple key down
            // numberOfKeysPressed = 
            // print(Input.inputString);

            // Testing on triggering a sequence of MPTKEvent
            midiStreamPlayer.MPTK_StartMidiStream();
            midiStreamPlayer.MPTK_PlayEvent(midiEventList[midiEventListIndex]);
            Debug.Log(midiEventList[midiEventListIndex]);
            midiEventListIndex += 1;
            

            // // Code triggering a single midi event.
            // midiStreamPlayer.MPTK_StartMidiStream();
            // MPTKEvent NotePlaying = new MPTKEvent() {
            //     Command = MPTKCommand.NoteOn,
            //     Value = 60, // play a C4 note
            //     Channel = 0,
            //     Duration = 1000, // one second
            //     Velocity = 100 };
            // midiStreamPlayer.MPTK_PlayEvent(NotePlaying);

            // midiFilePlayer.MPTK_Play();
            // Debug.Log(midiFilePlayer.MPTK_MidiEvents);
        }
    }
}
