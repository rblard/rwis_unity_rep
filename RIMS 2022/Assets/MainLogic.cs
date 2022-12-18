using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System.Runtime.InteropServices;

public class MainLogic : MonoBehaviour
{
    public MidiFileLoader midiFileLoader;
    public MidiStreamPlayer midiStreamPlayer;
    private List<MPTKEvent> midiEventList;
    private bool isPressed;

    private static readonly uint MAX_EVENT_AMOUNT = 4096;
    // Having more than 16*2*128 = 4096 events on one press would mean EVERY note on EVERY channel triggered on AND off...and then some !
    // Look, I know this is ugly, but I can't seem to find how to pass resizable arrays so far...so this will do 
    // TODO : Of course, change the array into a resizable one.

    // Magic values for MPTKEvent construction

    private static readonly ulong COMMAND_MASK = 0xF0 << 16;
    private static readonly ulong CHANNEL_MASK = 0x0F << 16;
    private static readonly ulong NOTE_ON_VALUE = 0x90 << 16;
    //private static readonly ulong NOTE_OFF_VALUE = 0x80 << 16;

    private static readonly ulong PITCH_MASK = 0xFF << 8;
    private static readonly ulong VELOCITY_MASK = 0xFF;

    [DllImport("libMidifilePerformer", EntryPoint = "pushMPTKEvent")]

    public static extern void pushMPTKEvent(long tick, bool pressed, int pitch, int channel, int velocity);

    [DllImport("libMidifilePerformer", EntryPoint = "finalizePerformer")]

    public static extern void finalizePerformer();

    [DllImport("libMidifilePerformer", EntryPoint = "clearPerformer")]

    public static extern void clearPerformer();

    [DllImport("libMidifilePerformer", EntryPoint = "renderCommand")]
    
    // Welkin Note 2022-12-18: Original rederCommand
    public static extern void renderCommand(bool pressed, uint ID, [Out, In] ulong[] dataContainer);
    // public static extern void renderCommand(bool pressed, uint ID, out ulong[] dataContainer);


    private MPTKEvent makeMPTKEvent(ulong data){ // we have to work with this, the normal constructor won't work !
        MPTKCommand commandValue;

        if((data & COMMAND_MASK) == NOTE_ON_VALUE) commandValue = MPTKCommand.NoteOn;
        else commandValue = MPTKCommand.NoteOff;

        int channelValue = (int) ((data & CHANNEL_MASK) >> 16);

        int pitchValue = (int) ((data & PITCH_MASK) >> 8);
        int velocityValue = (int) (data & VELOCITY_MASK);

        return new MPTKEvent() {
            Command = commandValue,
            Value = pitchValue,
            Channel = channelValue,
            Velocity = velocityValue,
            Duration = -1
        };
    }

    // Wrapper for performer.render().
    // For now, since we can't distinguish between keys, only have one.
    // Also once we can distinguish, we need to take note offs into account.
    
    // Also also : the array filling doesn't work yet. For some reason.
    // Returning the first element of the vector works beautifully. 
    // But of course, we want the whole vector ! That is not done yet.

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

    // Welkin Note 2022=12=18: a convertor turning fingerID to uint8
    // private byte[] ByteConvertor(int inputValue){
    //     byte[] intBytes = BitConverter.GetBytes(inputValue);
    //     Array.Reverse(intBytes);
    //     byte[] result = intBytes;
    //     return result;
    // }

    void Start()
    {
        midiFileLoader.MPTK_MidiName = "bach";
        midiFileLoader.MPTK_Load();
        midiEventList = midiFileLoader.MPTK_ReadMidiEvents();

        clearPerformer();

        foreach(MPTKEvent midiEvent in midiEventList)
        {
            if(!(midiEvent.Command == MPTKCommand.NoteOff) && !(midiEvent.Command == MPTKCommand.NoteOn)) continue;

            print(midiEvent.ToString());

            bool pressed = (midiEvent.Command == MPTKCommand.NoteOn && midiEvent.Velocity != 0) ? true : false;

            pushMPTKEvent(midiEvent.Tick, pressed, midiEvent.Value, midiEvent.Channel, midiEvent.Velocity);
        }

        finalizePerformer();
        
        // Welkin Note 2022-12-18: Touch input initial settings
        Input.multiTouchEnabled = true;
        Input.simulateMouseWithTouches = true;
        isPressed = false;
    }

    // Update is called once per frame
    void Update()
    {   
        int touchCount = Input.touchCount;

        if(touchCount > 0){
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

        // Welkin Note 2022-12-18: Simple debug test
        // if (Input.anyKeyDown){
        //     List<MPTKEvent> eventsToPlay = getEventsFromNative(true, Convert.ToUInt16(1));
        //     midiStreamPlayer.MPTK_PlayEvent(eventsToPlay);
        // }
    }
}
