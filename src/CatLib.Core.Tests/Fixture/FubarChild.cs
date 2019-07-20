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

using CatLib.Container;
using System.Collections.Generic;

namespace CatLib.Tests.Fixture
{
    public class FubarChild : Fubar
    {
        public FubarChild(Bar bar = null, IList<string> heros = null)
            : base(bar, heros)
        {
        }

        [Inject(Required = false)]
        public Foo Foo { get; set; }
    }
}
