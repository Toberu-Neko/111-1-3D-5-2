using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SkillState : MonoBehaviour
{
    public Skill skill;
    private GameObject hitParticle;
    //public int damage;
    //public float dieTime;
    //public float throwCD;
    //public float throwRange;
    //public float cooldown;

    private AudioSource aud;
    private Rigidbody rig;
    private SphereCollider col;
    private GameObject design;
    private bool hitTarget;
    void Start()
    {
        
        design = transform.Find("Design").gameObject;

        aud = GetComponent<AudioSource>();
        col = GetComponent<SphereCollider>();
        rig = GetComponent<Rigidbody>();
        hitParticle = skill.hitParticle;
        Destroy(gameObject, skill.dieTime);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (hitTarget)
        {
            return;
        }
        else if (skill.isTemporarySkill)
        {
            if (skill.waterTemporary)
            {
                if (collision.gameObject.CompareTag("WaterCeiling"))
                {
                    WallStatus wallStatus = collision.gameObject.GetComponent<WallStatus>();
                    if(wallStatus.currentHealth > 0)
                    aud.Play();

                    wallStatus.TakeDamage(skill.damage); 
                }
                else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemies"))
                {
                    EnemyStates enemy = collision.gameObject.GetComponent<EnemyStates>();
                    GameObject particle = Instantiate(hitParticle, transform.position, Quaternion.identity);
                    Destroy(particle, 1f);
                    aud.Play();
                    enemy.EnemyTakeDamage(3);
                }
                else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Bosses"))
                {
                    TestBossStates boss = collision.gameObject.GetComponent<TestBossStates>();
                    GameObject particle = Instantiate(hitParticle, transform.position, Quaternion.identity);
                    Destroy(particle, 1f);
                    if (!boss.invincible)
                        aud.Play();

                    boss.BossTakeDamage(1);
                }
                else if (collision.gameObject.layer == LayerMask.NameToLayer("Skill") || 
                    collision.collider.gameObject.layer == LayerMask.NameToLayer("Player") || 
                    collision.collider.gameObject.layer == LayerMask.NameToLayer("EnemyAttack"))
                {
                    return;
                }

                hitTarget = true;
                rig.isKinematic = true;
                design.SetActive(false);


                col.enabled = false;
                transform.SetParent(collision.transform);
            }
        }
        else
        {
            if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemies"))
            {
                EnemyStates enemy = collision.gameObject.GetComponent<EnemyStates>();
                GameObject particle = Instantiate(hitParticle, transform.position, Quaternion.identity);
                Destroy(particle, 1f);
                aud.Play();
                enemy.EnemyTakeDamage(skill.damage);
            }
            if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Bosses"))
            {
                TestBossStates boss = collision.gameObject.GetComponent<TestBossStates>();
                GameObject particle = Instantiate(hitParticle, transform.position, Quaternion.identity);
                Destroy(particle, 1f);
                if(!boss.invincible)
                aud.Play();

                boss.BossTakeDamage(skill.damage);
            }

            if (collision.gameObject.layer == LayerMask.NameToLayer("Skill")|| 
                collision.collider.gameObject.layer == LayerMask.NameToLayer("Player") || 
                collision.collider.gameObject.layer == LayerMask.NameToLayer("EnemyAttack"))
                return;




            hitTarget = true;
            rig.isKinematic = true;
            design.SetActive(false);


            col.enabled = false;
            transform.SetParent(collision.transform);
        }

    }
}
