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
    public float weight = 1.0f;//整体权重

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

        //物体指向前的轴向
        private Vector3 axis;
        //初始旋转，局部坐标系下，或者可以记录Root坐标系下的
        private Quaternion initialLocalRotation;
        //记录上一帧的旋转,用于计算缓动，在世界坐标系下
        private Quaternion lastFrameRotation;
        //记录动画机的旋转，在Root坐标系下
        private Quaternion animatorRotation;
        //修正模式下记录修正的缓动结果
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
        /// 指定旋转模式
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="weight"></param>
        /// <param name="root"></param>
        /// <param name="speed"></param>
        public void LookAt(Vector3 direction, float weight , Transform root , float speed)
        {
            //恢复到动画机的姿态
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
        /// 叠加动画模式
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="weight"></param>
        /// <param name="root"></param>
        /// <param name="speed"></param>
        public void Rotate(Vector3 direction, float weight, Transform root, float speed)
        {
            //恢复到动画机的姿态
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
        /// 获取物体向前的轴向，世界坐标系
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
        
        //注视条件判断        
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
        
        
        //记录动画机
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
