package com.example.midifileperformer;

import java.nio.ByteBuffer;

public class MidiMessage {


    public enum MidiMessageType {NOTE_OFF, NOTE_ON}

    public byte[] messageContents;

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


}
