﻿using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Linq;
using TMPro;

public class Manual
{
    //Use reflection to get currentState, as it is a protected variable
    public static FieldInfo _state = typeof(SceneManager).GetField("currentState", BindingFlags.NonPublic | BindingFlags.Instance);
    public static MethodInfo _GetAllModuleManuals = typeof(ModManager).GetMethod("GetAllModuleManuals", BindingFlags.NonPublic | BindingFlags.Instance);
    public static MethodInfo _GetAllNeedyModuleManuals = typeof(ModManager).GetMethod("GetAllNeedyModuleManuals", BindingFlags.NonPublic | BindingFlags.Instance);
    public static MethodInfo _GetAllAppendixManuals = typeof(ModManager).GetMethod("GetAllAppendixManuals", BindingFlags.NonPublic | BindingFlags.Instance);
    public static Material fontmaterial;
    public static TMP_FontAsset font;
    internal static bool button;
    internal static SceneManager SM { get { return SceneManager.Instance; } }
    private ManualCheckerLoader Instance = ManualCheckerLoader.Instance;
    //Due to button placement, I've made a new texture to fit the buttons in

    internal void OnStateChange(KMGameInfo.State state)
    {
        if (!Instance.transform.gameObject.activeInHierarchy) return;
        if (state == KMGameInfo.State.Setup)
        {
            Instance.StartCoroutine(CheckForBrochure());
        }
    }

    private IEnumerator CheckForBrochure()
    {
        yield return null;
        
        //Get the SetupRoom to call the ModManagerHoldable
        SetupRoom setupRoom = (SetupRoom)SceneManager.Instance.CurrentRoom;
        ModManagerHoldable _brochure = setupRoom.ModManagerHoldable;
        //Grab ModButton and move it to give room for the new button
        Selectable ModButton = _brochure.OpenModManagerButton;
        ModButton.transform.Translate(0, 0, 0.01f);
        //Clone the ModButton
        Selectable ManualButton = Object.Instantiate(ModButton, ModButton.transform);
        //Grab the Brochure's selectable to add the new button to the list of its children selectables.
        Selectable Brochure = _brochure.GetComponent<Selectable>();
        //Actually move the new button
        ManualButton.transform.Translate(-0.005f, -0.002f, 0.005f);
        //Quickly grab the texture for the middle panel so I can replace it.
        MeshRenderer replace = _brochure.GetComponent<Transform>().Find("PanelMiddleBack").GetComponent<MeshRenderer>();
        replace.material.mainTexture = ManualCheckerLoader.Instance.brochureReplacement;
        Brochure.Children = Brochure.Children.Concat(new[] { ManualButton, null }).ToArray();
        //The text autosizing makes the text smaller, which I do not want. So for now, setting this to false.
        ManualButton.GetComponentInChildren<TextMeshPro>().enableAutoSizing = false;
        ManualButton.GetComponentInChildren<TextMeshPro>().text = "Manage Manuals";
        //Using this to deal with see through text in the Manual manager, maybe.
        fontmaterial = ManualButton.GetComponentInChildren<TextMeshPro>().fontMaterial;
        font = ManualButton.GetComponentInChildren<TextMeshPro>().font;
        /*Getting ready to call EnterModManagerStateFromSetup, however, I will be replacing modManagerScene with my own scene
        As such, I'll basically be copying this method here, and replacing the last line.*/
        ManualButton.OnInteract += delegate () { button = true; Debug.LogFormat("[Manual] " + button.ToString()); SceneManager.Instance.EnterModManagerStateFromSetup(); return false; };
    }

    private void OnInteract()
    {
        //CurrentState is protected, use reflection to change it
        _state.SetValue(SM, SceneManager.State.Transitioning);
        SM.SetupState.PrepareForExitState();
        LoadingOverlay.Instance.Enable();
        SM.FadeOut(SM.SetupState.FadeOutTime, delegate
        {
            SM.SaveData();
            SM.SetupState.ExitState();
            ManualManager ManualManager = new ManualManager();
            ManualManager.Start();
        });
    }
}