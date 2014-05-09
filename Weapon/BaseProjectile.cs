﻿using UnityEngine;
using System;

using System.Collections;
[Serializable]
public class BaseDamage{
	public float Damage;
	public bool isVsArmor;
	public float pushForce;
	public Vector3 pushDirection;
	public Vector3 hitPosition;
	
	public BaseDamage(BaseDamage old){
		Damage = old.Damage;
		isVsArmor = old.isVsArmor;
		pushForce = old.pushForce;
	}

}
//We don't want to our projectile fly infinite time
[RequireComponent (typeof (DelayDestroyedObject))]
public class BaseProjectile : MonoBehaviour {



	public BaseDamage damage;
	public float startImpulse;
	public GameObject owner;
	public GameObject hitParticle;
	public int splashRadius;

	//звуки
	public AudioSource aSource;//источник звука задается в редакторе
	public AudioClip reactiveEngineSound;
	public AudioClip exploseSound;
	private soundControl sControl;//контроллер звука
	
	private Transform mTransform;
	private Rigidbody mRigidBody;
	private bool used=false;
	
	void Start () {
		
		sControl = new soundControl (aSource);//создаем обьект контроллера звука и передаем указатель на источник
		sControl.playClip (reactiveEngineSound);

		mTransform = transform;
		mRigidBody = rigidbody;
		mRigidBody.velocity = mTransform.TransformDirection(Vector3.forward * startImpulse);
		
		RaycastHit hit;
		
		if (Physics.Raycast (transform.position, mRigidBody.velocity.normalized, out hit)){
			
			if (hit.distance < mTransform.InverseTransformDirection (mRigidBody.velocity).z * 0.1f)
			{
				onBulletHit(hit);
			}
		}
	}
	
	void Update() {
		
		RaycastHit hit;
		mTransform.rotation = Quaternion.LookRotation ( mRigidBody.velocity);
		if (Physics.Raycast (transform.position, mRigidBody.velocity.normalized, out hit)){
			
			if (hit.distance < mTransform.InverseTransformDirection (mRigidBody.velocity).z * 0.1f)
			{
				onBulletHit(hit);
			}
		}
	}
	
	public virtual  void onBulletHit(RaycastHit hit)
	{
		if (owner == hit.transform.gameObject||used) {
			return;
		}
		used = true;
		damage.pushDirection = mTransform.forward;
		damage.hitPosition = hit.point;
		DamagebleObject obj = hit.transform.gameObject.GetComponent <DamagebleObject>();
		if (obj != null) {
			obj.Damage(damage,owner);
			//Debug.Log ("HADISH INTO SOME PLAYER! " + hit.transform.gameObject.name);
			Destroy (gameObject, 0.1f);
		}

		if(hitParticle!=null){
			Instantiate(hitParticle, hit.point, Quaternion.LookRotation(hit.normal));
		}
	
		Destroy (gameObject, 0.1f);

	}
	
	void OnDestroy() {
		if(splashRadius>0){
			ExplosionDamage();		
		}
	}
	
	void ExplosionDamage() {

		sControl.stopSound ();//останавливаем звук реактивного двигателя
		sControl.playClip (exploseSound);//звук взрыва

		Collider[] hitColliders = Physics.OverlapSphere(transform.position, splashRadius);
		
		for(int i=0;i < hitColliders.Length;i++) {
			//Debug.Log(hitColliders[i]);
			DamagebleObject obj = hitColliders[i].GetComponent <DamagebleObject>();
			if (obj != null) {
				obj.Damage(damage,owner);
			}
		}
	}
}
