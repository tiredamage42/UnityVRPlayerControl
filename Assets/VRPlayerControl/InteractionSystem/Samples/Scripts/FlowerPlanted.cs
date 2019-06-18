//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System.Collections;
using UnityEngine;

namespace Demo
{
    public class FlowerPlanted : MonoBehaviour
    {
        private void Start()
        {
            Plant();
        }

        public void Plant()
        {
            StartCoroutine(DoPlant());
        }

        private IEnumerator DoPlant()
        {
            Vector3 plantPosition = this.transform.position;

            RaycastHit hitInfo;
            bool hit = Physics.Raycast(this.transform.position, Vector3.down, out hitInfo);
            if (hit)
            {
                plantPosition = hitInfo.point + (Vector3.up * 0.05f);
            }
            
            GameObject planting = this.gameObject;
            planting.transform.position = plantPosition;
            planting.transform.rotation = Quaternion.Euler(0, Random.value * 360f, 0);

            planting.GetComponentInChildren<MeshRenderer>().material.SetColor("_TintColor", Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));

            Rigidbody rigidbody = planting.GetComponent<Rigidbody>();
            if (rigidbody != null)
                rigidbody.isKinematic = true;



            Vector3 initialScale = Vector3.one * 0.01f;
            Vector3 targetScale = Vector3.one * (1 + (Random.value * 0.25f));

            float startTime = Time.time;
            float overTime = 0.5f;
            float endTime = startTime + overTime;

            while (Time.time < endTime)
            {
                planting.transform.localScale = Vector3.Slerp(initialScale, targetScale, (Time.time - startTime) / overTime);
                yield return null;
            }


            if (rigidbody != null)
                rigidbody.isKinematic = false;
        }
    }
}