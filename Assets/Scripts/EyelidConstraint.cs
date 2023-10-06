using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyelidConstraint : MonoBehaviour
{

    public Transform eyeBone;
    public Transform headBone;
    public Vector3 eyeForwardLocal;

    private Quaternion initialRotation;
    public float rotate;

    
    // Start is called before the first frame update
    void Start()
    {
        initialRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        //�۾���������ͷ������ϵ�µ�����
        Vector3 eyeForward = eyeBone.TransformDirection(eyeForwardLocal.normalized);
        Vector3 dir = headBone.InverseTransformDirection(eyeForward);

        Debug.Log(dir.x);

        transform.localRotation = Quaternion.AngleAxis(dir.x * rotate, transform.rotation * Vector3.forward) * initialRotation;

        //��������ֱ�����ϵĽǶȱ仯

    }
}
