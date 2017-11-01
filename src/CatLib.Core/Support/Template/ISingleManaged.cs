
namespace CatLib
{
    /// <summary>
    /// 管理器模版（拓展解决方案为单例）- 扩展内容不对外可见
    /// </summary>
    public interface ISingleManaged<TInterface> : IManaged<TInterface>
    {
        /// <summary>
        /// 释放解决方案单例
        /// </summary>
        /// <param name="name">解决方案名</param>
        void Release(string name = null);
    }
}
