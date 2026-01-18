namespace Mks.Iot.I2c.Devices.Pca9538;

/// <summary>
/// Registers for the PCA9538 I/O Expander.
/// </summary>
public enum MksPca9538Register : byte
{
    /// <summary>
    /// Input Port Register.
    /// Reflects the incoming logic levels of the pins.
    /// </summary>
    InputPort = 0x00,

    /// <summary>
    /// Output Port Register.
    /// Shows the outgoing logic levels of the pins defined as outputs.
    /// </summary>
    OutputPort = 0x01,

    /// <summary>
    /// Polarity Inversion Register.
    /// Allows the user to invert the polarity of the Input Port register data.
    /// </summary>
    PolarityInversion = 0x02,

    /// <summary>
    /// Configuration Register.
    /// Configures the direction of the I/O pins.
    /// 1 = Input (High-Z), 0 = Output.
    /// </summary>
    Configuration = 0x03
}
