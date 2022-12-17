# rwis_unity_rep

Repository for development of a Unity-based Android app, as part of the Real-World Interactive System (RWIS) course dispensed by Prof. Yuta SUGIURA of Keio University (2022 Fall Semester)

# About the unity_branch

The unity_branch is supported by [API MPTK - Playing MIDI by script - Maestro - Midi Player Tool Kit](https://paxstellar.fr/api-mptk-v2/).

The basic idea is 

1. load midi files as a list<MPTKEvent>,

2. Control with keys,
   
   1. Key Down:
      
      1. Detect the number of keys,
      
      2. Form a onlist<MPTKEvent>,
      
      3. Play the onlist<MPTKEvent> and keep it on with duration = -1,
      
      4. Update the index of the list<MPTKEvent>,
   
   2. Key Up:
      
      1. Form a offlist<MPTKEvent>,
      
      2. Play the offlist<MPTKEvent> to cut off the note

2022.12.12:

Need to find way to precisely control Unity Input.



1. Key down
   
   1. Detect Key ID, Send Noteon to Lib (Unity)
   
   2. The Lib, 
