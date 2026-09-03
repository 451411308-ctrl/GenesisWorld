using UnityEngine;

namespace GenesisWorld.Interaction
{
    /// <summary>可被玩家视角交互的最小契约。</summary>
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        Transform InteractionTransform { get; }
        bool CanInteract { get; }

        void Interact(PlayerInteractionController interactor);
    }
}
