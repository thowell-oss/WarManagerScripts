using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{

    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class CollisionDetector : MonoBehaviour
    {
        Collider col;
        Rigidbody rbody;

        [SerializeField] bool _collision;
        public bool Collision => _collision;

        void Start()
        {
            col = GetComponent<Collider>();
            rbody = GetComponent<Rigidbody>();

            rbody.isKinematic = true;
            col.isTrigger = true;
        }

        void OnTriggerEnter()
        {
            _collision = true;
        }

        void OnTriggerExit()
        {
            _collision = false;
        }
    }
}
