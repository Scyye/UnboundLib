using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using InControl;
using System.Linq;
using Photon.Pun;

using System.Collections;
using UnityEngine.UI.ProceduralImage;
using System.Reflection;
using Unbound.Core;
using Unbound.Core.Utils;
using RWF;

namespace Unbound.Networking.UI
{
    static class Colors
   {
        public static Color Transparent(Color color, float a = 0.5f)
       {
            return new Color(color.r, color.g, color.b, a);
        }
        public static Color readycolor = new Color(0.2f, 0.8f, 0.1f, 1f);
        public static Color createdColor = new Color(0.9f, 0f, 0.1f, 1f);
        public static Color joinedcolor = new Color(0.566f, 0.566f, 0.566f, 1f);
        public static Color myBorderColor = Color.white;
        public static Color othersBorderColor = Color.clear;
    }
    [RequireComponent(typeof(PhotonView))]
    public class PrivateRoomCharacterSelectionInstance : MonoBehaviour, IPunInstantiateMagicCallback
   {
        private PhotonView view => gameObject.GetComponent<PhotonView>();

        public void OnPhotonInstantiate(Photon.Pun.PhotonMessageInfo info)
       {
            UnboundNetworking.instance.StartCoroutine(Instantiate(info));
        }
        private IEnumerator Instantiate(Photon.Pun.PhotonMessageInfo info)
       {
            // info[0] will be the actorID of the player and info[1] will be the localID of the player
            // info[2] will be the name of the player picking, purely to assign this gameobject's new name
            object[] instantiationData = info.photonView.InstantiationData;

            int actorID = (int) instantiationData[0];
            int localID = (int) instantiationData[1];
            string name = (string) instantiationData[2];

            yield return new WaitUntil(() =>
           {
                return PhotonNetwork.CurrentRoom != null
                    && PhotonNetwork.CurrentRoom.GetPlayer(actorID).GetProperty<LobbyCharacter[]>("players") != null
                    && PhotonNetwork.CurrentRoom.GetPlayer(actorID).GetProperty<LobbyCharacter[]>("players").Count() > localID
                    && PhotonNetwork.CurrentRoom.GetPlayer(actorID).GetProperty<LobbyCharacter[]>("players")[localID] != null
                    && (PhotonNetwork.LocalPlayer.ActorNumber != actorID || PrivateRoomHandler.instance.devicesToUse.Count() > localID);
            });

            LobbyCharacter lobbyCharacter = PhotonNetwork.CurrentRoom.GetPlayer(actorID).GetProperty<LobbyCharacter[]>("players")[localID];

            if (lobbyCharacter == null)
           {
                yield break;
            }

            gameObject.name += " " + name;

            if (lobbyCharacter.IsMine && !PrivateRoomHandler.instance.devicesToUse.ContainsKey(localID))
           {
                PhotonNetwork.Destroy(gameObject);
                yield break;
            }
            InputDevice inputDevice = lobbyCharacter.IsMine ? PrivateRoomHandler.instance.devicesToUse[localID] : null;

            VersusDisplay.instance.SetPlayerSelectorGO(lobbyCharacter.uniqueID, gameObject);

            VersusDisplay.instance.TeamGroupGO(lobbyCharacter.teamID, lobbyCharacter.colorID).SetActive(true);
            VersusDisplay.instance.PlayerGO(lobbyCharacter.uniqueID).SetActive(true);
            transform.SetParent(VersusDisplay.instance.PlayerGO(lobbyCharacter.uniqueID).transform);

            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;

            buttons = transform.GetComponentsInChildren<HoverEvent>(true);
            for (int i = 0; i < buttons.Length; i++)
           {
                if (buttons[i].GetComponent<SimulatedSelection>() != null)
               {
                    UnityEngine.GameObject.DestroyImmediate(buttons[i].GetComponent<SimulatedSelection>());
                }
                buttons[i].gameObject.GetOrAddComponent<PrivateRoomSimulatedSelection>().InvokeMethod("Start");
            }

            transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = name;

            StartPicking(lobbyCharacter.uniqueID, inputDevice);

            yield break;
        }
        private void Start()
       {

            transform.GetChild(0).localPosition = Vector2.zero;

            buttons = transform.GetComponentsInChildren<HoverEvent>(true);
            for (int i = 0; i < buttons.Length; i++)
           {
                if (buttons[i].GetComponent<SimulatedSelection>() != null)
               {
                    UnityEngine.GameObject.DestroyImmediate(buttons[i].GetComponent<SimulatedSelection>());
                }
                buttons[i].gameObject.GetOrAddComponent<PrivateRoomSimulatedSelection>().InvokeMethod("Start");
            }
        }

        public void ResetMenu()
       {
            base.transform.GetChild(0).gameObject.SetActive(false);
            uniqueID = 1;

        }

        private void OnEnable()
       {

        }

        public void StartPicking(int uniqueID, InputDevice device)
       {
            uniqueID = uniqueID;
            colorID = currentPlayer.colorID;
            device = device;
            currentlySelectedFace = currentPlayer.faceID;
            if (!currentPlayer.IsMine)
           {
                view.RPC(nameof(RPCS_RequestSelectedFace), currentPlayer.networkPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            try
           {
                GetComponentInChildren<GeneralParticleSystem>(true).gameObject.SetActive(false);
                GetComponentInChildren<GeneralParticleSystem>(true).Stop();
            }
            catch{ }

            transform.GetChild(0).gameObject.SetActive(true);

            buttons = transform.GetComponentsInChildren<HoverEvent>(true);
            for (int i = 0; i < buttons.Length; i++)
           {

                buttons[i].gameObject.GetOrAddComponent<PrivateRoomSimulatedSelection>().InvokeMethod("Start");

                buttons[i].enabled = false;
                buttons[i].GetComponent<Button>().interactable = false;
                buttons[i].GetComponent<CharacterCreatorPortrait>().controlType = MenuControllerHandler.MenuControl.Controller;

                buttons[i].transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
                buttons[i].transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
                buttons[i].transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 25f;
                buttons[i].transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().autoSizeTextContainer = true;
                buttons[i].transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().overflowMode = TextOverflowModes.Ellipsis;
                buttons[i].transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().enableWordWrapping = false;
                buttons[i].transform.GetChild(3).GetChild(0).localPosition -= new Vector3(buttons[i].transform.GetChild(3).GetChild(0).localPosition.x, -25f, 0f);
                buttons[i].transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().Rebuild(CanvasUpdate.Prelayout);
                buttons[i].transform.GetChild(3).GetChild(1).gameObject.SetActive(false);

                // enabled the "LOCKED" component to reuse as info text
                buttons[i].transform.GetChild(4).gameObject.SetActive(true);
                buttons[i].transform.GetChild(4).GetChild(0).gameObject.SetActive(false);
                buttons[i].transform.GetChild(4).GetChild(1).gameObject.SetActive(false);
                buttons[i].transform.GetChild(4).GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
                buttons[i].transform.GetChild(4).GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(150f, buttons[i].transform.GetChild(4).GetChild(2).GetComponent<RectTransform>().sizeDelta.y);
                buttons[i].transform.GetChild(4).GetChild(2).GetComponent<TextMeshProUGUI>().color = Colors.joinedcolor;

                // update colors
                buttons[i].transform.GetChild(2).GetChild(0).GetComponent<SpriteRenderer>().color = ExtraPlayerSkins.GetPlayerSkinColors(currentPlayer.colorID).color;

                // update the border color and width
                buttons[i].transform.GetChild(1).GetComponent<ProceduralImage>().color = currentPlayer.IsMine ? Colors.myBorderColor : Colors.othersBorderColor;
                buttons[i].transform.GetChild(1).GetComponent<ProceduralImage>().BorderWidth = 3f;
                buttons[i].transform.GetChild(1).GetComponent<ProceduralImage>().FalloffDistance = 3f;

                if (currentPlayer.IsMine)
               {
                    // set "playerID" so that preferences will be updated when changed
                    buttons[i].GetComponentInChildren<CharacterCreatorPortrait>().playerId = currentPlayer.localID;
                }


            }

            if (transform.GetChild(0).Find("CharacterSelectButtons") != null)
           {
                GameObject go1 = transform.GetChild(0).Find("CharacterSelectButtons")?.gameObject;

                UnityEngine.GameObject.Destroy(go1);
            }

            /*
            GameObject characterSelectButtons = new GameObject("CharacterSelectButtons");
            characterSelectButtons.transform.SetParent(transform.GetChild(0));
            GameObject leftarrow = new GameObject("LeftArrow", typeof(PrivateRoomCharacterSelectButton));
            leftarrow.transform.SetParent(characterSelectButtons.transform);
            GameObject rightarrow = new GameObject("RightArrow", typeof(PrivateRoomCharacterSelectButton));
            rightarrow.transform.SetParent(characterSelectButtons.transform);

            characterSelectButtons.transform.localScale = Vector3.one;
            characterSelectButtons.transform.localPosition = Vector3.zero;

            leftarrow.transform.localScale = new Vector3(1f, 3f, 1f);
            leftarrow.transform.localPosition = new Vector3(-60f, 0f, 0f);
            leftarrow.GetComponent<PrivateRoomCharacterSelectButton>().SetCharacterSelectionInstance(this);
            leftarrow.GetComponent<PrivateRoomCharacterSelectButton>().SetDirection(PrivateRoomCharacterSelectButton.LeftRight.Left);
            rightarrow.transform.localScale = new Vector3(1f, 3f, 1f);
            rightarrow.transform.localPosition = new Vector3(60f, 0f, 0f);
            rightarrow.GetComponent<PrivateRoomCharacterSelectButton>().SetCharacterSelectionInstance(this);
            rightarrow.GetComponent<PrivateRoomCharacterSelectButton>().SetDirection(PrivateRoomCharacterSelectButton.LeftRight.Right);
            */


            // disable all the buttons, except for the currently selected one
            for (int i = 0; i < buttons.Length; i++)
           {
                if (i == currentlySelectedFace){ continue; }
                buttons[i].gameObject.SetActive(false);
            }
            buttons[currentlySelectedFace].transform.GetChild(4).gameObject.SetActive(true);
            buttons[currentlySelectedFace].gameObject.SetActive(true);
            buttons[currentlySelectedFace].GetComponent<PrivateRoomSimulatedSelection>().Select();
            if (currentPlayer.IsMine){ buttons[currentlySelectedFace].GetComponent<Button>().onClick.Invoke(); }
            buttons[currentlySelectedFace].transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().Rebuild(CanvasUpdate.Prelayout);

            StartCoroutine(FinishSetup());
        }
        private IEnumerator FinishSetup()
       {
            yield return new WaitUntil(() => this?.buttons == null || buttons[currentlySelectedFace]?.gameObject == null || buttons[currentlySelectedFace].gameObject.activeInHierarchy);
            yield return new WaitForSecondsRealtime(0.1f);
            if (this?.buttons == null || buttons[currentlySelectedFace]?.gameObject == null || currentPlayer == null)
           {
                yield break;
            }
            buttons[currentlySelectedFace].GetComponent<PrivateRoomSimulatedSelection>().Select();
            if (currentPlayer.IsMine){ buttons[currentlySelectedFace].GetComponent<Button>().onClick.Invoke(); }
            buttons[currentlySelectedFace].transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().Rebuild(CanvasUpdate.Prelayout);

            yield break;
        }
        public void UpdateReadyVisuals()
       {
            for (int i = 0; i < buttons.Length; i++)
           {
                buttons[i].transform.GetChild(4).gameObject.SetActive(true);
                buttons[i].transform.GetChild(4).GetChild(0).gameObject.SetActive(GetFieldValue<bool>("isReady") || created);
                buttons[i].transform.GetChild(4).GetChild(1).gameObject.SetActive(GetFieldValue<bool>("isReady") || created);
                foreach (Graphic graphic in buttons[i].transform.GetChild(4).GetChild(0).GetComponentsInChildren<Graphic>(true))
               {
                    graphic.color = created ? Colors.Transparent(Colors.createdColor) : GetFieldValue<bool>("isReady") ? Colors.Transparent(Colors.readycolor) : Color.clear;
                }
                foreach (Graphic graphic in buttons[i].transform.GetChild(4).GetChild(1).GetComponentsInChildren<Graphic>(true))
               {
                    graphic.color = created ? Colors.Transparent(Colors.createdColor) : GetFieldValue<bool>("isReady") ? Colors.Transparent(Colors.readycolor) : Color.clear;
                }
                buttons[i].transform.GetChild(4).GetChild(2).GetComponent<TextMeshProUGUI>().text = created ? "IN GAME" : GetFieldValue<bool>("isReady") ? "READY" : "";
                buttons[i].transform.GetChild(4).GetChild(2).GetComponent<TextMeshProUGUI>().color = created ? Colors.createdColor : GetFieldValue<bool>("isReady") ? Colors.readycolor : Colors.joinedcolor;
            }
        }
        public void Created()
       {
            created = true;
            if (currentPlayer.IsMine){ view.RPC(nameof(RPCA_Created), RpcTarget.All); }
        }

        [PunRPC]
        public void RPCA_Created()
       {
            created = true;
        }
        private void Update()
       {
            if (PrivateRoomHandler.instance == null || PhotonNetwork.CurrentRoom == null || currentPlayer == null)
           {
                return;
            }

            UpdateReadyVisuals();

            if (!currentPlayer.IsMine)
           {
                colorID = currentPlayer.colorID;
                UpdateFaceColors();
                return;
            }
            else if (lastChangedTeams > 0f && Time.realtimeSinceStartup - lastChangedTeams >= 2f * PhotonNetwork.GetPing()*0.001f)
           {
                if (colorID != currentPlayer.colorID)
               {
                    ChangeToTeam(colorID);
                }
            }
            if (!currentPlayer.IsMine || !enableInput || GetFieldValue<bool>("isReady")){ return; }

            HoverEvent component = buttons[currentlySelectedFace].GetComponent<HoverEvent>();
            if (currentButton != component)
           {
                if (currentButton)
               {
                    currentButton.GetComponent<PrivateRoomSimulatedSelection>().Deselect();
                    currentButton.gameObject.SetActive(false);
                }
                else
               {
                    for (int i = 0; i < buttons.Length; i++)
                   {
                        if (i == currentlySelectedFace){ continue; }
                        buttons[i].GetComponent<PrivateRoomSimulatedSelection>().Deselect();
                        buttons[i].gameObject.SetActive(false);
                    }
                }
                currentButton = component;
                currentButton.transform.GetChild(4).gameObject.SetActive(true);
                currentButton.gameObject.SetActive(true);
                currentButton.GetComponent<PrivateRoomSimulatedSelection>().Select();
                if (currentPlayer.IsMine){ currentButton.GetComponent<Button>().onClick.Invoke(); }
                currentButton.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().Rebuild(CanvasUpdate.Prelayout);
            }
            int previouslySelectedFace = currentlySelectedFace;
            if (((device != null && (device.DeviceClass == InputDeviceClass.Controller) && (device.Direction.Left.WasPressed || device.Direction.Right.WasPressed || device.DPadLeft.WasPressed || device.DPadRight.WasPressed|| device.RightBumper.WasPressed || device.LeftBumper.WasPressed)) || (device == null && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow)))))
           {
                // change face
                if ((device != null && (device.DeviceClass == InputDeviceClass.Controller) && device.RightBumper.WasPressed) || (device == null && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.UpArrow))))
               {
                    currentlySelectedFace++;
                }
                else if ((device != null && (device.DeviceClass == InputDeviceClass.Controller) && device.LeftBumper.WasPressed) || (device == null && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.DownArrow))))
               {
                    currentlySelectedFace--;
                }
                bool colorChanged = false;
                int colorIDDelta = 0;
                // change team
                if (device != null && ((device.DeviceClass == InputDeviceClass.Controller) && (device.Direction.Right.WasPressed || device.DPadRight.WasPressed)) || ((device == null) && (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))))
               {
                    colorIDDelta = +1;
                    colorChanged = true;
                }
                else if (device != null && ((device.DeviceClass == InputDeviceClass.Controller) && (device.Direction.Left.WasPressed || device.DPadLeft.WasPressed)) || ((device == null) && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))))
               {
                    colorIDDelta = -1;
                    colorChanged = true;
                }

                if (colorChanged)
               {
                    // ask the host client for permission to change team
                    //view.RPC(nameof(RPCH_RequestChangeTeam), RpcTarget.MasterClient, colorIDDelta);
                    ChangeTeam(colorIDDelta);
                }

            }
            currentlySelectedFace %= buttons.Length;
            if (currentlySelectedFace != previouslySelectedFace)
           {
                currentPlayer.faceID = currentlySelectedFace;
                LobbyCharacter[] characters = PhotonNetwork.LocalPlayer.GetProperty<LobbyCharacter[]>("players");
                characters[currentPlayer.localID] = currentPlayer;
                PhotonNetwork.LocalPlayer.SetProperty("players", characters);
                PlayerFace faceToSend = CharacterCreatorHandler.instance.GetFacePreset(currentlySelectedFace);
                view.RPC(nameof(RPCO_SelectFace), RpcTarget.Others, currentlySelectedFace, faceToSend.eyeID, faceToSend.eyeOffset, faceToSend.mouthID, faceToSend.mouthOffset, faceToSend.detailID, faceToSend.detailOffset, faceToSend.detail2ID, faceToSend.detail2Offset);
            }
        }
        public void SetInputEnabled(bool enabled)
       {
            enableInput = enabled;
        }
        
        [PunRPC]
        private void RPCS_RequestSelectedFace(int askerID)
       {
            PlayerFace faceToSend = CharacterCreatorHandler.instance.GetFacePreset(currentlySelectedFace);
            view.RPC(nameof(RPCO_SelectFace), PhotonNetwork.CurrentRoom.GetPlayer(askerID), currentlySelectedFace, faceToSend.eyeID, faceToSend.eyeOffset, faceToSend.mouthID, faceToSend.mouthOffset, faceToSend.detailID, faceToSend.detailOffset, faceToSend.detail2ID, faceToSend.detail2Offset);
        }
        [PunRPC]
        private void RPCO_SelectFace(int faceID, int eyeID, Vector2 eyeOffset, int mouthID, Vector2 mouthOffset, int detailID, Vector2 detailOffset, int detail2ID, Vector2 detail2Offset)
       {
            currentPlayer.faceID = faceID;
            buttons = transform.GetComponentsInChildren<HoverEvent>(true);
            for (int i = 0; i < buttons.Length; i++)
           {
                if (i == faceID)
               {
                    buttons[i].gameObject.SetActive(true);
                    StartCoroutine(SelectFaceCoroutine(buttons[i], eyeID, eyeOffset, mouthID, mouthOffset, detailID, detailOffset, detail2ID, detail2Offset));
                }
                else
               {
                    buttons[i].GetComponent<PrivateRoomSimulatedSelection>().Deselect();
                    buttons[i].gameObject.SetActive(false);
                }
            }
        }
        private IEnumerator SelectFaceCoroutine(HoverEvent button, int eyeID, Vector2 eyeOffset, int mouthID, Vector2 mouthOffset, int detailID, Vector2 detailOffset, int detail2ID, Vector2 detail2Offset)
       {
            yield return new WaitUntil(() => button.gameObject.activeInHierarchy);

            button.GetComponent<PrivateRoomSimulatedSelection>().Select();
            button.transform.GetChild(1).gameObject.SetActive(true);
            button.transform.GetChild(4).gameObject.SetActive(true);
            button.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().Rebuild(CanvasUpdate.Prelayout);
            button.GetComponent<CharacterCreatorItemEquipper>().RPCA_SetFace(eyeID, eyeOffset, mouthID, mouthOffset, detailID, detailOffset, detail2ID, detail2Offset);
            

            yield break;
        }

        private void ChangeTeam(int colorIDDelta)
       {
            int newColorID = colorID + colorIDDelta;
            int orig = colorID;

            if (!GameModeManager.CurrentHandler.AllowTeams)
           {
                // teams not allowed, continue to next colorID
                while (PrivateRoomHandler.instance.PrivateRoomCharacters.Where(p => p != null && p.uniqueID != currentPlayer.uniqueID && p.colorID == newColorID).Any() && newColorID < UnboundNetworking.MaxColorsHardLimit && newColorID >= 0)
               {
                    newColorID = newColorID + colorIDDelta;
                    if (newColorID == orig || newColorID >= UnboundNetworking.MaxColorsHardLimit || newColorID < 0)
                   {
                        // make sure it's impossible to get stuck in an infinite loop here,
                        // even though prior logic limiting the number of players should prevent this
                        break;
                    }
                }
            }

            bool fail = newColorID == orig || newColorID >= UnboundNetworking.MaxColorsHardLimit || newColorID < 0;

            if (!fail)
           {
                ChangeToTeam(newColorID);
            }
        }

        private void ChangeToTeam(int newColorID)
       {
            // send the team change to all clients
            if (!currentPlayer.IsMine){ return; }

            lastChangedTeams = Time.realtimeSinceStartup;

            colorID = newColorID;

            LobbyCharacter character = PrivateRoomHandler.instance.FindLobbyCharacter(currentPlayer.uniqueID);
            character.colorID = newColorID;
            LobbyCharacter[] characters = PhotonNetwork.LocalPlayer.GetProperty<LobbyCharacter[]>("players");
            characters[character.localID] = character;
            PhotonNetwork.LocalPlayer.SetProperty("players", characters);

            UpdateFaceColors();

        }

        public void UpdateFaceColors()
       {
            // set player color
            if (transform.GetComponentsInChildren<HoverEvent>(true).Any())
           {
                buttons = transform.GetComponentsInChildren<HoverEvent>(true);
                for (int i = 0; i < buttons.Length; i++)
               {
                    buttons[i].transform.GetChild(2).GetChild(0).GetComponent<SpriteRenderer>().color = ExtraPlayerSkins.GetPlayerSkinColors(colorID).color;
                }
            }
        }

        [PunRPC]
        internal void RPCA_ChangeTeam(int newColorID)
       {
            if (currentPlayer.IsMine){ ChangeToTeam(newColorID); }

            return;

        }

        public int currentlySelectedFace;

        public LobbyCharacter currentPlayer => PrivateRoomHandler.instance.FindLobbyCharacter(uniqueID);

        public int uniqueID = 1;
        private int _colorID = -1;
        public int colorID
       {
            get
           {
                return _colorID;
            }
            private set
           {
                _colorID = value;
            }
        }

        public InputDevice device;

        public GameObject getReadyObj;

        private HoverEvent currentButton;

        private HoverEvent[] buttons;

        public bool isReady
       {
            get
           {
                if (currentPlayer != null)
               {
                    return currentPlayer.ready;
                }
                else
               {
                    return false;
                }
            }
        }

        private bool created = false;

        private float lastChangedTeams = -1f;

        private bool enableInput = true;
    }

}
