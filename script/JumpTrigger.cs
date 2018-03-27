using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class JumpTrigger : MonoBehaviour {
    public Transform endPos;

    void OnTriggerEnter(Collider other)
    {
        //other.GetComponent<NavMeshAgent>().SetDestination(endPos.position);
        //StartCoroutine(ResetPath(other.GetComponent<NavMeshAgent>()));
        StartCoroutine(JumpPath(other.GetComponent<NavMeshAgent>()));
    }

    IEnumerator JumpPath(NavMeshAgent navM)
    {
        navM.enabled = false;
        yield return new WaitForEndOfFrame();
        navM.transform.DOMove(navM.transform.position + navM.transform.forward * 1.5f, 0.2f);
        yield return new WaitForSeconds(0.2f);
        Vector3 end = new Vector3(navM.transform.position.x, endPos.position.y, navM.transform.position.z);
        navM.transform.DOMove(end, 0.5f);
        yield return new WaitForSeconds(0.5f);
        navM.enabled = true;
    }

    IEnumerator ResetPath(NavMeshAgent navM)
    {
        float dis = Vector3.Distance(navM.transform.position, endPos.position);
        Debug.Log(dis);
        while (dis>0.5f)
        {
            Debug.Log(dis);
            yield return new WaitForEndOfFrame();
            dis = Vector3.Distance(navM.transform.position, endPos.position);
        }

        navM.ResetPath();
    }
}
