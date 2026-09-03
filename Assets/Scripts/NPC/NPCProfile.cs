using UnityEngine;

namespace GenesisWorld.NPC
{
    /// <summary>
    /// 与场景对象分离的 NPC 身份数据。未来可在不修改 NPCActor 的情况下扩展 AI 上下文。
    /// </summary>
    [CreateAssetMenu(fileName = "NPCProfile", menuName = "GenesisWorld/NPC Profile")]
    public sealed class NPCProfile : ScriptableObject
    {
        private const string UnknownId = "npc_unknown";
        private const string UnknownName = "Unknown NPC";
        private const string DefaultGreeting = "Hello, traveler.";

        [Header("Identity")]
        [SerializeField] private string npcId = UnknownId;
        [SerializeField] private string displayName = UnknownName;
        [SerializeField] private string role = "Resident";

        [Header("Local Dialogue")]
        [SerializeField, TextArea(2, 5)] private string description;
        [SerializeField, TextArea(2, 5)] private string greeting = DefaultGreeting;

        public string NpcId => string.IsNullOrWhiteSpace(npcId) ? UnknownId : npcId.Trim();
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? UnknownName : displayName.Trim();
        public string Role => string.IsNullOrWhiteSpace(role) ? "Resident" : role.Trim();
        public string Description => description ?? string.Empty;
        public string Greeting => string.IsNullOrWhiteSpace(greeting) ? DefaultGreeting : greeting.Trim();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(npcId))
            {
                Debug.LogWarning($"NPC Profile '{name}' should define a stable NPC Id.", this);
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                Debug.LogWarning($"NPC Profile '{name}' should define a Display Name.", this);
            }
        }
#endif
    }
}
