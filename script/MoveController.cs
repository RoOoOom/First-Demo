using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveController : MonoBehaviour {
    public enum ControlType{ God , Player };
    public ControlType m_ctrlType = ControlType.Player;
    public Transform m_target;
    private float m_distance = 20f;
    float Speed = 10f;
    float m_step = 2f;
    float m_rotateAngle = 1f;
    float m_rotaDir = 0;
    float m_alrotate = 0;
    public Vector3 m_offset = Vector3.zero;
    public Vector3 m_pushDir = Vector3.zero;
    private Vector3 m_originOffset;
    private Vector3 lastDir;
    private Vector3 curDir;
    private Camera m_mainCam;
    private NavMeshAgent m_chaCtrl;
    private Animator m_animator;
    private Transform m_camTrans;
    private Quaternion m_quater;
    private bool attack = false;
    private bool bowView = false;
	// Use this for initialization
	void OnEnable() {
        Debug.Log("STart");
        if (m_ctrlType == ControlType.God)
        {
            if (m_target != null)
                this.transform.position = new Vector3(m_target.position.x, m_target.position.y + m_distance, m_target.position.z);

            m_mainCam = this.transform.GetComponent<Camera>();
            if (m_mainCam == null)
            {
                Debug.LogError("找不到摄像机组件");
            }
        }

        if(m_ctrlType == ControlType.Player)
        {
            Debug.Log("Type " + m_ctrlType);
            m_camTrans = Camera.main.transform;
           
            m_camTrans.position = transform.position - transform.forward;
            m_camTrans.LookAt(transform);
            m_camTrans.position = transform.position + m_offset;
            m_quater = m_camTrans.rotation;
            curDir = lastDir = transform.position - m_camTrans.position;

            m_originOffset = m_offset;
            m_chaCtrl = GetComponent<NavMeshAgent>();

            m_animator = GetComponent<Animator>();
        }

        Debug.Log(transform.forward);
    }

    public void Update()
    {
        
    }

	// Update is called once per frame
	public void UpdateInput () {        
        float hori = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        //Debug.Log("horizontal Vertical " + hori + "  " + vert);
        if (m_ctrlType == ControlType.God)
        {
            this.transform.Translate(new Vector3(hori * Time.deltaTime * Speed, 0, vert * Time.deltaTime * Speed));

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

        if (Input.GetKeyDown(KeyCode.U))
        {
            m_animator.SetTrigger("Attack");
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            //m_animator.SetFloat("Speed", 0);
            attack = true;
            bowView = true;
            m_animator.SetTrigger("Ready");
            //StartCoroutine(PauseAction(16));
        }
        else if (Input.GetKeyUp(KeyCode.J))
        {
            attack = false;
            m_animator.SetTrigger("Attack");
        }

        if (m_ctrlType == ControlType.Player)
        {
            if (!m_chaCtrl.isActiveAndEnabled)
            {
                Debug.Log("NavMeshAgent is missing");
                return;
            }            

            if (Input.GetKey(KeyCode.Q))
            {
                m_alrotate = m_alrotate + m_rotateAngle * Time.deltaTime;
                m_rotaDir = m_rotateAngle;
            }                
            else if (Input.GetKey(KeyCode.E))
            {
                m_alrotate = m_alrotate - m_rotateAngle * Time.deltaTime;
                m_rotaDir = -m_rotateAngle;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_chaCtrl.Move(Vector3.right * 2);
            }
            else
            {
                Vector3 verVec = (m_camTrans.forward * vert).normalized;
                Vector3 horiVec = (m_camTrans.right * hori).normalized;
                Vector3 Direction = verVec + horiVec;
                Vector3 moveVec = (verVec + horiVec) * m_step * Time.deltaTime;
                //Debug.Log(moveVec);

                if (Direction != Vector3.zero && !attack)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Direction, Vector3.up), 500f * Time.deltaTime);
                }

                m_animator.SetFloat("Speed", Direction.sqrMagnitude);
                m_chaCtrl.Move(moveVec);
            }
            
            
            //GetComponent<NavMeshAgent>().Move(moveVec);
        }         
	}
    
    IEnumerator PauseAction(float rate)
    {
        AnimatorStateInfo info = m_animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log(info.IsName("Attack"));

        float time = 0f;
        float stand = info.length * rate;
        while (time < stand)
        {
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Debug.Log(time);
        
        yield return null;
    }
    
    IEnumerator PauseAction(int frame)
    {
        AnimatorStateInfo info = m_animator.GetCurrentAnimatorStateInfo(1);
        
        while (!info.IsName("Attack"))
        {
            info = m_animator.GetCurrentAnimatorStateInfo(1);
            yield return new WaitForEndOfFrame();
        }        

        int count = 0;       
        while (count < frame)
        {
            count++;
            yield return new WaitForEndOfFrame();
        }

        if (attack)
            m_animator.speed = 0f;
        else
            m_animator.speed = 1f;
        yield return null;
    }

    IEnumerable SetCameraBowView()
    {
        yield return new WaitForEndOfFrame();

    }

    Vector3 CalculateRotateVec( float angle )
    {
        angle = angle % 360;
        return new Vector3(m_originOffset.z * Mathf.Sin(angle) + m_originOffset.x * Mathf.Cos(angle), m_originOffset.y, m_originOffset.z * Mathf.Cos(angle) + m_originOffset.x * Mathf.Sin(angle));
    }

    void LateUpdate()
    {

        if (m_ctrlType == ControlType.Player)
        {
            m_camTrans.RotateAround(transform.position, Vector3.up, m_rotaDir);
            m_offset = m_camTrans.rotation * m_originOffset;

            Vector3 finalPos = transform.position + m_offset;

            if (attack)
            {
                Vector3 offset = transform.rotation * m_originOffset;
                Vector3 push = m_camTrans.rotation * m_pushDir;
                finalPos = transform.position + offset;
                finalPos = finalPos + push;

                m_camTrans.rotation = transform.rotation;

                bowView = false;
            }

            Ray checkRay = new Ray(m_target.position, finalPos - transform.position);
            RaycastHit hitPoint;
            if (Physics.Raycast(checkRay, out hitPoint, Vector3.Distance(finalPos, transform.position)))
            {
                finalPos = hitPoint.point;
            }

            m_camTrans.position = Vector3.Lerp(m_camTrans.position, finalPos, Speed * Time.deltaTime);

            m_rotaDir = 0;
        }
    }

    public void InactiveNav()
    {
        m_chaCtrl.enabled = false;
    }

    public void ActiveNav()
    {
        m_chaCtrl.enabled = true;
    }
    void OnDrawGizmos()
    {        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + GetComponent<NavMeshAgent>().desiredVelocity * 10);
    }

    void OnGUI()
    {       
        GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 30, 30), "+");
    }
}
