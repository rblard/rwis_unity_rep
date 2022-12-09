package com.example.midifileperformer.nativewrap;

import org.bytedeco.javacpp.Loader;
import org.bytedeco.javacpp.Pointer;
import org.bytedeco.javacpp.annotation.Platform;

@Platform(include="NoteAndCommandEvents.h")

public class NoteData extends Pointer {
    static { Loader.load(); }

    protected NoteData(boolean on, int pitch, int velocity, int channel) { allocate(on,pitch,velocity,channel); }
    private native void allocate(boolean on, int pitch, int velocity, int channel);


}
