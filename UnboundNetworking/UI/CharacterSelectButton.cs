using TMPro;
using Unbound.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unbound.Networking.UI
{
    class CharacterSelectButton : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private LeftRight direction = CharacterSelectButton.LeftRight.Left;
        private CharacterSelectionInstance characterSelectionInstance = null;
        private const float hoverScale = 1.00f;
        private const float clickScale = 0.95f;
        private Vector3 defaultScale;
        private bool inBounds = false;
        private bool pressed = false;
        private TextMeshProUGUI text = null;
        private int currentlySelectedFace = -1;
        private bool isReady = false;
        private static Color disabledColor = new Color(0.75f, 0.75f, 0.75f, 0.25f);
        private static Color enabledColor = Color.white;

        public void SetDirection(LeftRight direction)
        {
            this.direction = direction;
        }
        public void SetCharacterSelectionInstance(CharacterSelectionInstance characterSelectionInstance)
        {
            this.characterSelectionInstance = characterSelectionInstance;
        }

        void Start()
        {
            text = gameObject.GetOrAddComponent<TextMeshProUGUI>();

            text.text = direction == CharacterSelectButton.LeftRight.Left ? "<" : ">";

            text.color = CharacterSelectButton.enabledColor;

            text.alignment = TextAlignmentOptions.Center;

            defaultScale = gameObject.transform.localScale;

            transform.parent.localPosition = Vector3.zero;
        }
        void Update()
        {
            if (characterSelectionInstance == null) { return; }

            if (characterSelectionInstance.GetFieldValue<bool>("isReady") != isReady)
            {
                isReady = characterSelectionInstance.GetFieldValue<bool>("isReady");
                if (isReady)
                {
                    text.color = CharacterSelectButton.disabledColor;
                }
                else
                {
                    text.color = CharacterSelectButton.enabledColor;
                }
            }

            if (currentlySelectedFace == characterSelectionInstance.currentlySelectedFace) { return; }

            text.color = CharacterSelectButton.enabledColor;


            if (currentlySelectedFace < characterSelectionInstance.currentlySelectedFace && direction == CharacterSelectButton.LeftRight.Right)
            {
                gameObject.transform.localScale = defaultScale * CharacterSelectButton.clickScale;
                this.ExecuteAfterSeconds(0.1f, () => gameObject.transform.localScale = inBounds ? defaultScale * CharacterSelectButton.hoverScale : defaultScale);
            }
            else if (currentlySelectedFace > characterSelectionInstance.currentlySelectedFace && direction == CharacterSelectButton.LeftRight.Left)
            {
                gameObject.transform.localScale = defaultScale * CharacterSelectButton.clickScale;
                this.ExecuteAfterSeconds(0.1f, () => gameObject.transform.localScale = inBounds ? defaultScale * CharacterSelectButton.hoverScale : defaultScale);
            }


            currentlySelectedFace = characterSelectionInstance.currentlySelectedFace;

        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (characterSelectionInstance.currentPlayer.data.input.inputType == GeneralInput.InputType.Controller) { return; }

            if (inBounds)
            {
                pressed = true;
                gameObject.transform.localScale = defaultScale * CharacterSelectButton.clickScale;
            }
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (characterSelectionInstance.currentPlayer.data.input.inputType == GeneralInput.InputType.Controller) { return; }

            if (inBounds && pressed)
            {
                if (characterSelectionInstance != null)
                {
                    if (direction == CharacterSelectButton.LeftRight.Left)
                    {
                        characterSelectionInstance.currentlySelectedFace--;
                    }
                    else if (direction == CharacterSelectButton.LeftRight.Right)
                    {
                        characterSelectionInstance.currentlySelectedFace++;
                    }

                    characterSelectionInstance.currentlySelectedFace = Mathf.Clamp(characterSelectionInstance.currentlySelectedFace, 0, ((HoverEvent[]) characterSelectionInstance.GetFieldValue("buttons")).Length - 1);
                }

            }
            pressed = false;
            if (!inBounds)
            {
                gameObject.transform.localScale = defaultScale;
            }
            else
            {
                gameObject.transform.localScale = defaultScale * CharacterSelectButton.hoverScale;
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (characterSelectionInstance.currentPlayer.data.input.inputType == GeneralInput.InputType.Controller) { return; }

            inBounds = true;
            gameObject.transform.localScale = defaultScale * CharacterSelectButton.hoverScale;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (characterSelectionInstance.currentPlayer.data.input.inputType == GeneralInput.InputType.Controller) { return; }

            inBounds = false;
            if (!pressed)
            {
                gameObject.transform.localScale = defaultScale;
            }
        }

        public enum LeftRight
        {
            Left,
            Right
        }

    }
}
