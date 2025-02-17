﻿using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    public float Speed;
    //[SerializeField] private MicInput _input;
    // [Header("地面检测")]
    //[SerializeField] private Vector3 offset;
    // [SerializeField] private float Radius=1f;
    // [Header("边缘碰撞检测")]
    //[SerializeField] private Vector3 m_Center;
    //[SerializeField] private Vector3 m_Size;
    // [SerializeField] private UnityEvent OnRight;
    // [SerializeField] private UnityEvent OnLeft;
    private Vector3 m_Velocity;
    [HideInInspector] public Transform m_Trans;
    [HideInInspector] public Rigidbody m_rig;
    public bool m_Grounded { get; set; }
    private Vector3 Orientation;
    private float Hor;
    private float Ver;

    public float JumpForce;
    public float MaxTimeCounter;
    public bool isSidecollider { get; set; }

    public float JumpTimeCounter { get; set; }
    public int m_Height { get; set; }
    private bool isJump;
    [SerializeField] private UnityEvent OnFallDown;
    public bool ReversalInput { get; set; }
    private Transform maincamera;
    private bool isFallDowned;
    private Vector3 dir;
    private Vector3 moveVelocity;
    private float x;
    private float y;
    [Range(0f, 3f)] public float JumpDis;
    [SerializeField] [Range(0f, 1f)] private float SmoothTime;

    [SerializeField]
    float angularDrag;
    [SerializeField]
    float Drag;
    //[SerializeField] private Mesh OriginMesh;
    Data GameDataINS;
    private void Awake()
    {
        m_rig = GetComponent<Rigidbody>();
        m_rig.angularDrag = angularDrag;
        m_rig.drag = Drag;
        // Dir = Vector3.right+Vector3.up;
        m_Trans = transform;
        //  Physics.gravity = Vector3.down *9.81f* (float)Math.PI;
        //  RotateSpeed = Speed;
        // m_MeshFilter = gameObject.GetComponent<MeshFilter>();
        maincamera = Camera.main.transform;
        GC.Collect();

    }

    private void FixPlayer()
    {
        // m_MeshFilter.mesh=OriginMesh;
        //  GetComponent<MeshCollider>().sharedMesh = OriginMesh;
        AutoSaver save = GetComponent<AutoSaver>();
        save.ClearData();
        GameManager.INS.RestartGame();
    }

    private void Start()
    {
        GC.Collect();

        x = GameManager.INS.GameData.X_MultiplyInpputValue;
        y = GameManager.INS.GameData.Y_MultiplyInpputValue;
        GameManager.INS.GameEvents.OnGameReset += FixPlayer;
        GameDataINS = GameManager.INS.GameData;
    }

    private void OnDisable()
    {
        m_rig.Sleep();
    }

    private void OnEnable()
    {
        m_rig.WakeUp();
    }

    private void FixedUpdate()
    {
        Hor = Input.acceleration.x * x;
        Ver = Input.acceleration.y * y;
        if (Hor > 1)
            Hor = 1;
        if (Ver > 1)
            Ver = 1;
        if (Hor < -1)
            Hor = -1;
        if (Ver < -1)
            Ver = -1;

        if (!Application.isMobilePlatform)//如果不为移动设备,则使用鼠标控制
        {
            var Middle = new Vector3(Screen.width / 2, Screen.height / 2);
            Hor = (Input.mousePosition - Middle).normalized.x;
            Ver = (Input.mousePosition - Middle).normalized.y;
        }
        // dir = (m_Trans.position - maincamera.position).normalized;
        if (ReversalInput)
        {
            Hor = -Hor;
            //Ver = -Ver;
        }
        moveVelocity.x = Hor * Speed;
        moveVelocity.z = Ver * Speed;

        if (m_Grounded)
        {
            m_rig.velocity = Vector3.SmoothDamp(m_rig.velocity, moveVelocity, ref m_Velocity, SmoothTime);
        }
        else
        {
            m_rig.velocity = new Vector3(moveVelocity.x * JumpDis, m_rig.velocity.y, moveVelocity.z * JumpDis);
        }

        #region 跳跃模块
        if (m_Grounded && isTouch)
        {
            JumpTimeCounter = MaxTimeCounter;
            m_rig.velocity = Vector3.up * JumpForce;
            //m_rig.AddForce(Vector3.up*JumpForce,ForceMode.VelocityChange);
            isJump = true;
        }
        if (isTouch && !m_Grounded)
        {
            if (JumpTimeCounter > 0)
            {
                if (m_rig.velocity.y < 9.81f)
                    m_rig.velocity += Vector3.up * (JumpForce - Time.deltaTime);

                //m_rig.AddForce(Vector3.up*(JumpForce-Time.deltaTime),ForceMode.Acceleration);

                JumpTimeCounter -= Time.deltaTime;
            }
        }
        if (!isTouch)
        {
            JumpTimeCounter = 0;
            isJump = false;
        }
        #endregion 跳跃模块
    }

    public float FallDownForce = 2f;
    bool isTouch;

    // public float RotateSpeed { get; set; }


    // Update is called once per frame

    void Update()
    {
        isTouch = InputManager.INS.m_RightTouch;
        if (!m_Grounded)
        {
            m_rig.velocity += Vector3.up * Physics.gravity.y * (FallDownForce - 1) * Time.deltaTime;
        }
        if (m_Grounded)
        {
            if (!isFallDowned)
            {
                OnFallDown?.Invoke();
                isFallDowned = true;
            }
        }

        if (!m_Grounded)
            isFallDowned = false;

        if (m_Grounded)
        {
            if ((int)m_Trans.position.y >= m_Height)
            {
                m_Height = (int)m_Trans.position.y;
                GameDataINS.Height = m_Height;
                if (m_Height > GameDataINS.BestHeight)
                {
                    GameDataINS.UpdateBestHeight((int)m_Trans.position.y);
                }
            }
        }

    }



    //private MeshFilter m_MeshFilter;

    private void LateUpdate()
    {
        if (isSidecollider)
        {
            m_rig.velocity+=Vector3.up*Physics.gravity.y*Time.deltaTime;
        }

    }


    //  private void CheckGround()

    // {

    //   var COLLECTION=  Physics.OverlapSphere(m_Trans.position + offset, Radius);

    //   m_Grounded = false;

    //   foreach (var collider in COLLECTION)

    //  {

    //      if (collider.transform != m_Trans&&!collider.isTrigger)

    //      {

    //          m_Grounded=true;

    //      }

    //  }

    // }


    private void OnGUI()
    {
        GUI.Label(new Rect(10, 50, 200, 200), String.Format("水平{0}X,纵向{1}X", x, y));
    }

    private void OnCollisionEnter(Collision other)
    {

    }

    private Ray ray;



    private void OnCollisionStay(Collision other)
    {
        ray = new Ray(m_Trans.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.distance < 2f)
            {
                m_Grounded = true;
            }
            else
            {
                m_Grounded = false;
            }
        }
        if (!m_Grounded)
            isSidecollider = true;
    }

    private void OnCollisionExit(Collision other)
    {
        m_Grounded = false;
        isSidecollider = false;
    }
}
