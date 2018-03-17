using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveController : MonoBehaviour {
    public Transform m_target;
    private float m_distance = 20f;
    float Speed = 50f;

    private Camera m_mainCam;
	// Use this for initialization
	void Start () {
        if(m_target != null)
            this.transform.position = new Vector3(m_target.position.x, m_target.position.y + m_distance, m_target.position.z);

        m_mainCam = this.transform.GetComponent<Camera>();
        if (m_mainCam == null)
        {
            Debug.LogError("找不到摄像机组件");
        }
    }
	
	// Update is called once per frame
	void Update () {
        float hori = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        this.transform.Translate(new Vector3(hori * Time.deltaTime * Speed, 0 , vert * Time.deltaTime * Speed));

        if (m_mainCam == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = m_mainCam.ScreenPointToRay(Input.mousePosition);
            Debug.Log(m_mainCam.ScreenToViewportPoint(Input.mousePosition));
            RaycastHit rcs;
            if (Physics.Raycast(ray, out rcs))
            {
                m_target.GetComponent<NavMeshAgent>().SetDestination(rcs.point);
            }
        }
	}
}
