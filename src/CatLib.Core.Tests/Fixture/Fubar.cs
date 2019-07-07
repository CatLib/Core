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
    public class Fubar
    {
        public Bar Bar { get; private set; }

        [Inject(Required = false)]
        public Position Position { get; private set;}

        [Inject(Required = false)]
        public IList<int> Ages { get; set; }

        public IList<string> Heros { get; set; }

        public Fubar(Bar bar = null, IList<string> heros = null)
        {
            Bar = bar;
            Heros = heros;
        }
    }
}
