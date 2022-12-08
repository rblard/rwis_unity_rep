package com.example.midifileperformer;

import com.leff.midi.MidiFile;
import com.leff.midi.MidiTrack;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;

import android.util.Log;

public class MidiFileParser {

    public static void parse(InputStream is) {
        MidiFile midi = null;

        try {
            midi = new MidiFile(is);
        }catch(IOException ioException) {
            ioException.printStackTrace();
            return;
        }


    }
}
