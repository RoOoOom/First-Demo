/********************************************************************
	created:	2016/01/18
	author :	张呈鹏
    company:    深圳自游网络有限公司
	purpose:	镜头跟随英雄，球面坐标系
*********************************************************************/
using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public float offsetY_ = 0.5F;
    public float alpha_ = 0.81F;
    public float beta_ = 0.0F;
    public float distance_ = 16.0F;
    public Transform target_ = null;
    public Vector3 map_right_bottom_ = Vector3.zero;
    public Vector3 camera_inner_left_top_ = Vector3.zero;
    public Vector3 camera_inner_right_bottom_ = Vector3.zero;
    // alpha interpolation
    // Use this for initialization
    void Start()
    {
        ReSet_();
	}

    private void ReSet_()
    {
        if(null == target_)
        {
            return;
        }
        transform.position = GetDestination();
        //
        //Vector3 target = target_.position;
        //LogCenter.Log(target.ToString());
        //target.y += offsetY_;
        //transform.LookAt(target);
    }
    
    Vector3 GetDestination()
    { 
        Vector3 p = target_.position;
        p.y += distance_ * Mathf.Cos(alpha_);
        p.x += distance_ * Mathf.Sin(alpha_) * Mathf.Sin(beta_);
        p.z -= distance_ * Mathf.Sin(alpha_) * Mathf.Cos(beta_);
        // 约束边缘
        if (p.x < camera_inner_left_top_.x)
        {
            p.x = camera_inner_left_top_.x;
        }
        if (p.x > camera_inner_right_bottom_.x)
        {
            p.x = camera_inner_right_bottom_.x;
        }
        if (p.z > camera_inner_left_top_.z)
        {
            p.z = camera_inner_left_top_.z;
        }
        if (p.z < camera_inner_right_bottom_.z)
        {
            p.z = camera_inner_right_bottom_.z;
        }

        return p;
    }
    public void OnEnable()
    {
        ReSet_();
    }
	// Update is called once per frame
	void Update () 
    {
        if(null == target_)
        {
            return;
        }
        {

            Vector3 current = transform.position;
            Vector3 dst = GetDestination();
            //transform.position = Vector3.SmoothDamp(current, dst, ref tCameraSpeed_, speed_);
            transform.position = dst;
        }
	}
}
