using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using UnityEngine.Events;
using MEC;

namespace MidiPlayerTK
{
    public partial class MidiFilePlayer : MidiSynth
    {
        /// <summary>@brief
        /// [MPTK PRO] Find a Midi in the Unity resources folder MidiDB which contains the name (case sensitive)\n
        /// Tips: Add Midi files to your project with the Unity menu MPTK or add it directly in the ressource folder and open Midi File Setup to automatically integrate Midi in MPTK.
        /// @code
        /// // Find the first Midi file name in MidiDB which contains "Adagio"
        /// midiFilePlayer.MPTK_SearchMidiToPlay("Adagio");
        /// // And play it
        /// midiFilePlayer.MPTK_Play();
        /// @endcode
        /// </summary>
        /// <param name="name">case sensitive part of a midi file name</param>
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
                            Debug.LogWarningFormat("No Midi file found with '{0}' in name", name);
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


        /// <summary>@brief
        /// [MPTK PRO] Play next or previous Midi from the MidiDB list.
        /// </summary>
        /// <param name="offset">Forward or backward count in the list. 1:the next, -1:the previous</param>
        public void MPTK_PlayNextOrPrevious(int offset)
        {
            try
            {
                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                {
                    int selectedMidi = MPTK_MidiIndex + offset;
                    if (selectedMidi < 0)
                        selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count - 1;
                    else if (selectedMidi >= MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count)
                        selectedMidi = 0;
                    MPTK_MidiIndex = selectedMidi;
                    if (offset < 0)
                        prevMidi = true;
                    else
                        nextMidi = true;
                    MPTK_RePlay();
                }
                else
                    Debug.LogWarning(MidiPlayerGlobal.ErrorNoMidiFile);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>@brief
        /// [MPTK PRO] Switch playing between two MIDIs with ramp-up.\n
        /// This method is useful for an integration with Bolt: main MIDI parameters are defined in one call.
        /// </summary>
        /// <param name="index">Index of the MIDI to play. Index is used only if name parameter is null or empty.</param>
        /// <param name="name">Name of the MIDI to play. Can be part of the MIDI Name. If set, this parameter has the priority over index parameter.</param>
        /// <param name="volume">Volume of the MIDI. -1 to not change the default volume</param>
        /// <param name="delayToStopMillisecond">Delay to stop the current MIDI playing (with volume decrease) or delay before playing the MIDI if no MIDI is playing</param>
        /// <param name="delayToStartMillisecond">Delay to get the MIDI at full volume (ramp-up volume)</param>
        public void MPTK_SwitchMidiWithDelay(int index, string name, float volume, float delayToStopMillisecond, float delayToStartMillisecond)
        {
            if (volume >= 0f)
                MPTK_Volume = volume;
            //Debug.Log($"Search for {name}");
            if (delayToStopMillisecond < 0f) delayToStopMillisecond = 0f;
            if (delayToStartMillisecond < 0f) delayToStartMillisecond = 0f;
            MPTK_Stop(delayToStopMillisecond);

            if (!string.IsNullOrWhiteSpace(name))
                MPTK_SearchMidiToPlay(name);
            else
                MPTK_MidiIndex = index;

            Routine.RunCoroutine(TheadPlayWithDelay(delayToStopMillisecond, delayToStartMillisecond), Segment.RealtimeUpdate);
        }

        /// <summary>@brief
        /// [MPTK PRO] Play the midi file defined with MPTK_MidiName or MPTK_MidiIndex with ramp-up to the volume defined with MPTK_Volume.\n
        /// The time to get a MIDI playing at full MPTK_Volume is delayRampUp + startDelay.\n
        /// A delayed start can also be set.
        /// </summary>
        /// <param name="delayRampUp">ramp-up delay in milliseconds to get the default volume</param>
        /// <param name="startDelay">delayed start in milliseconds V2.89.1</param>
        public virtual void MPTK_Play(float delayRampUp, float startDelay = 0)
        {
            Routine.CallDelayed(startDelay / 1000f, () =>
            {
                //Debug.Log("Delayed play");
                needDelayToStop = false;
                delayRampUpSecond = delayRampUp / 1000f;
                timeRampUpSecond = Time.realtimeSinceStartup + delayRampUpSecond;
                needDelayToStart = true;
                MPTK_Play();
            });
        }
        /// <summary>@brief
        /// Play the midi file from a byte array.\n
        /// Look at MPTK_StatusLastMidiLoaded to get status.
        /// @code
        ///   // Example of using with Windows or MACOs
        ///   using (Stream fsMidi = new FileStream(filepath, FileMode.Open, FileAccess.Read))
        ///   {
        ///         byte[] data = new byte[fsMidi.Length];
        ///         fsMidi.Read(data, 0, (int) fsMidi.Length);
        ///         midiFilePlayer.MPTK_Play(data);
        ///   }
        /// @endcode
        /// </summary>
        public void MPTK_Play(byte[] data)
        {
            try
            {
                MPTK_StatusLastMidiLoaded = LoadingStatusMidiEnum.NotYetDefined;

                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    playPause = false;

                    if (!MPTK_IsPlaying)
                    {
                        if (data == null)
                        {
                            //Debug.LogWarning("MPTK_Play: set MPTK_MidiName or Midi Url/path in inspector before playing");
                            MPTK_StatusLastMidiLoaded = LoadingStatusMidiEnum.MidiNameNotDefined;
                        }
                        else if (data.Length < 4)
                        {
                            MPTK_StatusLastMidiLoaded = LoadingStatusMidiEnum.TooShortSize;
                            //Debug.LogWarning($"Error Loading Midi:{pathmidiNameToPlay} - Not a midi file, too short size");
                        }
                        else if (System.Text.Encoding.Default.GetString(data, 0, 4) != "MThd")
                        {
                            MPTK_StatusLastMidiLoaded = LoadingStatusMidiEnum.NoMThdSignature;
                            //Debug.LogWarning($"Error Loading Midi:{pathmidiNameToPlay} - Not a midi file, signature MThd not found");
                        }
                    }
                    else
                    {
                        //Debug.LogWarning("Already playing - " + pathmidiNameToPlay);
                        MPTK_StatusLastMidiLoaded = LoadingStatusMidiEnum.AlreadyPlaying;
                    }
                }
                else
                {
                    //Debug.LogWarning("SoundFont not loaded");
                    MPTK_StatusLastMidiLoaded = LoadingStatusMidiEnum.SoundFontNotLoaded;
                }

                // If no error, load and play the midi in background
                if (MPTK_StatusLastMidiLoaded == LoadingStatusMidiEnum.NotYetDefined)
                {
                    MPTK_InitSynth();
                    MPTK_StartSequencerMidi();

                    // Start playing
                    if (MPTK_CorePlayer)
                        Routine.RunCoroutine(ThreadCorePlay(data).CancelWith(gameObject), Segment.RealtimeUpdate);
                    else
                        Routine.RunCoroutine(ThreadLegacyPlay(data).CancelWith(gameObject), Segment.RealtimeUpdate);
                }
                else
                {
                    try
                    {
                        OnEventEndPlayMidi.Invoke(MPTK_MidiName, EventEndMidiEnum.MidiErr);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("OnEventEndPlayMidi: exception detected. Check the callback code");
                        Debug.LogException(ex);
                    }
                }
            }

            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
        /// <summary>@brief
        /// [MPTK PRO] Play the midi from a MidiFileWriter2 object
        /// </summary>
        /// <param name="mfw2">aMidiFileWriter2 object</param>
        /// <param name="delayRampUp"></param>
        public void MPTK_Play(MidiFileWriter2 mfw2, float delayRampUp = 0f)
        {
            try
            {
                MPTK_StatusLastMidiLoaded = LoadingStatusMidiEnum.NotYetDefined;

                if (delayRampUp > 0f)
                {
                    delayRampUpSecond = delayRampUp / 1000f;
                    timeRampUpSecond = Time.realtimeSinceStartup + delayRampUpSecond;
                    needDelayToStart = true;
                }
                else
                    needDelayToStart = false;

                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    playPause = false;

                    if (!MPTK_IsPlaying)
                    {
                        if (mfw2 == null)
                        {
                            //Debug.LogWarning("MPTK_Play: set MPTK_MidiName or Midi Url/path in inspector before playing");
                            MPTK_StatusLastMidiLoaded = LoadingStatusMidiEnum.MidiNameNotDefined;
                        }
                        //else if (mfw2.Length < 4)
                        //{
                        //    MPTK_StatusLastMidiLoaded = LoadingStatusMidiEnum.TooShortSize;
                        //    //Debug.LogWarning($"Error Loading Midi:{pathmidiNameToPlay} - Not a midi file, too short size");
                        //}
                    }
                    else
                    {
                        //Debug.LogWarning("Already playing - " + pathmidiNameToPlay);
                        MPTK_StatusLastMidiLoaded = LoadingStatusMidiEnum.AlreadyPlaying;
                    }
                }
                else
                {
                    //Debug.LogWarning("SoundFont not loaded");
                    MPTK_StatusLastMidiLoaded = LoadingStatusMidiEnum.SoundFontNotLoaded;
                }

                // If no error, load and play the midi in background
                if (MPTK_StatusLastMidiLoaded == LoadingStatusMidiEnum.NotYetDefined)
                {
                    MPTK_InitSynth();
                    MPTK_StartSequencerMidi();
                    midiNameToPlay = string.IsNullOrEmpty(mfw2.MPTK_MidiName) ? "" : mfw2.MPTK_MidiName;
                    // Start playing
                    Routine.RunCoroutine(ThreadMFWPlay(mfw2).CancelWith(gameObject), Segment.RealtimeUpdate);
                }
                else
                {
                    try
                    {
                        OnEventEndPlayMidi.Invoke(mfw2.MPTK_MidiName, EventEndMidiEnum.MidiErr);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("OnEventEndPlayMidi: exception detected. Check the callback code");
                        Debug.LogException(ex);
                    }
                }
            }

            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>@brief
        /// [MPTK PRO] Stop playing within a delay. After the stop delay (0 by default), the volume decrease until the playing is stopped.\n
        /// The time to get a real MIDI stop is delayRampDown + stopDelay.
        /// </summary>
        /// <param name="delayRampDown">decrease time in millisconds</param>
        /// <param name="stopDelay">delayed stop in milliseconds V2.89.1</param>
        public virtual void MPTK_Stop(float delayRampDown, float stopDelay = 0)
        {
            Routine.CallDelayed(stopDelay / 1000f, () =>
            {
                if (midiLoaded != null && midiIsPlaying)
                {
                    needDelayToStart = false;
                    delayRampDnSecond = delayRampDown / 1000f;
                    timeRampDnSecond = Time.realtimeSinceStartup + delayRampDnSecond;
                    needDelayToStop = true;
                }
            });
        }

        //! @cond NODOC
        public void StopAndPlayMidi(int index, string name)
        {
            MPTK_Stop();
            if (!string.IsNullOrWhiteSpace(name))
                MPTK_SearchMidiToPlay(name);
            else
                MPTK_MidiIndex = index;
            MPTK_Play();
        }

        protected IEnumerator<float> TheadPlayWithDelay(float delayToStopMillisecond, float delayToStartMillisecond)
        {
            //Debug.Log($"TheadPlayWithDelay for {delayToStopMillisecond}");
            yield return Routine.WaitForSeconds((delayToStopMillisecond + 100f) / 1000f);
            //Debug.Log($"TheadPlayWithDelay play {delayToStartMillisecond}");
            MPTK_Play(delayToStartMillisecond);
        }

        public void PlayAndPauseMidi(int index, string name, int pauseMillisecond = -1)
        {
            MPTK_Stop();
            if (!string.IsNullOrWhiteSpace(name))
                MPTK_SearchMidiToPlay(name);
            else
                MPTK_MidiIndex = index;
            MPTK_Play();
            MPTK_Pause(pauseMillisecond);
        }

        protected IEnumerator<float> ThreadMFWPlay(MidiFileWriter2 mfw2, float fromPosition = 0, float toPosition = 0)
        {
            StartPlaying();
            string currentMidiName = midiNameToPlay;
            //Debug.Log("Start play " + fromPosition + " " + toPosition);
            try
            {

                midiLoaded = new MidiLoad();
                midiLoaded.KeepNoteOff = MPTK_KeepNoteOff;
                midiLoaded.MPTK_KeepEndTrack = MPTK_KeepEndTrack;
                midiLoaded.MPTK_EnableChangeTempo = MPTK_EnableChangeTempo;
                midiLoaded.LogEvents = MPTK_LogEvents;
                if (!midiLoaded.MPTK_Load(mfw2))
                    midiLoaded = null;
#if DEBUG_START_MIDI
                Debug.Log("After load midi " + (double)watchStartMidi.ElapsedTicks / ((double)System.Diagnostics.Stopwatch.Frequency / 1000d));
#endif
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }

            Routine.RunCoroutine(ThreadInternalMidiPlaying(currentMidiName, fromPosition, toPosition).CancelWith(gameObject), Segment.RealtimeUpdate);
            yield return 0;
        }
        //! @endcond


    }
}

