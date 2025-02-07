﻿using System;

namespace PBJKModBase
{
    public abstract class JumpKingModUtilities
    {
        /// <summary>
        /// Attempts to perform multiple actions in a row, returning on the first successful one.
        /// Catches any exceptions and returns them
        /// </summary>
        /// <param name="onAllActionsFail">An action invoked when all of the provided actions threw exceptions</param>
        /// <param name="actions">An array of tuples of actions to execute in order. If the actions throws an exception the second Action will be invoked</param>
        public static void AttemptMultipleActions(Action onAllActionsFail, params Tuple<Action, Action<Exception>>[] actions)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                try
                {
                    actions[i].Item1.Invoke();

                    return;
                }
                catch (Exception e)
                {
                    actions[i].Item2.Invoke(e);
                }
            }

            onAllActionsFail.Invoke();
        }
    }
}
