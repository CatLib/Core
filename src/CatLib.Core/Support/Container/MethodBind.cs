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

using System.Reflection;

namespace CatLib
{
    /// <summary>
    /// The default method binding data implementation.
    /// </summary>
    internal sealed class MethodBind : Bindable<IMethodBind>, IMethodBind
    {
        /// <summary>
        /// The <see cref="MethodContainer"/> instance.
        /// </summary>
        private readonly MethodContainer methodContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodBind"/> class.
        /// </summary>
        /// <param name="methodContainer">The <see cref="MethodContainer"/> instance.</param>
        /// <param name="container">The <see cref="Container"/> instance.</param>
        /// <param name="service">The service name.</param>
        /// <param name="target">The instance on which to call the method.</param>
        /// <param name="call">The method to called.</param>
        public MethodBind(MethodContainer methodContainer, Container container, string service, object target, MethodInfo call)
            : base(container, service)
        {
            this.methodContainer = methodContainer;
            Target = target;
            MethodInfo = call;
            ParameterInfos = call.GetParameters();
        }

        /// <summary>
        /// Gets the method info.
        /// </summary>
        public MethodInfo MethodInfo { get; }

        /// <summary>
        /// Gets the instance on which to call the method.
        /// </summary>
        public object Target { get; }

        /// <summary>
        /// Gets an array of the method parameters.
        /// </summary>
        public ParameterInfo[] ParameterInfos { get; }

        /// <summary>
        /// Unbinds a method from the container.
        /// </summary>
        protected override void ReleaseBind()
        {
            methodContainer.Unbind(this);
        }
    }
}
