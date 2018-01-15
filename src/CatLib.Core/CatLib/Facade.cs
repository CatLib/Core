/*
 * This file is part of the CatLib package.
 *
 * (c) Yu Bin <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: http://catlib.io/
 */

namespace CatLib
{
    /// <summary>
    /// 门面
    /// </summary>
    public abstract class Facade<TService>
    {
        /// <summary>
        /// 实例
        /// </summary>
        private static TService instance;

        /// <summary>
        /// 绑定数据
        /// </summary>
        private static IBindable binder;

        /// <summary>
        /// 是否已经被初始化
        /// </summary>
        private static bool inited;

        /// <summary>
        /// 门面静态构造
        /// </summary>
        static Facade()
        {
            if (!typeof(TService).IsInterface)
            {
                throw new RuntimeException("Facade<" + typeof(TService).Name + "> , generic type must be interface.");
            }

            App.OnNewApplication += app =>
            {
                instance = default(TService);
                binder = null;
                inited = false;
            };
        }

        /// <summary>
        /// 门面实例
        /// </summary>
        public static TService Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                return Make();
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private static TService Make()
        {
            if (!inited)
            {
                App.Watch<TService>(ServiceRebound);
                inited = true;
            }

            var newBinder = App.GetBind<TService>();
            if (newBinder == null || !newBinder.IsStatic)
            {
                return App.Make<TService>();
            }

            if (binder == null || binder != newBinder)
            {
                newBinder.OnRelease((_, __) => instance = default(TService));
                binder = newBinder;
            }

            return instance = App.Make<TService>();
        }

        /// <summary>
        /// 当服务被重绑定时
        /// </summary>
        /// <param name="service">新的服务实例</param>
        private static void ServiceRebound(TService service)
        {
            var newBinder = App.GetBind<TService>();
            if (newBinder == null || !newBinder.IsStatic)
            {
                instance = default(TService);
                return;
            }

            instance = service;
        }
    }
}