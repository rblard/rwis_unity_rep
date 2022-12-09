package com.example.midifileperformer.nativewrap;

import com.example.midifileperformer.MidiMessage;

import org.bytedeco.javacpp.*;
import org.bytedeco.javacpp.annotation.*;

@Platform(include={"NoteAndCommandEvents.h","SequencePerformer.h"})

// TODO : TEMPORARILY PUBLIC WITH TEMPORARILY PUBLIC METHODS FOR TESTING !! SHOULD ONLY BE PROTECTED

public class NativeManager {

    public static CommandData convertMessageToCommandData(MidiMessage message){
        if(!message.isNoteMessage()) throw new IllegalArgumentException("Message provided is not a note on/off message");

        boolean pressed = false;
        if(message.isPressed()) pressed = true;

        int channel = message.getNoteMessageChannel();
        int pitch = message.getNoteMessagePitch();
        int velocity = message.getNoteMessageVelocity();

        return new CommandData(pressed,channel,pitch,velocity);
    }

}
