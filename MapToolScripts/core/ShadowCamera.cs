using UnityEngine;
using System.Collections;

public class ShadowCamera : MonoBehaviour {
    public Camera sc_;
    public float distance_ = 6.26F;
    public float alpha_ = 0.94F;
    public float beta_ = 3.63F;
    public Projector projector_;
    public GameObject hero_;
	// Use this for initialization
	void Start () 
    {
        //distance_ = Camera.main.gameObject.GetComponent<CameraController>().distance_ * 30.0f / 12.0f;
	}
	
	// Update is called once per frame
	void Update () {
        //
        if (hero_ == null)
        {
            return;
        }
        Vector3 rel = Vector3.zero;
        rel.y = Mathf.Cos(alpha_);
        rel.x = Mathf.Sin(alpha_) * Mathf.Cos(beta_);
        rel.z = Mathf.Sin(alpha_) * Mathf.Sin(beta_);
        gameObject.transform.position = hero_.transform.position + rel * distance_;
        sc_.transform.LookAt(hero_.transform.position);
        sc_.fieldOfView = Camera.main.fieldOfView;
        projector_.transform.position = gameObject.transform.position;
        projector_.transform.rotation = gameObject.transform.rotation;
        projector_.fieldOfView = sc_.fieldOfView;
	}
}
