package com.example.midifileperformer;

import java.nio.ByteBuffer;

public class MidiMessage {

    private static final int noteMessageTypeMask = 0xF0;
    private static final int noteMessageChannelMask = 0x0F;

    public enum MidiMessageType {NOTE_OFF, NOTE_ON}

    private byte[] messageContents;

    private byte[] onOrOffMessage(MidiMessageType type, int pitch, int channel, int velocity){
        ByteBuffer wrapper = ByteBuffer.allocate(3);

        int commandID = 0;
        switch(type){
            case NOTE_OFF:
                commandID = 0x80;
                break;
            case NOTE_ON:
                commandID = 0x90;
                break;
        }

        wrapper.put((byte)(commandID+channel));
        wrapper.put((byte)pitch);
        wrapper.put((byte)velocity);

        return wrapper.array();
    }

    public MidiMessage(MidiMessageType type, int pitch, int channel, int velocity){
        switch(type){
            case NOTE_OFF:
            case NOTE_ON:
                messageContents = onOrOffMessage(type,pitch,channel,velocity);
                break;
        }
    }

    public boolean isNoteMessage(){
        int type = this.messageContents[0] & noteMessageTypeMask;
        return (type == 0x80 || type == 0x90);
    }

    public boolean isPressed(){
        if(!this.isNoteMessage()) throw new IllegalArgumentException("This message is not a note on/off message");

        int type = this.messageContents[0] & noteMessageTypeMask;
        return (type == 0x90 && this.messageContents[2] != 0);
    }

    public int getNoteMessageChannel(){
        if(!this.isNoteMessage()) throw new IllegalArgumentException("This message is not a note on/off message");

        return this.messageContents[0] & noteMessageChannelMask;
    }

    public int getNoteMessagePitch(){
        if(!this.isNoteMessage()) throw new IllegalArgumentException("This message is not a note on/off message");

        return this.messageContents[1];
    }

    public int getNoteMessageVelocity(){
        if(!this.isNoteMessage()) throw new IllegalArgumentException("This message is not a note on/off message");

        return this.messageContents[2];
    }
}
