
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using UnityEngine.Events;
using MEC;
using System.IO;

namespace MidiPlayerTK
{

    public partial class MidiFileLoader : MonoBehaviour
    {
        /// <summary>@brief
        /// [MPTK PRO] Load a MIDI file from a local desktop file. Look at MPTK_MidiLoaded for detailed information about the MIDI loaded.\n
        /// Example of path for Mac "/Users/xxx/Desktop/WellTempered.mid"\n
        /// Example of path for Windows "C:\Users\xxx\Desktop\BIM\Sound\Midi\DreamOn.mid"\n
        /// </summary>
        /// <param name="filePath">Example for Windows: filePath= "C:\Users\xxx\Desktop\BIM\Sound\Midi\DreamOn.mid"</param>
        /// <returns>true if loading succeed</returns>
        public bool MPTK_Load(string filePath)
        {
            bool result = false;
            // example with filePath= C:\Users\Thierry\Desktop\BIM\Sound\Midi\DreamOn.mid
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    Debug.LogWarning($"MPTK_Load: file path not defined");
                else if (!File.Exists(filePath))
                    Debug.LogWarning($"MPTK_Load: {filePath} not found");
                else
                {
                    using (Stream fsMidi = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] midiBytesToLoad = new byte[fsMidi.Length];
                        fsMidi.Read(midiBytesToLoad, 0, (int)fsMidi.Length);
                        midiLoaded = new MidiLoad();
                        midiLoaded.KeepNoteOff = MPTK_KeepNoteOff;
                        midiLoaded.MPTK_KeepEndTrack = MPTK_KeepEndTrack;
                        midiLoaded.MPTK_EnableChangeTempo = true;
                        midiLoaded.LogEvents = MPTK_LogEvents;
                        if (!midiLoaded.MPTK_Load(midiBytesToLoad))
                            return false;
                        SetAttributes();
                        midiNameToPlay = Path.GetFileNameWithoutExtension(filePath);
                        result = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return result;
        }

        /// <summary>@brief
        /// [MPTK PRO] Find a Midi in the Unity resources folder MidiDB which contains the name (case sensitive)\n
        /// <b>Beware: name of this method is not appropriate because class MidiFileLoader is not able to play MIDI</b>. Rather use MidiFilePlayer class.\n
        /// We keep it only for compatibility, could be removed with a major version.\n
        ///  
        /// Tips: Add MIDI files to your project with the Unity menu MPTK or add it directly in the ressource folder and open MIDI File Setup to automatically integrate MIDI in MPTK.
        /// @code
        /// // Find the first MIDI file name in MidiDB which contains "Adagio"
        /// midiLoadPlayer.MPTK_SearchMidiToPlay("Adagio");
        /// @endcode
        /// </summary>
        /// <param name="name">case sensitive part of a MIDI file name</param>
        /// <returns>true if found else false</returns>
        public bool MPTK_SearchMidiToPlay(string name)
        {
            int index = -1;
            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null)
                    {
                        index = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s.Contains(name));
                        if (index >= 0)
                        {
                            MPTK_MidiIndex = index;
                            //Debug.LogFormat("MPTK_SearchMidiToPlay: '{0}' selected", MPTK_MidiName);
                            return true;
                        }
                        else
                            Debug.LogWarningFormat("No MIDI file found with '{0}' in name", name);
                    }
                    else
                        Debug.LogWarning(MidiPlayerGlobal.ErrorNoMidiFile);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return false;
        }

    }
}

