using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class MusicControl : MonoBehaviour
{
    public AudioSource C_Note;
    public AudioSource Cs_Note;
    public AudioSource D_Note;
    public AudioSource Ds_Note;
    public AudioSource E_Note;
    public AudioSource F_Note;
    public AudioSource Fs_Note;
    public AudioSource G_Note;
    public AudioSource Gs_Note;
    public AudioSource A_Note;
    public AudioSource B_Note;
    public AudioSource Bs_Note;
    public AudioSource C1_Note;

    private void OnEnable() {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // VisualElement white_keys = root.Query<VisualElement>("white_keys");
        // VisualElement black_keys_1 = root.Query<VisualElement>("black_keys_1");
        // VisualElement black_keys_2 = root.Query<VisualElement>("black_keys_2");


        Button C_Button = root.Query<Button>("C");
        Debug.Log(C_Button);
        // Button buttonCenter = root.Q<Button>("Center");
        // Button buttonFrontLeft = root.Q<Button>("FrontLeft");
        // Button buttonLeft = root.Q<Button>("Left");
        // Button buttonFrontRight = root.Q<Button>("FrontRight");
        // Button buttonRight = root.Q<Button>("Right");

        C_Button.clicked += () => Cpressed();

        void Cpressed(){
            Debug.Log("Preess");
        }

    }
}
