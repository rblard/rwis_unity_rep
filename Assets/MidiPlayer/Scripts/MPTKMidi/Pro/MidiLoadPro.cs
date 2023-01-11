#define DEBUG_LOGEVENT 
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using MPTK.NAudio.Midi;
using System;
using System.IO;
using System.Linq;

namespace MidiPlayerTK
{
    // Class for loading a Midi file. 
    // No sequencer, no synthetizer, so no music playing capabilities. 
    // Usefull to load all the Midi events from a Midi and process, transform, write them to what you want. 
    public partial class MidiLoad
    {
        /// <summary>@brief
        /// Load MIDI file from a local file (Moved to PRO since version 2.89.5)
        /// </summary>
        /// <param name="filename">Midi path and filename to load (OS dependant)</param>
        /// <param name="strict">if true the MIDI must strictely respect the midi norm</param>
        /// <returns></returns>
        public bool MPTK_LoadFile(string filename, bool strict = false)
        {
            bool ok = true;
            try
            {
                using (Stream sfFile = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    byte[] data = new byte[sfFile.Length];
                    sfFile.Read(data, 0, (int)sfFile.Length);
                    ok = MPTK_Load(data, strict);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
                ok = false;
            }
            return ok;
        }
        /// <summary>@brief
        /// Load Midi from a MidiFileWriter2 object
        /// </summary>
        /// <param name="mfw2">MidiFileWriter2 object</param>
        /// <returns>true if loaded</returns>
        public bool MPTK_Load(MidiFileWriter2 mfw2)
        {
            Init();
            bool ok = true;
            try
            {
                midifile = mfw2.MPTK_BuildNAudioMidi();
                List<TrackMidiEvent> tmEvents = ConvertFromMidiFileToTrackMidiEvent();
                if (tmEvents != null)
                {
                    AnalyseTrackMidiEvent(tmEvents);
                    MPTK_MidiEvents = tmEvents;
                }
                // Try to play directly. To be done in a next version, perhaps ...
                //midifile = new MidiFile(mfw2.MPTK_MidiFileType, mfw2.MPTK_DeltaTicksPerQuarterNote);
                //AnalyseTrackMidiEvent(mfw2.TmidiEvents);
                //MPTK_MidiEvents = mfw2.TmidiEvents;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
                ok = false;
            }
            return ok;
        }
    }
}

