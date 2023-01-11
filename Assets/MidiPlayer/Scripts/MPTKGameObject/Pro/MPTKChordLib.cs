﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MidiPlayerTK
{
    /// <summary>
    /// [MPTK PRO] Build Chord from a library and Play with MidiStreamPlayer.\n
    /// 
    /// See example in TestMidiStream.cs and ExtStreamPlayerPro.cs
    /// </summary>
    public class MPTKChordLib
    {
        /// <summary>@brief
        /// Position in the list
        /// </summary>
        public int Index;

        /// <summary>@brief
        /// Long name of the scale
        /// </summary>
        public string Name;

        /// <summary>@brief
        /// Some indicator when available.
        /// @li   M = major 
        /// @li   m = minor 
        /// @li   A = augmented
        /// @li   D = diminished
        /// @li   S = Suspended
        /// @li   empty = undetermined
        /// </summary>
        public string Modifier3;

        /// Chord contains a 7iem
        /// @li   7 = major 
        /// @li   empty = undetermined
        public string Modifier7;

        /// <summary>@brief
        /// Count of notes in the chord
        /// </summary>
        public int Count;

        /// <summary>@brief
        /// [MPTK PRO] Delta in 1/2 ton from the tonic, so first index=0 return 0 regardless the chord selected. 
        /// </summary>
        /// <param name="index">Position in the scale. If exceed count of notes in the scale, the delta in 1/2 tons is taken from the next octave.</param>
        /// <returns>Delta in 1/2 ton from the tonic</returns>
        public int this[int index]
        {
            get
            {
                try
                {
                    if (chords == null) Init();
                    if (chord == null)
                    {
                        chord = new int[Count];
                        int num = 0;
                        for (int pos = 0; pos < position.Length; pos++)
                        {
                            if (position[pos] == '1')
                            {
                                chord[num++] = pos;
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
                return chord[index];
            }
        }

        private int[] chord;

        /// <summary>@brief
        /// A full scale is based on 12 1/2 tons. This array contains 1/2 tons selected for the scale.
        /// </summary>
        private char[] position;

        private static List<MPTKChordLib> chords;

        /// <summary>@brief
        /// [MPTK PRO] List of chords availables.
        /// </summary>
        public static List<MPTKChordLib> Chords
        {
            get
            {
                if (chords == null) Init();
                return chords;
            }
        }

        /// <summary>@brief
        /// [MPTK PRO] Count of chords availables
        /// </summary>
        public static int ChordCount
        {
            get
            {
                if (chords == null) Init();
                return chords.Count;
            }
        }

        private static void Init(bool log = false)
        {
            if (chords == null)
            {
                chords = new List<MPTKChordLib>();
                TextAsset mytxtData = Resources.Load<TextAsset>("GeneratorTemplate/ChordLib");
                string text = System.Text.Encoding.UTF8.GetString(mytxtData.bytes);
                string[] list1 = text.Split('\r');
                if (list1.Length >= 1)
                {
                    for (int i = 1; i < list1.Length; i++) // first line = title
                    {
                        string[] c = list1[i].Trim('\n').Split(';');
                        if (c.Length == 16)
                        {
                            MPTKChordLib scale = new MPTKChordLib();
                            try
                            {
                                scale.Index = chords.Count;
                                scale.Name = c[0];                                //if (scale.Name[1] == '\n') scale.Name = scale.Name.Remove(0, 1);
                                scale.Modifier3 = c[1];
                                scale.Modifier7 = c[2];
                                scale.Count = Convert.ToInt32(c[3]);
                                scale.position = new char[12];
                                for (int j = 0; j < 12; j++)
                                {
                                    scale.position[j] = c[j + 4][0];
                                }
                            }
                            catch (System.Exception ex)
                            {
                                MidiPlayerGlobal.ErrorDetail(ex);
                            }
                            chords.Add(scale);
                        }
                    }

                }
                if (log)
                    Debug.Log("Ranges loaded: " + MPTKChordLib.chords.Count);
            }
        }

        //private void BuildChord(bool log = false)
        //{
        //    if (chord == null)
        //    {
        //        try
        //        {
        //            chord = new int[Count];
        //            int iEcart = 0;
        //            int vEcart = 1;
        //            chord[0] = 0;
        //            iEcart++;
        //            for (int i = 1; i < position.Length; i++)
        //            {
        //                if (position[i].Trim().Length == 0)
        //                {
        //                    vEcart++;
        //                }
        //                else
        //                {
        //                    chord[iEcart] = vEcart;
        //                    iEcart++;
        //                    vEcart += 1;
        //                }
        //            }
        //            //octave[octave.Length - 1] = 12;
        //        }
        //        catch (System.Exception ex)
        //        {
        //            MidiPlayerGlobal.ErrorDetail(ex);
        //        }

        //        if (log)
        //        {
        //            string info = string.Format("Range:{0} '{1}'", FlagMm, Name);
        //            foreach (int e in chord)
        //                info += string.Format(" [{0} {1}]", e, HelperNoteLabel.LabelFromMidi(48 + e));
        //            Debug.Log(info);
        //        }
        //    }
        //}
    }
}
