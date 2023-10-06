using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LookAt : MonoBehaviour
{

    public Transform target;

    public float lookAtAngle = 40.0f;
    public bool isLookAt = false;
    private float IKweight = 0.0f;

    [Range(0.0f,1.0f)]
    public float weight = 1.0f;//����Ȩ��

    public Transform root;
    
    public LookAtBone head;    
    public List<LookAtBone> spine;
    public List<LookAtBone> eyes;

    public float headSpeed = 2.0f;
    public float eyeSpeed = 2.0f;

    //public float bodyWeight = 1.0f;
    //public float headWeight = 1.0f;
    //public float eyesWeight = 1.0f;

    //private LookAtBone[] spine = new LookAtBone[0];
    //private LookAtBone head = new LookAtBone();
    //private LookAtBone[] eyes = new LookAtBone[0];



    [System.Serializable]
    public class LookAtBone
    {
                
        public Transform transform;        

        public float weight;

        public Vector3 fixAngle;

        //����ָ��ǰ������
        private Vector3 axis;
        //��ʼ��ת���ֲ�����ϵ�£����߿��Լ�¼Root����ϵ�µ�
        private Quaternion initialLocalRotation;
        //��¼��һ֡����ת,���ڼ��㻺��������������ϵ��
        private Quaternion lastFrameRotation;
        //��¼����������ת����Root����ϵ��
        private Quaternion animatorRotation;
        //����ģʽ�¼�¼�����Ļ������
        private Quaternion fixRotation;

        public LookAtBone() { }

        public LookAtBone(Transform transform)
        {
            this.transform = transform;
        }


        public void Initiate(Transform root)
        {
            if (transform == null) return;

            axis = Quaternion.Inverse(transform.rotation) * root.forward;
            initialLocalRotation = transform.localRotation;
            lastFrameRotation = transform.rotation;
            fixRotation = Quaternion.identity;
        }

        public void StoreAnimatorRotation(Transform root)
        {
            animatorRotation = Quaternion.Inverse(root.rotation) * transform.rotation;
        }

        
        /// <summary>
        /// ָ����תģʽ
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="weight"></param>
        /// <param name="root"></param>
        /// <param name="speed"></param>
        public void LookAt(Vector3 direction, float weight , Transform root , float speed)
        {
            //�ָ�������������̬
            transform.rotation = root.rotation * animatorRotation;
            
            Quaternion fromTo = Quaternion.FromToRotation(forward, direction);
            Quaternion r = transform.rotation;

            //transform.rotation = Quaternion.Slerp(r, fromTo * r, weight);
            Quaternion targetQuaternion = Quaternion.Lerp(r, fromTo * r, weight);

            float Angle = Quaternion.Angle(lastFrameRotation, targetQuaternion);

            if (Angle < speed * Time.deltaTime)
            {
                transform.rotation = targetQuaternion;

            }
            else
            {
                transform.rotation = Quaternion.Slerp(lastFrameRotation, targetQuaternion, speed * Time.deltaTime);

            }

            lastFrameRotation = transform.rotation;

        }


        /// <summary>
        /// ���Ӷ���ģʽ
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="weight"></param>
        /// <param name="root"></param>
        /// <param name="speed"></param>
        public void Rotate(Vector3 direction, float weight, Transform root, float speed)
        {
            //�ָ�������������̬
            transform.rotation = root.rotation * animatorRotation;

            Quaternion fromTo = Quaternion.FromToRotation(forward, direction);

            Quaternion targetQuaternion = Quaternion.Lerp(Quaternion.identity, fromTo, weight);

            float Angle = Quaternion.Angle(fixRotation, targetQuaternion);

            if (Angle < speed * Time.deltaTime)
            {
                fixRotation = targetQuaternion;

            }
            else
            {
                fixRotation = Quaternion.Slerp(fixRotation, targetQuaternion, speed * Time.deltaTime);

            }

            transform.rotation = fixRotation * transform.rotation;


        }

        
        /// <summary>
        /// ��ȡ������ǰ��������������ϵ
        /// </summary>
        public Vector3 forward
        {
            get
            {
                return transform.rotation * axis;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (spine != null)
        {
            foreach (LookAtBone bone in spine)
            {
                bone.Initiate(root);
            }
        }

        if (head != null) head.Initiate(root);

        if (eyes != null)
        {
            foreach (LookAtBone bone in eyes)
            {
                bone.Initiate(root);
            }
        }
    }

    /*
    // Update is called once per frame
    void Update()
    {
        
    }*/

    private void LateUpdate()
    {

        if (root == null)
            return;

        if (head == null)
            return;
        
        //ע�������ж�        
        Vector3 lookdir = (target.position - head.transform.position).normalized;
        float lookangle = Vector3.Angle(lookdir, root.forward);

        if (lookangle < lookAtAngle)
        {
            IKweight = 1.0f;
        }
        else
        {
            IKweight = 0.0f;
        }
        
        
        //��¼������
        if (spine != null)
        {
            foreach (LookAtBone bone in spine)
            {
                bone.StoreAnimatorRotation(root);
            }
        }

        if (head != null) head.StoreAnimatorRotation(root);

        if (eyes != null)
        {
            foreach (LookAtBone bone in eyes)
            {
                bone.StoreAnimatorRotation(root);
            }
        }



        SolveSpine();

        SolveHead();

        SolveEyes();

    }



    public void SolveSpine()
    {
        for (int i = 0; i < spine.Count; i++)
        {
            Vector3 lookdir = (target.position - spine[i].transform.position).normalized;

            float moveWeight = weight * spine[i].weight * IKweight;

            //spine[i].LookAt(lookdir, moveWeight, root, headSpeed);
            spine[i].Rotate(lookdir, moveWeight, root, headSpeed);
        }
    }

    public void SolveHead()
    {

        Vector3 lookdir = (target.position - head.transform.position).normalized;

        float moveWeight = weight * head.weight * IKweight;

        //head.LookAt(lookdir, moveWeight, root, headSpeed);
        head.Rotate(lookdir, moveWeight, root, headSpeed);

    }

    public void SolveEyes()
    {
        for (int i = 0; i < eyes.Count; i++)
        {
            Vector3 lookdir = (target.position - eyes[i].transform.position).normalized;

            float moveWeight = weight * eyes[i].weight * IKweight;

            eyes[i].LookAt(lookdir, moveWeight, root, eyeSpeed);
        }
    }


    private void OnDrawGizmosSelected()
    {

        Handles.color = Color.green;
        if (head != null)
        {
            Handles.DrawLine(head.transform.position, target.position);
        }

        Handles.color = Color.blue;
        if (spine != null)
        {
            for (int i = 0; i < spine.Count - 1; i++)
            {
                Handles.DrawLine(spine[i].transform.position, spine[i + 1].transform.position);
            }

            Handles.DrawLine(spine[spine.Count - 1].transform.position, head.transform.position);
        }

        
    }

    

}
