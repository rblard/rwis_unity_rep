package com.example.midifileperformer.nativewrap;

import org.bytedeco.javacpp.Loader;
import org.bytedeco.javacpp.Pointer;
import org.bytedeco.javacpp.annotation.Platform;

@Platform(include="NoteAndCommandEvents.h")

public class CommandData {

    static { Loader.load(); }

    protected CommandData(boolean pressed, int pitch, int channel, int velocity) { allocate(pressed,pitch,channel,velocity); }
    private native void allocate(boolean pressed, int pitch, int velocity, int channel);
}
