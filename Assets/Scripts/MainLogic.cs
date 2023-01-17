using System.Collections;
using System;
using System.Linq;
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
    public MidiExternalPlayer midiFilePlayer; // used to play back the file itself
    private String midiFilePath = null; // current path of the MIDI file
    private bool isPlaybackActive = false; // used to prevent simultaneous tapping and playback
    private bool isPressed = false; // keeps track of screen press status

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

    // Debug function that imitates the Events::isStart method in the C++ library

    private bool isStartingSet(List<MPTKEvent> midiEventList){

        foreach(MPTKEvent midiEvent in midiEventList){
            if(midiEvent.Command == MPTKCommand.NoteOn) return true;
        }

        return false;
    }

    // Called every time a new MIDI file is loaded in

    private void refreshMidiFile(){
        if(midiFilePath == null) return;

        midiFileLoader.MPTK_Load(midiFilePath);
        List<MPTKEvent> midiEventList = midiFileLoader.MPTK_ReadMidiEvents();

        clearPerformer(); 

        int latestEventTime = 0; // we're going to convert ticks to relative

        foreach(MPTKEvent midiEvent in midiEventList)
        {

            // Trying to apply presets indicated in the file so we're not stuck with piano
            // This is a bad way of doing this because we're betting on there only being one preset change...
            // But it's the best we can do until the underlying C++ library implements patch change events.
            // When that happens, we can treat them just like other MIDI events, pulling them with key presses.

            if(midiEvent.Command == MPTKCommand.PatchChange) midiStreamPlayer.MPTK_ChannelPresetChange(midiEvent.Channel, midiEvent.Value);

            // Discard all non-note events
            // TODO : do we actually need tempo events to convert to ms in order to prevent anti-shadowing ?
            if(!(midiEvent.Command == MPTKCommand.NoteOff) && !(midiEvent.Command == MPTKCommand.NoteOn)) continue;

            // Determine event type (on or off)
            bool pressed = (midiEvent.Command == MPTKCommand.NoteOn && midiEvent.Velocity != 0) ? true : false;

            int eventTickMs = Mathf.RoundToInt(midiEvent.RealTime);
            pushMPTKEvent(eventTickMs - latestEventTime, pressed, midiEvent.Value, midiEvent.Channel, midiEvent.Velocity); // push the relative tick (difference to latest event)
            if(eventTickMs > latestEventTime) latestEventTime = eventTickMs; // update latest tick
        }

        finalizePerformer(); // tell C++ performer we're ready to play

        // Stop both players

        midiStreamPlayer.MPTK_ClearAllSound();
        midiFilePlayer.MPTK_Stop();
        midiFilePlayer.MPTK_ClearAllSound();
    }

    // Wrapper around the NativeFilePicker library to update the current file path
    // End with a refresh

    public void loadFile(){
        if(NativeFilePicker.IsFilePickerBusy()) return;

        NativeFilePicker.Permission permission = NativeFilePicker.PickFile( 
            (path) => {
                if(path == null) return;
                else{
                    midiFilePath = path;
                    midiFilePlayer.MPTK_MidiName = "file://" + midiFilePath; // for some reason the MidiExternalPlayer wants a "file://" to start the path..???
                }
            },
            new string[] {NativeFilePicker.ConvertExtensionToFileType("mid"), NativeFilePicker.ConvertExtensionToFileType("midi")}
        );

        refreshMidiFile();
    }

    // Passive playback function used in the Play File button.

    public void playFile(){
        // Welkin Note 2023-01-15: Debug
        Debug.Log("PlayFile Button is Clicked. Current MPTK_MidiName is: " + midiFilePlayer.MPTK_MidiName);
        if(midiFilePlayer.MPTK_MidiName == null || midiFilePlayer.MPTK_MidiName == "") return;

        if(isPlaybackActive){
            isPlaybackActive = false;
            midiFilePlayer.MPTK_Pause();
        }

        else{
            midiStreamPlayer.MPTK_ClearAllSound();
            isPlaybackActive = true;
            midiFilePlayer.MPTK_Play();
        }
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
        
        // Welkin note 2023-01-15: Debug why note off are not triggered
        // Debug.Log("isPressed is: " + isPressed + ", first value in dataContainer is: " + dataContainer[0]);


        List<MPTKEvent> returnedEvents = new List<MPTKEvent>();
        foreach(ulong data in dataContainer) 
        {
            if (data == 0) {
                // Debug.Log("Break Happened");
                break; // this terminator is used because we don't have a resizable array. It's due to C# initializing arrays with 0.
            }
            MPTKEvent renderedEvent = makeMPTKEvent(data);
            returnedEvents.Add(renderedEvent);
        }

        // Welkin note 2023-01-15: Debug why note off are not triggered
        // Debug.Log("Finger ID is: " + fingerID + ", isPressed is: " + isPressed + ", length of returnedEvents is: " + returnedEvents.Count + ", first value in returnedEvents is: " + returnedEvents[0]);
        return returnedEvents;
    }

    // ------------------------------------------------------------------------
    // --------------------------UNITY BEHAVIOUR-------------------------------
    // ------------------------------------------------------------------------

    void Start()
    {
        clearPerformer(); // apparently if we don't do this the file keeps its state between restarts ??? 
        midiStreamPlayer.MPTK_InitSynth();

        // Welkin Note 2022-12-18: Touch input initial settings
        Input.multiTouchEnabled = true;
        Input.simulateMouseWithTouches = true;
        Debug.Log("Device Resolution is: " + Screen.currentResolution);
    }

    void Update()
    {   
        int touchCount = Input.touchCount;

        if(touchCount > 0 && !isPlaybackActive){
            // Welkin 2023-01-06 Debug
            // SEAudioSource.Play();

            // This following line should mean : "exclude any touch that happens over a UI object" (i.e. buttons and menus)

            // But...it actually excludes EVERY TOUCH except for releases !!!

            var validTouches = Input.touches;//.Where(touch => !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId)).ToArray();

            foreach(Touch touch in validTouches){                
                List<MPTKEvent> eventsToPlay;
                
                // Welkin Note 2022-12-18: This won't work, the Update() will keep trigger the PlayEvent.
                // if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary){
                //      isPressed = true;
                //     print("Press");
                // }
                // if (touch.phase == TouchPhase.Ended){
                //     isPressed = false;
                //     print("Release");
                // }
                // eventsToPlay = getEventsFromNative(isPressed, Convert.ToUInt16(touch.fingerId)); 
                // midiStreamPlayer.MPTK_PlayEvent(eventsToPlay);

                // Welkin Note 2023-01-15: Add a restriction area for Midi Play
                if (touch.position.y < Screen.height * 0.80f){
                    // Welkin Note 2023-01-15: isPressed Flag Solution.
                    // The SEAudioSource triggered everytime my finger leave the screen.
                    if (!isPressed){
                        isPressed = true;
                        eventsToPlay = getEventsFromNative(isPressed, Convert.ToUInt16(touch.fingerId));
                        midiStreamPlayer.MPTK_PlayEvent(eventsToPlay);
                    }

                    if(touch.phase == TouchPhase.Ended){
                        // Welkin Note 2023-01-15: Debug SEAudioSource
                        // Debug.Log("When Release, FingerID is: " + touch.fingerId);
                        SEAudioSource.Play();
                        // Welkin Note 2023-01-15: Using this as a patch for NoteOff not triggering bug now
                        midiStreamPlayer.MPTK_ClearAllSound();

                        isPressed = false;
                        // Welkin Note 2023-01-15: Testing if it's the input parameter problem
                        eventsToPlay = getEventsFromNative(isPressed, Convert.ToUInt16(touch.fingerId));

                        midiStreamPlayer.MPTK_PlayEvent(eventsToPlay);
                    }
                }
                
            }
        }
    }
}
