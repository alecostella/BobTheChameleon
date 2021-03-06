﻿using UnityEngine;

public class Camouflage : MonoBehaviour
{

    [SerializeField]
    private float camouflageTime; //Should last as long as the animation
    [SerializeField]
    private Animator animator;

    private bool isCamouflaged;
    //private bool startedCamouflage;
    //private bool canCamouflage;

    private void OnEnable()
    {
        isCamouflaged = false;
        //startedCamouflage = false;
        //canCamouflage = false;
        //StartCoroutine(WaitForCamouflage());
    }
    /// <summary>
    /// Called after camouflageTime sets the boolean to true
    /// </summary>
    private void FinishCamouflage()
    {
        isCamouflaged = true;
    }

    /// <summary>
    /// Used to enable camouflage only where it's allowed or disable it
    /// </summary>
    /// <param name="value">True if player is in a place where it can camouflage</param>
    public void SetCamouflageFlag(bool value)
    {
        //canCamouflage = value;
        if(value)
            Invoke("FinishCamouflage", camouflageTime);
        else
            isCamouflaged = false;

        animator.SetBool("Camo", value);

    }

    public bool IsCamouflaged()
    {
        return isCamouflaged;
    }

    #region Old Camouflage
    //private IEnumerator WaitForCamouflage()
    //{
    //    while(!startedCamouflage)
    //    {
    //        //If C is pressed and isn't camouflaged
    //        if(Input.GetKey(KeyCode.C) && !startedCamouflage && canCamouflage)
    //        {
    //            startedCamouflage = true;
    //            Invoke("FinishCamouflage", camouflageTime);
    //            animator.SetBool("Camo", true);
    //        }
    //        yield return null;
    //    }
    //    StopAllCoroutines();
    //    StartCoroutine(WaitForAnotherKey());
    //}

    //private IEnumerator WaitForAnotherKey()
    //{
    //    while(startedCamouflage)
    //    {
    //        if((Input.anyKeyDown || !canCamouflage) && !Input.GetKeyDown(KeyCode.C))//Any other key breaks the camouflagement
    //        {
    //            isCamouflaged = false;
    //            startedCamouflage = false;
    //            animator.SetBool("Camo", false);
    //        }
    //        yield return null;
    //    }
    //    StopAllCoroutines();
    //    StartCoroutine(WaitForCamouflage());
    //}
    #endregion
}
