using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class MusicControl : MonoBehaviour
{   
    public AudioSource Source;
    public AudioClip C_Note;
    public AudioClip Cs_Note;
    public AudioClip D_Note;
    public AudioClip Ds_Note;
    public AudioClip E_Note;
    public AudioClip F_Note;
    public AudioClip Fs_Note;
    public AudioClip G_Note;
    public AudioClip Gs_Note;
    public AudioClip A_Note;
    public AudioClip B_Note;
    public AudioClip Bb_Note;
    public AudioClip C1_Note;

    private void OnEnable() {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        

        root.Q<Button>("C").clicked += () => {
            Source.PlayOneShot(C_Note);
        };

        root.Q<Button>("Cs").clicked += () => {
            Source.PlayOneShot(Cs_Note);
        };

        root.Q<Button>("D").clicked += () => {
            Source.PlayOneShot(D_Note);
        };
        
        root.Q<Button>("Ds").clicked += () => {
            Source.PlayOneShot(Ds_Note);
        };

        root.Q<Button>("E").clicked += () => {
            Source.PlayOneShot(E_Note);
        };
        
        root.Q<Button>("F").clicked += () => {
            Source.PlayOneShot(F_Note);
        };
        
        root.Q<Button>("Fs").clicked += () => {
            Source.PlayOneShot(Fs_Note);
        };

        root.Q<Button>("G").clicked += () => {
            Source.PlayOneShot(G_Note);
        };
        
        root.Q<Button>("Gs").clicked += () => {
            Source.PlayOneShot(Gs_Note);
        };

        root.Q<Button>("A").clicked += () => {
            Source.PlayOneShot(A_Note);
        };
        
        root.Q<Button>("Bb").clicked += () => {
            Source.PlayOneShot(Bb_Note);
        };
        
        root.Q<Button>("B").clicked += () => {
            Source.PlayOneShot(B_Note);
        };
        
        root.Q<Button>("C1").clicked += () => {
            Source.PlayOneShot(C1_Note);
        };
        

    }
}
