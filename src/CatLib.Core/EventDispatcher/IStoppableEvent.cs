/*
 * This file is part of the CatLib package.
 *
 * (c) CatLib <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: https://catlib.io/
 */

namespace CatLib.EventDispatcher
{
    /// <summary>
    /// An event whose processing may be interrupted when the event has been handled.
    /// </summary>
    public interface IStoppableEvent
    {
        /// <summary>
        /// Whether propagation stopped.
        /// </summary>
        /// <returns>True if the propagation stopped.</returns>
        bool IsPropagationStopped();
    }
}
