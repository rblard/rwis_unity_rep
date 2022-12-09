package com.example.midifileperformer;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.appcompat.app.AppCompatActivity;
import android.app.Activity;

import android.annotation.SuppressLint;
//import android.media.midi.*;
import android.content.Intent;
import android.os.Bundle;
import android.net.Uri;
import android.view.MotionEvent;
import android.widget.Button;

import com.example.midifileperformer.nativewrap.CommandData;
import com.example.midifileperformer.nativewrap.NativeManager;

import java.io.FileNotFoundException;
import java.io.InputStream;

@SuppressLint("ClickableViewAccessibility")

public class MainActivity extends AppCompatActivity {

    //public final static String URIHeader = "/document/primary:";

    public void loadMidiFile(){
        Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
        intent.setType("audio/midi");
        intent.addCategory(Intent.CATEGORY_OPENABLE);
        resultLauncher.launch(intent);
    }

    ActivityResultLauncher<Intent> resultLauncher = registerForActivityResult(
        new ActivityResultContracts.StartActivityForResult(),
            result -> {
                if (result.getResultCode() == Activity.RESULT_OK) {
                    assert result.getData() != null;
                    Uri contentUri = result.getData().getData();
                    InputStream is = null;
                    try {
                        is = getContentResolver().openInputStream(contentUri);
                    }catch(FileNotFoundException e){
                        e.printStackTrace();
                    }
                    MidiFileParser.parse(is);
                }
            }
    );

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        Button midiButton = findViewById(R.id.midi_button);
        Button loadButton = findViewById(R.id.load_button);

        loadButton.setOnClickListener((view) -> loadMidiFile());

        midiButton.setOnTouchListener((view, event) -> {
            view.performClick();
            int actionType = event.getAction();
            if(actionType == MotionEvent.ACTION_DOWN) {
                MidiMessage msg = new MidiMessage(MidiMessage.MidiMessageType.NOTE_ON,60,0,64);
                CommandData cmd = NativeManager.convertMessageToCommandData(msg);
            }
            else if(actionType == MotionEvent.ACTION_UP || actionType == MotionEvent.ACTION_CANCEL) {
                MidiMessage msg = new MidiMessage(MidiMessage.MidiMessageType.NOTE_OFF,60,0,64);
            }
            return false;
        });
    }
}