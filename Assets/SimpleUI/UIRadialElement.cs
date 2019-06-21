﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
namespace SimpleUI {


     [ExecuteInEditMode]
    public class UIRadialElement : SelectableElement {


        Image[] _images;
        Image[] images {
            get {
                if (_images == null) {
                    _images = GetComponentsInChildren<Image>();
                }
                return _images;
            }
        }

        Image _mainImage;
        Image mainImage {
            get {
                if (_mainImage == null) {
                    _mainImage = mainImageTransform.GetComponent<Image>();
                }
                return _mainImage;
            }
        }   
        

        Transform _mainImgT;
        public Transform mainImageTransform {
            get {
                if (_mainImgT == null) {
                    _mainImgT = transform.GetChild(1);
                }
                return _mainImgT;
            }
        }
        Transform _flairT;
        public Transform selectFlairTransform {
            get {
                if (_flairT == null) {
                    _flairT = transform.GetChild(0);
                }
                return _flairT;
            }
        }

        protected override void UpdateElement () {
            mainImage.color = selected ? UIManager.instance.mainLightColor : new Color32(0,0,0,0);
            
            int l = images.Length; // get the images in case e havent yet
            selectFlairTransform.gameObject.SetActive(selected);
        }

        protected override void OnEnable ( ) {
            base.OnEnable();
            //only need to set these once
            selectFlairTransform.GetComponent<Image>().color = UIManager.instance.mainDarkColor;
            selectFlairTransform.GetChild(0).GetComponent<Image>().color = UIManager.instance.mainLightColor;


        }
        
        // void OnEnable () {
            
        //     OnDeselect();
        // }
        protected override void OnSubmit () {
            
        }
        protected override void OnSelect () {
            // mainImage.color = UIManager.instance.mainLightColor;
            // selectFlairTransform.gameObject.SetActive(true);
        }
        protected override void OnDeselect () {
            // mainImage.color = Color.clear;//UIManager.instance.mainDarkColor;
            // selectFlairTransform.gameObject.SetActive(false);
        }

        public void UpdateLayout(float radialAmount, float radialAngle, float elementAngle) {

            for (int i =0 ; i< images.Length; i++) {
                images[i].fillAmount = radialAmount;
            }

            Quaternion localRotation = Quaternion.Euler(0,0, radialAngle*.5f);
            mainImageTransform.localRotation = localRotation;
            selectFlairTransform.localRotation = localRotation;

            text.transform.localRotation = Quaternion.Euler (0,0, -elementAngle);

            if (elementAngle < -180f) {
                text.SetAnchor(TextAnchor.MiddleRight);
            }
            else {
                text.SetAnchor(TextAnchor.MiddleLeft);
            }
        }
    }
}