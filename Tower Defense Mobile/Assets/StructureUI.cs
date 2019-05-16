using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureUI : MonoBehaviour
{
    private Structure target;
    [HideInInspector] public GameObject view;

    public void setTargetStructure(Structure targetStructure)
    {
        if (targetStructure == null)
            return;

        target = targetStructure;

        Vector3 offset = new Vector3(0, -1.1f, 0);
        transform.position = targetStructure.transform.position;
        transform.position += offset;
    }

    public void setVisibility(bool vis)
    {
        if (view.activeSelf != vis)
            view.SetActive(vis);
    }

    public void UpgradeTarget()
    {
        //docelowo uruchamiać to raczej z MapManager tak żeby móc sprawdzać czy gracza stać na ulepszenie
        if (target != null)
        {
            target.Upgrade();
        }
    }
}
