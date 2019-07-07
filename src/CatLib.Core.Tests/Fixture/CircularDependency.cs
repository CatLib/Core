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

namespace CatLib.Tests.Fixture
{
    public class CircularDependency
    {
        public CircularDependency(CircularDependency dependency)
        {
            // This is a class that causes a circular
            // dependency call to occur in DI.
        }

        public virtual object Foo(CircularDependency dependency)
        {
            return "foo";
        }
    }
}
