package com.example.midifileperformer;

import androidx.appcompat.app.AppCompatActivity;

import android.annotation.SuppressLint;
//import android.media.midi.*;
import android.os.Bundle;
import android.view.MotionEvent;
import android.widget.Button;

import com.leff.midi.event.NoteOn;

@SuppressLint("ClickableViewAccessibility")

public class MainActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        Button midiButton = findViewById(R.id.button);

        midiButton.setOnTouchListener((view, event) -> {
            view.performClick();
            int actionType = event.getAction();
            if(actionType == MotionEvent.ACTION_DOWN) {
                MidiMessage msg = new MidiMessage(MidiMessage.MidiMessageType.NOTE_ON,60,0,64);
            }
            else if(actionType == MotionEvent.ACTION_UP || actionType == MotionEvent.ACTION_CANCEL) {
                MidiMessage msg = new MidiMessage(MidiMessage.MidiMessageType.NOTE_OFF,60,0,64);
            }
            return false;
        });
    }
}