using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Neko.ThreeDGameProjecct
{
    public class ForceMotion : MonoBehaviour
    {
        #region Variables
        private float moveSpeed;//原為orgSpeed
        [Header("玩家速度")]
        public float walkSpeed;
        public float sprintSpeed;
        public float groundDrag;
        public float turnSmoothTime;
        [Range(0f, 0.999f)]
        public float airMultiplier;
        [Header("跳躍")]
        public float jumpForce;
        public float jumpCooldown;
        bool readyToJump;
        [Header("蹲下")]
        public float crouchSpeed;
        public float crouchSuckToGorundMutiplier;
        public float crouchYScale;
        private float startYScale;

        
        [Header("Keybind")]
        public KeyCode jumpKey = KeyCode.Space;
        public KeyCode sprintKey = KeyCode.LeftShift;
        public KeyCode crouchKey = KeyCode.C;
        //public KeyCode keepCrouchKey = KeyCode.LeftControl;

        [Header("附加物件")]
        public LayerMask ground;
        public Transform groundDetector;
        public Transform playerCamTrans;
        public Transform orientation;
        //public Camera normalCam;

        public MovementState state;
        private float turnSmoothVelocity;
        private float hMove, vMove;
        private float targetAngle;
        private float angle;
        Vector3 movementDirection;
        // private float defultFOV;
        //private float adjustedSpeed;
        private Rigidbody rig;

        bool isGrounded;
        //bool jump, jumped = false;

        #endregion
        #region Monobehaviour Callbacks
        void Start()
        {
            crouchKey = KeyCode.C;
            //defultFOV = normalCam.fieldOfView;
            //Camera.main.enabled = false;
            rig = GetComponent<Rigidbody>();
            rig.freezeRotation = true;
            startYScale = transform.localScale.y;
        }
        private void Update()
        {
            StateHandler();
            PlayerInput();
            SpeedControl();

            //Controls
            //bool sprint = Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift);
            bool jump = Input.GetKeyDown(KeyCode.Space);

            //States
            isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);//Raycast(偵測目標位置, 偵測方向, 偵測離主角距離，小於為真, layerMask)
            bool isJumping = jump && isGrounded;
            //bool isSprinting = sprint && vMove > 0 && !isJumping && isGrounded;

            //Jumping
            if (isJumping)
            {
                rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                //jumped = true;
            }

            //Drag
            if (isGrounded)
                rig.drag = groundDrag;
            else
                rig.drag = 0;
        }

        void FixedUpdate()
        {
            //Controls
            //bool sprint = Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift);
            //bool jump = Input.GetKeyDown(KeyCode.Space);

            //States
            //isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);//Raycast(偵測目標位置, 偵測方向, 偵測離主角距離，小於為真, layerMask)
            //bool isJumping = jump && isGrounded;
            //bool isSprinting = sprint && vMove > 0 && !isJumping && isGrounded;

            //PlayerTurn();
            Movement();


            //FOV
            /*if (isSprinting)
            {
                //Lerp(a,b,c)=a經過c秒變成b
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, defultFOV * sprintFOVModifier, Time.deltaTime * 8f);
            }
            else
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, defultFOV, Time.deltaTime * 8f);
            }*/

        }
        #endregion
        #region States
        public enum MovementState
        {
            walking,
            sprinting,
            crouching,
            air
        }
        private void StateHandler()
        {
            if (Input.GetKey(crouchKey))
            {
                state = MovementState.crouching;
                moveSpeed = crouchSpeed;
            }
            else if (isGrounded && Input.GetKey(sprintKey))
            {
                state = MovementState.sprinting;
                moveSpeed = sprintSpeed;
            }
            else if(isGrounded)
            {
                state = MovementState.walking;
                moveSpeed = walkSpeed;
            }
            else
            {
                state = MovementState.air;
            }
        }
        #endregion

        private void PlayerInput()
        {
            hMove = Input.GetAxisRaw("Horizontal");//水平A+1, D-1
            vMove = Input.GetAxisRaw("Vertical");//垂直W+1, S=1

            if(Input.GetKeyDown(jumpKey) && isGrounded && readyToJump)
            {
                //Jump
                readyToJump = false;
                Jump();
                #region Invoke用法（註解）
                /*
                public void Invoke(string methodName, float time);
                    -Invoke ( 委派的funtion,幾秒後開始調用 )

                public void InvokeRepeating(string methodName, float time, float repeatRate);
                    -InvokeRepeating ( 委派的funtion, 幾秒後開始調用, 開始調用後每幾秒再調用 ) 

                public bool IsInvoking(string methodName);
                    -IsInvoking ( 委派的funtion ) 判斷是否正在調用中
                */
                #endregion
                Invoke(nameof(resetJump), jumpCooldown);//Invoke(nameof(A), b) A=Function, b=幾秒後執行;
            }
            //Crouch
            if (Input.GetKeyDown(crouchKey))
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                rig.AddForce(Vector3.down * crouchSuckToGorundMutiplier, ForceMode.Impulse);
            }
            if (Input.GetKeyUp(crouchKey))
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
        }
        private void PlayerTurn()
        {
            #region 轉角色
            //Atan2 = https://home.gamer.com.tw/artwork.php?sn=5068016
            targetAngle = Mathf.Atan2(hMove, vMove) * Mathf.Rad2Deg + playerCamTrans.eulerAngles.y;//算目標角度+相機角度（0~360）

            angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            #endregion
        }
        private void Movement()
        {
            //Movement
            movementDirection = orientation.forward * vMove + orientation.right * hMove;
            /*Vector3 t_direction = new Vector3(hMove, 0, vMove);
            t_direction.Normalize();
            Vector3 t_targetVelocity = transform.TransformDirection(t_direction) * moveSpeed * Time.deltaTime;*/

            if (isGrounded)
                rig.AddForce(movementDirection * moveSpeed * 10f, ForceMode.Force);
            if (!isGrounded)
                rig.AddForce(movementDirection * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        }
        private void Jump()
        {
            //reset t velocity
            rig.velocity = new Vector3(rig.velocity.x, 0f, rig.velocity.z);

            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);//Vector3.up 不考慮rotation, transform.up考慮
        }
        private void resetJump()
        {
            readyToJump = true;
        }
        
        private void SpeedControl()
        {
            Vector3 flatVel = new Vector3(rig.velocity.x, 0f, rig.velocity.z);

            //limit velocity if needed
            if(flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rig.velocity = new Vector3 (limitedVel.x, rig.velocity.y, limitedVel.z);
            }
        }
    }
}

