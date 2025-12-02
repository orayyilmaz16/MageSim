
namespace MageSim.Domain.Events
{
    public enum CombatEventType
    {
        Cast = 0,
        StateChange = 1,
        Warning = 2,
        Action = 3,   // Skill kullanıldığında
        Info = 4,     // Genel bilgi/Idle gibi durumlarda
        Error = 5     // Hata yakalama/log için opsiyonel
    }
}