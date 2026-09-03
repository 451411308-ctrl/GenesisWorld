using GenesisWorld.NPC;
using GenesisWorld.Player;
using TMPro;
using UnityEngine;

namespace GenesisWorld.UI
{
    /// <summary>管理基础对话 UI、当前 NPC 与玩家输入锁，不负责产生 AI 回复。</summary>
    [DisallowMultipleComponent]
    public sealed class DialogueController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private GameObject dialoguePanel;

        [Header("Text")]
        [SerializeField] private TMP_Text interactionPromptText;
        [SerializeField] private TMP_Text npcNameText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text hintText;

        public bool IsOpen { get; private set; }
        public NPCActor CurrentNpc { get; private set; }
        public TMP_Text InteractionPromptText => interactionPromptText;
        public string DisplayedNpcName => npcNameText != null ? npcNameText.text : string.Empty;
        public string DisplayedMessage => messageText != null ? messageText.text : string.Empty;

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
            }

            CloseDialogue(false);
            HideInteractionPrompt();
        }

        public void OpenDialogue(NPCActor npc)
        {
            if (npc == null)
            {
                Debug.LogWarning("DialogueController cannot open a dialogue without an NPCActor.", this);
                return;
            }

            CurrentNpc = npc;
            IsOpen = true;

            if (npcNameText != null)
            {
                npcNameText.text = $"{npc.DisplayName}  ·  {npc.Role}";
            }

            DisplayMessage(npc.Greeting);

            if (hintText != null)
            {
                hintText.text = "Press E or Esc to close";
            }

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            HideInteractionPrompt();
            playerController?.SetInputEnabled(false);
        }

        public void DisplayMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = string.IsNullOrWhiteSpace(message) ? "..." : message;
            }
        }

        public void CloseDialogue()
        {
            CloseDialogue(true);
        }

        public void ShowInteractionPrompt(string prompt)
        {
            if (interactionPromptText == null || IsOpen)
            {
                return;
            }

            interactionPromptText.text = prompt ?? string.Empty;
            interactionPromptText.gameObject.SetActive(!string.IsNullOrWhiteSpace(prompt));
        }

        public void HideInteractionPrompt()
        {
            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }

        private void CloseDialogue(bool restoreInput)
        {
            IsOpen = false;
            CurrentNpc = null;

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            if (restoreInput)
            {
                playerController?.SetInputEnabled(true);
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                CloseDialogue(true);
            }
        }
    }
}
