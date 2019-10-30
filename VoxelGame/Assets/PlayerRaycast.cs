using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRaycast : MonoBehaviour
{
    public RectTransform CurrentBlockIndicator;
    public Vector3 startingPosition;
    public Vector3 shiftIndicatorAmount;

    private int CurrentBlock;
    public float mouseValue;
    RaycastHit rayCastHit;
    int layerMask = ~(1 << 2);
    private void Start()
    {
        startingPosition = CurrentBlockIndicator.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out rayCastHit, 10, layerMask))
            {
                rayCastHit.collider.GetComponent<ChunkGenerator>().DestroyBlock(rayCastHit, transform.TransformDirection(Vector3.forward));
            }
        }
        else if (Input.GetButtonDown("Fire2"))
        {
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out rayCastHit, 10, layerMask))
            {
                if(rayCastHit.distance < 2.5) return;
                rayCastHit.collider.GetComponent<ChunkGenerator>().AddBlock(rayCastHit, transform.TransformDirection(Vector3.forward), false, CurrentBlock);
            }
        }
        float currentValue = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(currentValue) > 0.01f)
        {
            if (currentValue > 0)
                ShiftCurrentBlock(1);
            else
                ShiftCurrentBlock(-1);
        }
    }

    public void ShiftCurrentBlock(int shiftBy)
    {
        CurrentBlock += shiftBy;
        if (CurrentBlock > 3) CurrentBlock = 0;
        if (CurrentBlock < 0) CurrentBlock = 3;
        CurrentBlockIndicator.position = startingPosition + CurrentBlock * shiftIndicatorAmount;


    }

}
