using System;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Ball
{
    public class Ball : MonoBehaviour
    {
		[SerializeField] private float movePower = 1000; // The force added to the ball to move it.
		[SerializeField] private bool useTorque = true; // Whether or not to use torque to move the ball.
		[SerializeField] private float maxAngularVelocity = 25; // The maximum velocity the ball can rotate at.
		[SerializeField] private float jumpPower = 500; // The force added to the ball when it jumps.
		[SerializeField] private float hoverPower = 25;
		[SerializeField] private float boostPower = 125;
		[SerializeField] private float gravityAssist = 500;
		[SerializeField] private float airResistance = 0.01f;
		[SerializeField] private float boostTiming = 0.1f;

		bool boostReady = true;
		bool boostIsActive = false;
		float originalFriction;

		private const float groundRayLength = 1f; // The length of the ray to check if the ball is grounded.
		private Rigidbody _rigidbody;
		private Collider _collider;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
			_collider = GetComponent<Collider>();

            // Set the maximum angular velocity.
            _rigidbody.maxAngularVelocity = maxAngularVelocity;

			originalFriction = _collider.material.dynamicFriction;
        }

		void OnCollisionEnter(Collision collision)
		{
			if (collision.gameObject.CompareTag("Block") == true)
			{
				float impact = (_rigidbody.velocity.x + _rigidbody.velocity.y + _rigidbody.velocity.z) * _rigidbody.mass;
				impact *= Mathf.Sign(impact);
				impact /= 1000;

				if (boostIsActive && impact > 0.1f)
				{
					//BashBlocks(collision);
				}
			}
		}

        public void Move(Vector3 moveDirection, bool jump, bool boosting, bool boostActivated)
        {

			bool grounded = Physics.Raycast(transform.position, -Vector3.up, groundRayLength);

			if (boostReady && boostActivated)
			{
				boostIsActive = true;
				boostReady = false;
				_collider.material.dynamicFriction = 0;
				//trail.enabled = true;
				Invoke("BoostOff", boostTiming);
			}

			// Boost
			if (boosting && boostIsActive)
			{
				if (moveDirection == Vector3.zero && !grounded)
				{
					moveDirection = Vector3.down * boostPower;
				}
				else if (!grounded)
				{
					moveDirection *= boostPower * 0.5f;
				}
				else
				{
					moveDirection *= boostPower;
				}
			}

            // If using torque to rotate the ball...
            if (useTorque)
            {
                // ... add torque around the axis defined by the move direction.
                _rigidbody.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x) * movePower * 0.5f);
				_rigidbody.AddForce(moveDirection * movePower);
            }
            else
            {
                // Otherwise add force in the move direction.
                _rigidbody.AddForce(moveDirection * movePower);
            }

            // If on the ground and jump is pressed...
            if (grounded)
            {
				if (jump)
				{
					// ...add force in upwards.
					_rigidbody.AddForce(moveDirection * movePower * airResistance + Vector3.up * jumpPower, ForceMode.Impulse);
				}
            }
			// In the air and jump is held
			else
			{
				if (jump)
				{
					_rigidbody.AddForce(moveDirection * movePower * airResistance + Vector3.up * hoverPower, ForceMode.Impulse);
				}
				else
				{
					_rigidbody.AddForce(moveDirection * movePower * airResistance + Vector3.down * gravityAssist, ForceMode.Impulse);
				}
			}
        }

		void BoostOff()
		{
			boostIsActive = false;
			_collider.material.dynamicFriction = originalFriction;
			//trail.enabled = false;
			Invoke("AllowBoost", boostTiming);
		}

		void AllowBoost()
		{
			boostReady = true;
		}
			
    }
}
