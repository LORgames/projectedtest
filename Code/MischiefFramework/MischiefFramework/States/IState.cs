using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MischiefFramework.States {
    internal interface IState {
        /// <summary>
        /// Updates the State..?
        /// </summary>
        /// <param name="gameTime">The time snapshot</param>
        /// <returns>True if the next later needs to be updated as well, false otherwise</returns>
        bool Update(GameTime gameTime);

        /// <summary>
        /// Renders the State to screen probably..?
        /// </summary>
        /// <param name="gameTime">The time snapshot</param>
        /// <returns>True if the next layer needs to be rendered as well, false otherwise</returns>
        bool Render(GameTime gameTime);

        /// <summary>
        /// Called when this State is removed from the stack.
        /// </summary>
        /// <returns>True if its safe to remove it (99% of the time) and False if it cannot be removed</returns>
        bool OnRemove();
    }
}
