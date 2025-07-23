using System;
using Player;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DefaultNamespace
{
    public class UIShapeController : MonoBehaviour
    {
        [SerializeField] private Transform DogImage;
        [SerializeField] private Transform BirdImage;
        [SerializeField] private Transform RatImage;
        [SerializeField] private Vector2 selectedScale;
        [SerializeField] private Vector2 unSelectedScale;

        [Inject]
        private PlayerController playerController;

        private void Awake()
        {
            BirdImage.gameObject.SetActive(false);
            RatImage.gameObject.SetActive(false);

            playerController.OnShapeChanged += OnShapeChanged;
            playerController.OnShapeUnlocked += OnShapeUnlocked;

            foreach (PlayerController.ShapeSettings shape in playerController.Shapes)
            {
                    switch (shape.shapeType)
                    {
                        case PlayerController.Shape.Dog:
                            DogImage.gameObject.SetActive(shape.isUnlocked);
                            break;
                        case PlayerController.Shape.Bird:
                            BirdImage.gameObject.SetActive(shape.isUnlocked);
                            break;
                        case PlayerController.Shape.Rat:
                            RatImage.gameObject.SetActive(shape.isUnlocked);
                            break;
                    }

            }
        }

        private void OnDisable()
        {
            playerController.OnShapeChanged -= OnShapeChanged;
            playerController.OnShapeUnlocked -= OnShapeUnlocked;
        }

        private void OnShapeChanged(PlayerController.Shape shape)
        {
            switch (shape)
            {
                case PlayerController.Shape.Dog:
                    DogImage.localScale = selectedScale;
                    BirdImage.localScale = unSelectedScale;
                    RatImage.localScale = unSelectedScale;
                    break;
                case PlayerController.Shape.Bird:
                    BirdImage.localScale = selectedScale;
                    RatImage.localScale = unSelectedScale;
                    DogImage.localScale = unSelectedScale;
                    break;
                case PlayerController.Shape.Rat:
                    RatImage.localScale = selectedScale;
                    DogImage.localScale = unSelectedScale;
                    BirdImage.localScale = unSelectedScale;
                    break;
            }
        }

        private void OnShapeUnlocked(PlayerController.Shape shape)
        {
            switch (shape)
            {
                case PlayerController.Shape.Dog:
                    DogImage.gameObject.SetActive(true);
                    break;
                case PlayerController.Shape.Bird:
                    BirdImage.gameObject.SetActive(true);
                    break;
                case PlayerController.Shape.Rat:
                    RatImage.gameObject.SetActive(true);
                    break;
            }
        }
    }
}