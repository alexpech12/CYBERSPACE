using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyberBuildingProc : MonoBehaviour
{
    public Transform sectionPrefab;

    public Material buildMaterial; // The material to use during construction.
    public List<Material> materialList; // List to choose final materials from.

    public int sectionNum = 3;
    public int startWidth = 10;
    public int startLength = 10;

    // Start is called before the first frame update
    int width;
    int length;
    int x = 0;
    int y = 0;
    void Start()
    {
        width = startWidth;
        length = startLength;

    }

    // Update is called once per frame
    int section = 0;
    bool complete = false;
    bool readyForNextSection = true;
    int height = 0;
    CyberBuildingProcSection currentSection;
    void Update()
    {
        if(section < sectionNum)
        {
            if (readyForNextSection)
            {
                var newSectionInst = Instantiate(sectionPrefab, transform);
                currentSection = newSectionInst.GetComponent<CyberBuildingProcSection>();
                currentSection.floorNum = Random.Range(5, 15);
                currentSection.width = width;
                currentSection.length = length;

                // Position section randomly on top
                newSectionInst.localPosition = new Vector3(y, height, x);

                width = Random.Range(Mathf.Clamp(width - 4, 1, width + 1), width + 1);
                length = Random.Range(Mathf.Clamp(length - 4, 1, length + 1), length + 1);

                x += Random.Range(0,currentSection.width - width+1);
                y += Random.Range(0, currentSection.length - length + 1);

                //y += Random.Range(0, width - currentSection.width);
                //x += Random.Range(0, length - currentSection.length);

                height += currentSection.floorNum;
                //width = currentSection.width;
                //length = currentSection.length;

                currentSection.initialMaterial = buildMaterial;
                currentSection.finalMaterial = materialList[Random.Range(0, materialList.Count)];


                readyForNextSection = false;

                //public int width = 3;
                //public int length = 3;

                //public Material initialMaterial;
                //public Material finalMaterial;
            }
            else
            {
                // Waiting for section to complete
                if(currentSection.Complete())
                {
                    readyForNextSection = true;
                    ++section;
                }
            }
        }
        else
        {
            // Building is done.
        }
    }
}
