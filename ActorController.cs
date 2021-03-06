﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class ActorController : MonoBehaviour {

    private Animator ani;
    private AnimatorStateInfo currentBaseState;
    private Rigidbody rig;

    private Vector3 velocity;
    // 旋转速度，奔跑速度
    private float rotateSpeed = 15f;
    private float runSpeed = 5f;
    
	void Start () {
        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
	}

    void FixedUpdate() {
        //如果小姐姐死亡，不执行所有动作
        if (!ani.GetBool("isAlive")) return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //设置速度，用来判断做哪个动作
        ani.SetFloat("Speed", Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)));
        //跑步时候的动画速度
        ani.speed = 1 + ani.GetFloat("Speed") / 3;

        velocity = new Vector3(x, 0, z);

        if (x != 0 || z != 0) {
            Quaternion rotation = Quaternion.LookRotation(velocity);
            //ani正面的方向就是z轴正方向
            //如果需要转向，球形插值转向
            if (transform.rotation != rotation) transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * rotateSpeed);
        }
        //小姐姐位置移动   
        transform.position += velocity * Time.fixedDeltaTime * runSpeed;
    }

    // 用于检测Actor进入某个区域
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Area")) {
            //进入区域后，发布消息
            Publish publish = Publisher.getInstance();
            int patrolType = other.gameObject.name[other.gameObject.name.Length - 1] - '0';
            publish.notify(ActorState.ENTER_AREA, patrolType, this.gameObject);
        }
    }

    /// 用于检测Actor与Patrol碰撞后死亡
    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Patrol") && ani.GetBool("isAlive")) {
            Debug.Log("death");
            //调整相应条件以满足状态转移，执行死亡动作
            ani.SetBool("isAlive", false);
            ani.SetTrigger("toDie");
            //碰撞后，发布死亡信息
            Publisher publisher = Publisher.getInstance();
            publisher.notify(ActorState.DEATH, 0, null);
        }
    }
}
