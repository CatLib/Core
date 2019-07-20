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
        /// Before the <see cref="Application.Bootstrap"/> call.
        /// </summary>
        Bootstrap = 1,

        /// <summary>
        /// When during <see cref="Application.Bootstrap"/> execution,
        /// you are in the <see cref="Bootstrapping"/> phase.
        /// </summary>
        Bootstrapping = 2,

        /// <summary>
        /// After the <see cref="Application.Bootstrap"/> called.
        /// </summary>
        Bootstraped = 3,

        /// <summary>
        /// Before the <see cref="Application.Init"/> call.
        /// </summary>
        Init = 4,

        /// <summary>
        /// When during <see cref="Application.Init"/> execution,
        /// you are in the <see cref="Initing"/> phase.
        /// </summary>
        Initing = 5,

        /// <summary>
        /// After the <see cref="Application.Init"/> called.
        /// </summary>
        Inited = 6,

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
