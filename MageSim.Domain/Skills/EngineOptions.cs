namespace MageSim.Domain.Skills
{
    /// <summary>
    /// RotationEngine için opsiyonel ayarlar.
    /// </summary>
    public sealed class EngineOptions
    {
        /// <summary>
        /// Rotasyon hızını çarpan ile ayarlamak için.
        /// Örn: 2.0 → iki kat hızlı tick.
        /// </summary>
        public double SpeedMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Hata toleransı aktif mi?
        /// </summary>
        public bool ErrorTolerance { get; set; } = false;

        /// <summary>
        /// Ayrıntılı loglama aktif mi?
        /// </summary>
        public bool VerboseLogging { get; set; } = false;
    }
}
