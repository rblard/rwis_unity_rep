package com.example.midifileperformer;

import java.nio.ByteBuffer;

public class MidiMessage {


    public enum MidiMessageType {NOTE_OFF, NOTE_ON}

    public byte[] messageContents;

    private byte[] onOrOffMessage(int pitch, int channel, int velocity){
        byte[] noteOnContent = {};
        ByteBuffer wrapper = ByteBuffer.wrap(noteOnContent);

        wrapper.putChar((char)(0x90+channel));
        wrapper.putChar((char)pitch);
        wrapper.putChar((char)velocity);

        return wrapper.array();
    }

    public MidiMessage(MidiMessageType type, int pitch, int channel, int velocity){
        switch(type){
            case NOTE_OFF:
            case NOTE_ON:
                messageContents = onOrOffMessage(pitch,channel,velocity);
                break;
        }
    }


}
