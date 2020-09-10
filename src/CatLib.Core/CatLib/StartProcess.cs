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
        Boot = 4,

        /// <summary>
        /// When the framework running.
        /// </summary>
        Running = 7,

        /// <summary>
        /// Before the <see cref="Application.Terminate"/> call.
        /// </summary>
        Terminate = 8,

        /// <summary>
        /// When during <see cref="Application.Terminate"/> execution,
        /// you are in the <see cref="Terminating"/> phase.
        /// </summary>
        Terminating = 9,

        /// <summary>
        /// After the <see cref="Application.Terminate"/> called.
        /// All resources are destroyed.
        /// </summary>
        Terminated = 10,
    }
}
