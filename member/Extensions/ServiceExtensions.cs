using System.Reflection; // 必須引用反射命名空間

namespace member.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// 使用 Reflection 自動掃描並註冊 Services
        /// </summary>
        public static void AddBusinessServices(this IServiceCollection services)
        {
            // 1. 取得目前執行的 Assembly (專案)
            // 如果你的 Service 跟這個 Extension 在同一個專案，用 GetExecutingAssembly 即可
            var assembly = Assembly.GetExecutingAssembly();

            // 2. 找出所有符合條件的類別
            var serviceTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract) // 必須是類別，且不是抽象類別
                .Where(t => t.Namespace == "member.Services") // 指定命名空間 (請確認你的 Namespace 是這個)
                                                              // .Where(t => t.Name.EndsWith("Service")) // (選用) 如果你想更嚴謹，可以限制檔名結尾
                .ToList();

            // 3. 迴圈自動註冊
            foreach (var type in serviceTypes)
            {
                // 相當於 services.AddScoped<UserService>();
                services.AddScoped(type);
            }
        }
    }
}