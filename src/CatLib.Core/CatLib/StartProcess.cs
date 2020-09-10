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

namespace CatLib
{
    /// <summary>
    /// The framework start process type.
    /// </summary>
    public enum StartProcess
    {
        /// <summary>
        /// When you create a new <see cref="Application"/>,
        /// you are in the <see cref="Construct"/> phase.
        /// </summary>
        Construct = 0,

        /// <summary>
        /// Before the <see cref="Application.BootstrapWith"/> call.
        /// </summary>
        Bootstrap = 1,

        /// <summary>
        /// When call the <see cref="Application.Boot"/> method.
        /// </summary>
        Boot = 2,

        /// <summary>
        /// When the framework running.
        /// </summary>
        Running = 3,

        /// <summary>
        /// When call the <see cref="Application.Terminate"/>.
        /// </summary>
        Terminate = 4,

        /// <summary>
        /// When end of the terminate.
        /// </summary>
        Terminated = 5,
    }
}
