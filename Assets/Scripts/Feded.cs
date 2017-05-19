using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feded : MonoBehaviour {

	public void FadeMe()
    {
        StartCoroutine(Dofade());
    }
    IEnumerator Dofade()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        while(canvasGroup.alpha>0)
        {
            canvasGroup.alpha -= Time.deltaTime/2;
            yield return null;
        }
        canvasGroup.interactable = false;
        yield return null;

    }
		
}
