using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace LiftStudio
{
    public class MovementCard : MonoBehaviour
    {
        [SerializeField] private TMP_Text cardOwnerText;
        [SerializeField] private Transform abilityContainer;
        [SerializeField] private Transform movementDirectionContainer;
        [SerializeField] private GameObject researchImage;
        [SerializeField] private GameObject elevatorImage;
        [SerializeField] private GameObject portalImage;
        [SerializeField] private Transform movementDirectionPrefab;

        public void SetMovementCardSettings(string cardOwner, MovementCardSettings cardSettings)
        {
            var cardDirectionList = cardSettings.GetCardArrowRotation();

            SetCardText(cardOwner);
            
            foreach (var rotation in cardDirectionList)
            {
                var spawnedCardArrow = Instantiate(movementDirectionPrefab, movementDirectionContainer);
                spawnedCardArrow.rotation = Quaternion.AngleAxis(rotation, Vector3.forward);
            }

            researchImage.SetActive(cardSettings.canUseResearch);
            elevatorImage.SetActive(cardSettings.canUseElevator);
            portalImage.SetActive(cardSettings.canUsePortal);

            UpdateContainerActive(cardSettings, cardDirectionList);
        }

        private void SetCardText(string cardOwner)
        {
            if (PhotonNetwork.LocalPlayer.NickName == cardOwner)
            {
                cardOwnerText.text = "You";
                cardOwnerText.color = Color.red;
                return;
            }

            cardOwnerText.text = cardOwner;
        }
        
        private void UpdateContainerActive(MovementCardSettings cardSettings, ICollection cardDirectionList)
        {
            var hasAnyAbility = cardSettings.canUseElevator || cardSettings.canUsePortal || cardSettings.canUseResearch;
            var hasAnyMovement = cardDirectionList.Count > 0;
            
            abilityContainer.gameObject.SetActive(hasAnyAbility);
            movementDirectionContainer.gameObject.SetActive(hasAnyMovement);
        }
    }
}