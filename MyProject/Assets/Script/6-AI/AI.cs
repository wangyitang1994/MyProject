using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    // [SerializeField]private Transform target;
    // private float mass = 1;
    // private float separate_dis = 3;
    // private float alignment_dis = 5;
    // private float towards_dis = 5;
    // [SerializeField]private float separate_weight = 1;
    // [SerializeField]private float alignment_weight = 1;
    // [SerializeField]private float towards_weight = 1;
    // [SerializeField]private float target_weight = 1;
    // private float speed = 3;
    // [SerializeField]private List<GameObject> separate_list;
    // [SerializeField]private List<GameObject> alignment_list;
    // [SerializeField]private List<GameObject> towards_list;
    // [SerializeField]private Vector3 sum_force = Vector3.zero;
    // private Vector3 velocity = Vector3.zero;

    // private float interval;
    // // Start is called before the first frame update
    // void Start()
    // {
    //     // interval = Random.Range(1,5);
    //     interval = 0.2f;
    //     // LogTool.Log("//?interval",interval);
    //     InvokeRepeating("UpdateForce",0,interval);
    // }

    // // Update is called once per frame
    // void Update()
    // {
    //     // Vector3 a = sum_force / mass;
    //     //velocity += a * Time.deltaTime;
    //     transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(sum_force), Time.deltaTime*3);
    //     transform.Translate(transform.forward * speed * Time.deltaTime, Space.World );
    // }

    // public void UpdateForce(){
    //     Debug.DrawLine(transform.position,transform.position+CalcSeparateForce(),Color.red,interval);
    //     Debug.DrawLine(transform.position,transform.position+CalcTowardsForce(),Color.green,interval);
    //     Debug.DrawLine(transform.position,transform.position+CalcAlignmentForce(),Color.blue,interval);
    //     sum_force = Vector3.zero;
    //     sum_force += CalcSeparateForce();
    //     sum_force += CalcTowardsForce();
    //     sum_force += CalcAlignmentForce();
    //     //保持恒定飞行速度z的力
    //     // Vector3 engineForce = velocity.normalized * startVelocity.magnitude;
    //     // sum_force += engineForce * 0.1f;
    //     Vector3 targetDir = target.position - transform.position;
    //     sum_force += targetDir.normalized;
    //     Debug.DrawLine(transform.position,transform.position+targetDir.normalized,Color.yellow,interval);
    //     Debug.DrawLine(transform.position,transform.position+sum_force,Color.black,interval);
    // }

    // //分力
    // private Vector3 CalcSeparateForce(){
    //     Vector3 separate_force = Vector3.zero;
    //     Collider[] collider = Physics.OverlapSphere(transform.position,separate_dis);
    //     separate_list.Clear();
    //     for(int i = 0;i < collider.Length;i++){
    //         GameObject go = collider[i].gameObject;
    //         if(CanAddInList(go)){
    //             separate_list.Add(go);
    //             Vector3 force = transform.position - go.transform.position;
    //             separate_force += force.normalized/force.magnitude;
    //         }
    //     }
    //     separate_force *= separate_weight;
    //     //  if(separate_list.Count > 0){
    //     //     separate_force = separate_force/separate_list.Count;
    //     // }
    //     return separate_force;
    // }

    // //面朝方向
    // private Vector3 CalcTowardsForce(){
    //     Vector3 towards_force = Vector3.zero;
    //     Collider[] collider = Physics.OverlapSphere(transform.position,towards_dis);
    //     towards_list.Clear();
    //     for(int i = 0;i < collider.Length;i++){
    //         GameObject go = collider[i].gameObject;
    //         if(CanAddInList(go)){
    //             towards_list.Add(go);
    //             // LogTool.Log("//?forward",go.transform.forward);
    //             towards_force += go.transform.forward;
    //         }
    //     }
    //     if(towards_list.Count > 0){
    //         towards_force =  towards_force/towards_list.Count - transform.forward;
    //         towards_force *= speed * towards_weight;
    //     }
    //     return towards_force;
    // }

    // //聚集
    // private Vector3 CalcAlignmentForce(){
    //     Vector3 alignment_force = Vector3.zero;
    //     Collider[] collider = Physics.OverlapSphere(transform.position,alignment_dis);
    //     alignment_list.Clear();
    //     for(int i = 0;i < collider.Length;i++){
    //         GameObject go = collider[i].gameObject;
    //         if(CanAddInList(go)){
    //             alignment_list.Add(go);
    //             alignment_force += go.transform.position;
    //         }
    //     }
    //     if(alignment_list.Count > 0){
    //         alignment_force = alignment_force/alignment_list.Count - transform.position;
    //         alignment_force *= alignment_weight;
    //     }
    //     return alignment_force;
    // }

    // // center /= alignmentNeighbors.Count;
    // // Vector3 dirToCenter = center - transform.position;
    // // cohesionForce += dirToCenter.normalized * velocity.magnitude;
    // // cohesionForce *= cohesionWeight;
    // // sumForce += alignmentForce;

    // private bool CanAddInList(GameObject go){
    //     return go.tag == "Player" && go != this.gameObject;
    // }
}
