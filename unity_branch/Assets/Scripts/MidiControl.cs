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
    }

    // Update is called once per frame
    void Update()
    {   
        // Get Number of Key Pressed
        // int numberOfKeysPressed;
        // numberOfKeysPressed = keysToCheck.Count(key => Input.GetKey(key));
        // Debug.Log(numberOfKeysPressed);
        
        // Muptiple Key Pressed Streaming Play Test
        // if (Input.GetKeyDown(KeyCode.Space)){
        //     midiStreamPlayer.MPTK_StartMidiStream();
        //     List<MPTKEvent> subList = midiEventList.GetRange(1, 20);
        //     midiStreamPlayer.MPTK_PlayEvent(subList);
        // }

        if (Input.anyKeyDown)
        {   
            // Testing on triggering a sequence of MPTKEvent
            midiStreamPlayer.MPTK_StartMidiStream();
            midiStreamPlayer.MPTK_PlayEvent(midiEventList[midiEventListIndex]);
            // Debug.Log(midiEventList[midiEventListIndex]);
            midiEventListIndex += 1;  
        }
        
        
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
