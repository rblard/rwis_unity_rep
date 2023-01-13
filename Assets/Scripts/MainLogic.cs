using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using NativeFilePickerNamespace;
using System.Runtime.InteropServices;

public class MainLogic : MonoBehaviour
{
    // Welkin 2023-01-06 Debug
    public AudioSource SEAudioSource;
    
    // ------------------------------------------------------------------------
    // ----------------------------COMPONENTS----------------------------------
    // ------------------------------------------------------------------------

    public MidiFileLoader midiFileLoader; // used to parse the MIDI file for us
    public MidiStreamPlayer midiStreamPlayer; // used to transform a list of MIDI events into sound
    private String midiFilePath = null; // current path of the MIDI file
    private bool isPressed; // keeps track of screen press status

    // ------------------------------------------------------------------------
    // ----------------------------CONSTANTS-----------------------------------
    // ------------------------------------------------------------------------

    // The fixed size of the buffer array passed to the C++ side when using performer.render()

    private static readonly uint MAX_EVENT_AMOUNT = 4096;
    // Having more than 16*2*128 = 4096 events on one press would mean EVERY note on EVERY channel triggered on AND off...and then some !
    // Look, I know this is ugly, but I can't seem to find how to pass resizable arrays so far...so this will do 
    // TODO : Of course, change the array into a resizable one.

    // Magic values for MPTKEvent construction

    private static readonly ulong COMMAND_MASK = 0xF0 << 16;
    private static readonly ulong CHANNEL_MASK = 0x0F << 16;
    private static readonly ulong NOTE_ON_VALUE = 0x90 << 16;
    //private static readonly ulong NOTE_OFF_VALUE = 0x80 << 16; // never used

    private static readonly ulong PITCH_MASK = 0xFF << 8;
    private static readonly ulong VELOCITY_MASK = 0xFF;

    // ------------------------------------------------------------------------
    // ---------------------------C++ LIB API----------------------------------
    // ------------------------------------------------------------------------

    // Static equivalent to performer.push()
    // Push an event to the performer's chronology 

    [DllImport("libMidifilePerformer", EntryPoint = "pushMPTKEvent")]

    public static extern void pushMPTKEvent(long tick, bool pressed, int pitch, int channel, int velocity);

    // Static equivalent of performer.finalize()
    // Change performer's state from building a chronology to "ready to play"

    [DllImport("libMidifilePerformer", EntryPoint = "finalizePerformer")]

    public static extern void finalizePerformer();

    // Static equivalent of performer.clear()
    // Reset performer's state and clear its chronology

    [DllImport("libMidifilePerformer", EntryPoint = "clearPerformer")]

    public static extern void clearPerformer();

    // Static equivalent of performer.render()
    // Move one step forward in the chronology

    [DllImport("libMidifilePerformer", EntryPoint = "renderCommand")]

    public static extern void renderCommand(bool pressed, uint ID, ulong[] dataContainer);

    // ------------------------------------------------------------------------
    // ------------------------PRIVATE UTIL METHODS----------------------------
    // ------------------------------------------------------------------------

    // Called every time a new MIDI file is loaded in

    private void refreshMidiFile(){
        if(midiFilePath == null) return;

        midiFileLoader.MPTK_Load(midiFilePath);
        List<MPTKEvent> midiEventList = midiFileLoader.MPTK_ReadMidiEvents();

        clearPerformer(); 

        int latestEventTime = 0; // we're going to convert ticks to relative

        foreach(MPTKEvent midiEvent in midiEventList)
        {
            // Discard all non-note events
            // TODO : do we actually need tempo events to convert to ms in order to prevent anti-shadowing ?
            if(!(midiEvent.Command == MPTKCommand.NoteOff) && !(midiEvent.Command == MPTKCommand.NoteOn)) continue;

            // Determine event type (on or off)
            bool pressed = (midiEvent.Command == MPTKCommand.NoteOn && midiEvent.Velocity != 0) ? true : false;

            int eventTickMs = Mathf.RoundToInt(midiEvent.RealTime);
            pushMPTKEvent(eventTickMs - latestEventTime, pressed, midiEvent.Value, midiEvent.Channel, midiEvent.Velocity);
            if(eventTickMs > latestEventTime) latestEventTime = eventTickMs;
        }

        finalizePerformer(); // tell C++ performer we're ready to play
        midiStreamPlayer.MPTK_ClearAllSound();
    }

    // Wrapper around the NativeFilePicker library to update the current file path
    // End with a refresh

    public void loadFile(){
        if(NativeFilePicker.IsFilePickerBusy()) return;

        NativeFilePicker.Permission permission = NativeFilePicker.PickFile( 
            (path) => {
                if(path == null) return;
                else midiFilePath = path;
            },
            new string[] {NativeFilePicker.ConvertExtensionToFileType("mid"), NativeFilePicker.ConvertExtensionToFileType("midi")}
        );

        refreshMidiFile();
    }

    // A "pseudo-constructor" that creates an MPTK event from a note on/off MIDI message stored as a ulong.
    // That ulong was obtained from the NoteData struct on the C++ side.

    private MPTKEvent makeMPTKEvent(ulong data){
        MPTKCommand commandValue;

        if((data & COMMAND_MASK) == NOTE_ON_VALUE) commandValue = MPTKCommand.NoteOn;
        else commandValue = MPTKCommand.NoteOff;

        int channelValue = (int) ((data & CHANNEL_MASK) >> 16);

        int pitchValue = (int) ((data & PITCH_MASK) >> 8);
        int velocityValue = (int) (data & VELOCITY_MASK);

        return new MPTKEvent() {
            Command = commandValue,
            Value = pitchValue, // yeah okay this is ugly I'm sorry
            Channel = channelValue,
            Velocity = velocityValue,
            Duration = -1 // let the event last forever and if it's an on, only be terminated by its off event
        };
    }

    // Wrapper for performer.render().
    // It collects the NoteData structs encoded as ulongs and translates them into MPTKEvents for the stream player.

    private List<MPTKEvent> getEventsFromNative(bool isPressed, uint fingerID)
    {
        ulong[] dataContainer = new ulong[MAX_EVENT_AMOUNT];
        renderCommand(isPressed, fingerID, dataContainer); 
        List<MPTKEvent> returnedEvents = new List<MPTKEvent>();
        foreach(ulong data in dataContainer) 
        {
            if (data == 0) break; // this terminator is used because we don't have a resizable array. It's due to C# initializing arrays with 0.
            MPTKEvent renderedEvent = makeMPTKEvent(data);
            returnedEvents.Add(renderedEvent);
        }
        return returnedEvents;
    }

    // ------------------------------------------------------------------------
    // --------------------------UNITY BEHAVIOUR-------------------------------
    // ------------------------------------------------------------------------

    void Start()
    {
        clearPerformer(); // apparently if we don't do this the file keeps its state between restarts ??? 

        // Welkin Note 2022-12-18: Touch input initial settings
        Input.multiTouchEnabled = true;
        Input.simulateMouseWithTouches = true;
        isPressed = false;
    }

    void Update()
    {   
        int touchCount = Input.touchCount;

        if(touchCount > 0){
            // Welkin 2023-01-06 Debug

            // SEAudioSource.Play();
            foreach(Touch touch in Input.touches){
                
                // Welkin Note 2022-12-18: This won't work, the Update() will keep trigger the PlayEvent.
                // if (touch.phase == TouchPhase.Began){
                //     isPressed = true;
                // }
                // if (touch.phase == TouchPhase.Ended){
                //     isPressed = false;
                // }
                // List<MPTKEvent> eventsToPlay = getEventsFromNative(isPressed, Convert.ToUInt16(touch.fingerId)); 
                // midiStreamPlayer.MPTK_PlayEvent(eventsToPlay);


                List<MPTKEvent> eventsToPlay;
                if (!isPressed){
                    isPressed = true;
                    eventsToPlay = getEventsFromNative(isPressed, Convert.ToUInt16(touch.fingerId));
                    midiStreamPlayer.MPTK_PlayEvent(eventsToPlay);
                }

                if(touch.phase == TouchPhase.Ended){
                    isPressed = false;
                    eventsToPlay = getEventsFromNative(isPressed, Convert.ToUInt16(touch.fingerId));
                    midiStreamPlayer.MPTK_PlayEvent(eventsToPlay);
                }
            }
        }
    }
}
