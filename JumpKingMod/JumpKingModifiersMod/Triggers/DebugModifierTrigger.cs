﻿using JumpKingModifiersMod.API;
using Microsoft.Xna.Framework.Input;
using PBJKModBase.API;
using PBJKModBase.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Triggers
{
    /// <summary>
    /// An implementation of <see cref="IModifierTrigger"/> and <see cref="IModEntity"/> which
    /// listens for keyboard input within the Monogame application as an entity to toggle a provided 
    /// modifier
    /// </summary>
    public class DebugModifierTrigger : IModifierTrigger, IModEntity
    {
        private IModifier modifier;
        private bool pressedCooldown;
        private bool isTriggerActive;

        /// <summary>
        /// Ctor for creating a <see cref="DebugModifierTrigger"/>
        /// </summary>
        /// <param name="modEntityManager">The <see cref="ModEntityManager"/> to register itself to</param>
        /// <param name="modifier">The <see cref="IModifier"/> implementation to toggle</param>
        public DebugModifierTrigger(ModEntityManager modEntityManager, IModifier modifier)
        {
            this.modifier = modifier ?? throw new ArgumentNullException(nameof(modifier));

            pressedCooldown = false;
            isTriggerActive = false;
            modEntityManager.AddEntity(this);
        }

        /// <inheritdoc/>
        public bool DisableTrigger()
        {
            isTriggerActive = false;
            return true;
        }

        /// <inheritdoc/>
        public bool EnableTrigger()
        {
            isTriggerActive = true;
            return true;
        }

        /// <inheritdoc/>
        public bool IsTriggerEnabled()
        {
            return isTriggerActive;
        }

        /// <summary>
        /// Checks for keyboard input each frame, and toggles the modifier. Does nothing if the trigger is not active
        /// </summary>
        public void Update(float p_delta)
        {
            if (!isTriggerActive)
            {
                return;
            }

            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.L))
            {
                if (!pressedCooldown)
                {
                    pressedCooldown = true;
                    if (modifier.IsModifierEnabled())
                    {
                        modifier.DisableModifier();
                    }
                    else
                    {
                        modifier.EnableModifier();
                    }
                }
            }
            else
            {
                pressedCooldown = false;
            }
        }

        /// <inheritdoc/>
        public void Draw()
        {
            // Do nothing
        }
    }
}
